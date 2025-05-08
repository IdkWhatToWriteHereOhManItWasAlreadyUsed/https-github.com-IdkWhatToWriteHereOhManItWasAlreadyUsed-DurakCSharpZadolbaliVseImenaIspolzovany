using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

public class MessageFilter
{
    public Func<string, bool> Predicate { get; }
    public Func<string, Task> Handler { get; }

    public MessageFilter(Func<string, bool> predicate, Func<string, Task> handler)
    {
        Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        Handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }
}

public class FilterAwaiter
{
    public Func<string, bool> Predicate { get; }
    public TaskCompletionSource<string> CompletionSource { get; }
    public CancellationTokenSource CancellationTokenSource { get; }

    public FilterAwaiter(Func<string, bool> predicate, int timeoutMilliseconds)
    {
        Predicate = predicate;
        CompletionSource = new TaskCompletionSource<string>();
        CancellationTokenSource = new CancellationTokenSource(timeoutMilliseconds);
        CancellationTokenSource.Token.Register(() => CompletionSource.TrySetCanceled());
    }
}

public class NetworkClient : IDisposable
{
    private readonly ConcurrentBag<MessageFilter> _messageFilters = new ConcurrentBag<MessageFilter>();
    private readonly ConcurrentDictionary<Guid, FilterAwaiter> _messageAwaiters = new ConcurrentDictionary<Guid, FilterAwaiter>();
    private TcpClient _client;
    private NetworkStream _stream;
    private readonly string _serverAddress;
    private readonly int _serverPort;
    public string _clientId;
    private string _clientName;
    private string _clientAddress;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isInitialized;

    public NetworkClient(string clientName, string serverAddress, int port)
    {
        _clientName = clientName;
        _serverAddress = serverAddress;
        _serverPort = port;
        _cancellationTokenSource = new CancellationTokenSource();
        _ = ConnectAsync();
    }

    public async Task<string> WaitForMessageAsync(Func<string, bool> predicate, int timeoutMilliseconds = 5000)
    {
        var awaiter = new FilterAwaiter(predicate, timeoutMilliseconds);
        var awaiterId = Guid.NewGuid();

        _messageAwaiters.TryAdd(awaiterId, awaiter);

        try
        {
            return await awaiter.CompletionSource.Task;
        }
        finally
        {
            _messageAwaiters.TryRemove(awaiterId, out _);
            awaiter.CancellationTokenSource.Dispose();
        }
    }

    private async Task ProcessReceivedMessage(string message)
    {
      //  Console.WriteLine(message);
        foreach (var (id, awaiter) in _messageAwaiters)
        {
            if (awaiter.Predicate(message))
            {
                if (_messageAwaiters.TryRemove(id, out _))
                {
                  //  Console.WriteLine("Handled by awaiter");
                    awaiter.CompletionSource.TrySetResult(message);
                    return;
                }
            }
        }

        bool handled = false;
      //  Console.WriteLine("Handled by filter");
        foreach (var filter in _messageFilters)
        {
            if (filter.Predicate(message))
            {
                handled = true;
                try
                {
                    await filter.Handler(message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка в обработчике сообщения: {ex.Message}");
                }
            }
        }

        if (!handled)
        {
         //   MessageBox.Show($"Необработанное сообщение: {message}");
        }
    }

    public void AddMessageFilter(Func<string, bool> predicate, Func<string, Task> handler)
    {
        _messageFilters.Add(new MessageFilter(predicate, handler));
    }

    public void RemoveAllFilters()
    {
        while (!_messageFilters.IsEmpty)
        {
            _messageFilters.TryTake(out _);
        }
    }

    public async Task ConnectAsync()
    {
        try
        {
            _client = new TcpClient(new IPEndPoint(GetLocalIpAddress(), 6666));
            await _client.ConnectAsync(_serverAddress, _serverPort);
            _stream = _client.GetStream();
            _isInitialized = true;

            _clientAddress = ((IPEndPoint)_client.Client.LocalEndPoint).Address.ToString();
            _clientId = ComputeClientId(_clientAddress);

            _ = Task.Run(() => ReceiveMessagesAsync(_cancellationTokenSource.Token));

            await SendRawMessageAsync($"{_clientName}:SERVER:REGISTER");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка подключения: {ex.Message}");
        }
    }

    public static IPAddress GetLocalIpAddress()
    {
        byte[] addressBytes = new byte[4];
        Random rand = new Random();
        addressBytes[0] = 127;
        addressBytes[1] = 0;
        addressBytes[2] = 0;
        addressBytes[3] = (byte)rand.Next(2, 255);
        return new IPAddress(addressBytes);
    }

    private string ComputeClientId(string address)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(address));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 4; i++) // Берем первые 4 байта для краткости
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    public async Task SendMessageAsync(string recipientId, string message)
    {
        try
        {
            if (!_isInitialized || !_client.Connected) return;
            await SendRawMessageAsync($"{_clientId}:{recipientId}:{message}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка отправки: {ex.Message}");
        }
    }

    private async Task SendRawMessageAsync(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        await _stream.WriteAsync(data, 0, data.Length);
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _client.Connected)
        {
            try
            {
                byte[] buffer = new byte[432];
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == 0) break;

                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                _ =  ProcessReceivedMessage(receivedMessage);
               // Console.WriteLine(receivedMessage);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения: {ex.Message}");
                break;
            }
        }
    }

    public void Disconnect()
    {
        _cancellationTokenSource.Cancel();
        _stream?.Close();
        _client?.Close();
        _isInitialized = false;
    }

    public void Dispose()
    {
        Disconnect();
        _cancellationTokenSource.Dispose();
    }
}