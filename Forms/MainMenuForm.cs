using System;
using System.Media;
using Durak_.Forms;
using Button = System.Windows.Forms.Button;
using ProgressBar = System.Windows.Forms.ProgressBar;

namespace Durak_
{
    public partial class MainMenuForm : Form
    {
        private CancellationTokenSource cts;
        public MainMenuForm()
        {
            InitializeComponent();
            PlayersGrid.Columns["Players"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            PlayersGrid.AutoGenerateColumns = false;
            PlayersGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            PlayersGrid.AllowUserToAddRows = false;
            PlayersGrid.ReadOnly = true;

            PlayersGrid.Columns.Clear();

            PlayersGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "Имя клиента",
                Name = "colName"
            });

            PlayersGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Id",
                HeaderText = "ID",
                Name = "colId"
            });

            PlayersGrid.Columns.Add(new DataGridViewButtonColumn
            {
                HeaderText = "Действие",
                Name = "colAction",
                Text = "Подключиться",
                UseColumnTextForButtonValue = true
            });
        }

        private NetworkClient _networkClient;

        private void MainMenuForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            _networkClient?.Dispose();
            _networkClient = new NetworkClient(NameTextBox.Text, "127.0.0.1", 8888);
            _networkClient.AddMessageFilter(msg => msg.StartsWith("SERVER:CLIENTS"), UpdatePlayersGrid);
            _networkClient.AddMessageFilter(msg => msg.Contains("GO?"), ShowGameRequestWindow);
        }

        private async Task ShowGameRequestWindow(string message)
        {
            string[] parts = message.Split(':');
            if (parts.Length < 2)
            {
                MessageBox.Show("Некорректный формат сообщения.");
                return;
            }

            string senderId = parts[0];
            string receiverId = parts[1];
            DataGridViewRow senderRow = PlayersGrid.Rows.Cast<DataGridViewRow>().FirstOrDefault(row => row.Cells["colId"].Value?.ToString() == senderId);

            if (senderRow == null)
            {
                return;
            }

            string senderName = senderRow.Cells["colName"].Value?.ToString() ?? "Неизвестный";

            var requestForm = new Form()
            {
                Width = 300,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Приглашение в игру",
                MinimizeBox = false,
                MaximizeBox = false
            };

            var label = new Label()
            {
                Text = $"{senderName} приглашает вас в игру. Принять?",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 50
            };

            var progressBar = new ProgressBar()
            {
                Maximum = 100,
                Minimum = 0,
                Value = 100,
                Dock = DockStyle.Top,
                Height = 20
            };

            var buttonYes = new Button()
            {
                Text = "Да",
                DialogResult = DialogResult.Yes,
                Width = 75
            };
            var buttonNo = new Button()
            {
                Text = "Нет",
                DialogResult = DialogResult.No,
                Width = 75
            };

            var buttonPanel = new FlowLayoutPanel()
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.LeftToRight,
                Height = 50,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Anchor = AnchorStyles.None,
            };

            buttonPanel.Controls.Add(buttonYes);
            buttonPanel.Controls.Add(buttonNo);

            buttonPanel.Location = new Point(
                (requestForm.ClientSize.Width - buttonPanel.Width) / 2,
                requestForm.ClientSize.Height - buttonPanel.Height + 10
            );

            requestForm.Controls.Add(label);
            requestForm.Controls.Add(progressBar);
            requestForm.Controls.Add(buttonPanel);

            buttonPanel.Controls.Add(buttonYes);
            buttonPanel.Controls.Add(buttonNo);

            requestForm.Controls.Add(label);
            requestForm.Controls.Add(progressBar);
            requestForm.Controls.Add(buttonPanel);

            requestForm.PerformLayout();


            var timer = new System.Windows.Forms.Timer() { Interval = 100 };
            int timeLeft = 87;

            timer.Tick += (s, e) =>
            {
                timeLeft--;
                progressBar.Value = timeLeft;

                if (timeLeft <= 0)
                {
                    timer.Stop();
                    requestForm.DialogResult = DialogResult.No;
                    requestForm.Close();
                }
            };
            timer.Start();

            var result = requestForm.ShowDialog();
            timer.Stop();
            if (result == DialogResult.Yes)
            {
                await _networkClient.SendMessageAsync(senderId, "GO!");
                var player = new SoundPlayer("accept.wav");
                player.Play();

                this.Invoke((MethodInvoker)delegate
                {
                    GameForm form = new();
                    form.recipientId = senderId;
                    form.senderId = _networkClient._clientId;
                    form.networkClient = _networkClient;
                    form.ShowDialog();
                });
                return;
            }
            else
            {
                await _networkClient.SendMessageAsync(senderId, "NO!");
                return;
            }
        }

        private async void PlayersGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != PlayersGrid.Columns["colAction"].Index)
                return;

            string id = PlayersGrid.Rows[e.RowIndex].Cells["colId"].Value?.ToString();
            if (string.IsNullOrEmpty(id))
                return;

            await _networkClient.SendMessageAsync(id, "GO?");
            SoundPlayer player = new SoundPlayer("invite.wav");
            player.Play();
            using var progressForm = new Form();
            progressForm.Text = "Ожидание ответа";
            progressForm.Size = new System.Drawing.Size(300, 100);
            progressForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            progressForm.StartPosition = FormStartPosition.CenterParent;
            progressForm.MinimizeBox = false;
            progressForm.MaximizeBox = false;

            var label = new Label
            {
                Dock = DockStyle.Top,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Text = "Ожидаем ответа от игрока..."
            };

            var progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Minimum = 0,
                Maximum = 100,
                Value = 100,
                Style = ProgressBarStyle.Continuous
            };

            progressForm.Controls.Add(label);
            progressForm.Controls.Add(progressBar);

            var cts = new CancellationTokenSource();

            var waitTask = _networkClient.WaitForMessageAsync(
                msg => msg.Contains("GO!") || msg.Contains("NO!") && msg.Contains(id),
                10000);

            var countdownTask = CountdownAsync(progressBar, label, cts.Token);
            progressForm.Show();

            var completedTask = await Task.WhenAny(waitTask, countdownTask);
            cts.Cancel();
            progressForm.Close();
            if (completedTask == waitTask)
            {
                player.Stop();
                try
                {
                    var response = await waitTask;
                    if (response.Contains("NO!"))
                        MessageBox.Show("Игрок отклонил приглашение!", "Результат",
                        MessageBoxButtons.OK);
                    else
                    {
                        player = new SoundPlayer("accept.wav");
                        player.Play();

                        this.Invoke((MethodInvoker)delegate
                        {
                            GameForm form = new();
                            form.recipientId = id;
                            form.senderId = _networkClient._clientId;
                            form.ShowDialog();
                        });
                        return;
                    }
                }
                catch (TaskCanceledException)
                {
                    MessageBox.Show("Игрок не принял приглашение!", "Результат",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            return;
        }

        private async Task UpdatePlayersGrid(string message)
        {
            string clientsPart = message["SERVER:CLIENTS:".Length..];
            clientsPart.Replace("SERVER", "");
            string[] clientEntries = clientsPart.Split([','], StringSplitOptions.RemoveEmptyEntries);

            PlayersGrid.Invoke((Action)(() =>
            {
                PlayersGrid.Rows.Clear();
                string[] clientEntries = clientsPart.Split([','], StringSplitOptions.RemoveEmptyEntries);
                foreach (string entry in clientEntries)
                {
                    string[] clientInfo = entry.Split(':');
                    if (clientInfo.Length >= 2)
                    {
                        string clientId = clientInfo[0];
                        string clientName = clientInfo[1];

                        if (clientId != _networkClient._clientId)
                            PlayersGrid.Rows.Add(clientName, clientId);
                    }
                }
            }));
        }

        private async Task CountdownAsync(ProgressBar progressBar, Label label, CancellationToken cancellationToken)
        {
            for (int i = 10; i >= 0; i--)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;
                if (progressBar.InvokeRequired)
                {
                    progressBar.Invoke((Action)(() =>
                    {
                        progressBar.Value = i * 10;
                        label.Text = $"Осталось: {i} сек.";
                    }));
                }
                else
                {
                    progressBar.Value = i * 10;
                    label.Text = $"Осталось: {i} сек.";
                }
                try
                {
                    await Task.Delay(1000, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                GameForm form = new();
                form.recipientId = "asdasd";
                form.senderId = "asd";
                form.ShowDialog();
            });
        }
    }
}
