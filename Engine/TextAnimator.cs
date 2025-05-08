using System;
using System.Drawing.Drawing2D;
using NAudio.Wave;

public class AudioPlayer : IDisposable
{
    private WaveOutEvent _waveOut;
    private AudioFileReader _audioFile;
    private float _volume = 1.0f;

    /// <summary>
    /// Воспроизводит звук из файла с мгновенным стартом
    /// </summary>
    /// <param name="filePath">Путь к аудиофайлу</param>
    /// <param name="volume">Громкость (0.0 - 1.0)</param>
    public void PlaySound(string filePath, float volume = 1.0f)
    {
        try
        {
            _volume = Math.Clamp(volume, 0f, 1f);
            _audioFile = new AudioFileReader(filePath);
            _audioFile.Volume = _volume; // Устанавливаем громкость сразу

            _waveOut = new WaveOutEvent();
            _waveOut.DesiredLatency = 100; // Минимальная задержка для мгновенного старта
            _waveOut.Init(_audioFile);
            _waveOut.Play();
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка воспроизведения звука: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Устанавливает громкость (0.0 - 1.0)
    /// </summary>
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = Math.Clamp(value, 0f, 1f);
            if (_audioFile != null)
                _audioFile.Volume = _volume;
        }
    }

    /// <summary>
    /// Останавливает воспроизведение
    /// </summary>
    public void Stop()
    {
        _waveOut?.Stop();
        _waveOut?.Dispose();
        _waveOut = null;

        _audioFile?.Dispose();
        _audioFile = null;
    }

    public void Dispose()
    {
        Stop();
    }
}

public class TextAnimator
{
   
    private PictureBox pictureBox;
    private Font font;
    private Bitmap bufferBitmap;
    private Graphics bufferGraphics;
    private AudioPlayer audioPlayer;

    public TextAnimator(PictureBox pb)
    {
        pictureBox = pb;
        pictureBox.BackColor = Color.DarkGreen;

        bufferBitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
        bufferGraphics = Graphics.FromImage(bufferBitmap);
        font = new Font("Times New Roman", 72, FontStyle.Bold);
        audioPlayer = new AudioPlayer();
    }

    // Старый метод для "You Lose"
    public async Task ShowDefeatText()
    {
        bufferGraphics.Clear(Color.DarkGreen);
        SizeF textSize = bufferGraphics.MeasureString("You Lose", font);
        PointF position = new PointF(
            (pictureBox.Width - textSize.Width) / 2,
            (pictureBox.Height - textSize.Height) / 2);

        _ = Task.Run(async () =>
        {
           
            for (int i = 1; i <= "You Lose".Length - 1; i++)
            {
                PlaySound("text.wav");
                await Task.Delay(240);
            }
        });

        await Task.Delay(750);
        for (int i = 1; i <= "You Lose".Length; i++)
        {          
            string currentText = "You Lose".Substring(0, i);
            if (!currentText.EndsWith(' '))
                await Task.Delay(239);
            bufferGraphics.Clear(Color.DarkGreen);
            DrawTextWithBorder(bufferGraphics, currentText, position);

            pictureBox.Image?.Dispose();
            pictureBox.Image = (Bitmap)bufferBitmap.Clone();         
        }

        await Task.Delay(1500);
        PlaySound("text.wav");
    }

    // Новый метод для "You Win"
    public async Task ShowVictoryText()
    {
      
        PlaySound("text.wav");
        await Task.Delay(700);
        bufferGraphics.Clear(Color.DarkGreen);
        // Рассчитываем позицию для всего текста "You Win"
        SizeF fullTextSize = bufferGraphics.MeasureString("You Win", font);
        PointF position = new PointF(
            (pictureBox.Width - fullTextSize.Width) / 2,
            (pictureBox.Height - fullTextSize.Height) / 2);

        // 1. Выводим "You" сразу
        DrawTextWithBorder(bufferGraphics, "You", position);
        pictureBox.Image?.Dispose();
        pictureBox.Image = (Bitmap)bufferBitmap.Clone();
        await Task.Delay(255);
        PlaySound("text.wav");
        await Task.Delay(500);

        // 2. Выводим "You Win" (полный текст)

        bufferGraphics.Clear(Color.DarkGreen);
        DrawTextWithBorder(bufferGraphics, "You Win", position);
        pictureBox.Image?.Dispose();
        pictureBox.Image = (Bitmap)bufferBitmap.Clone();
        await Task.Delay(1777);
    }

    public async Task AnimateConfetti(int durationSeconds = 5)
    {
        // Сохраняем текущее изображение PictureBox
        Bitmap background = new Bitmap(pictureBox.Image ?? new Bitmap(pictureBox.Width, pictureBox.Height));
        audioPlayer.PlaySound("win.wav");
     //   _ = Task.Run(async () => { await Task.Delay(3000); audioPlayer.PlaySound("win.wav"); });
        // Создаем список конфетти
        List<ConfettiPiece> confettiPieces = new List<ConfettiPiece>();
        Random random = new Random();

        // Создаем таймер для анимации
        var startTime = DateTime.Now;
        var endTime = startTime.AddSeconds(durationSeconds);

        // Создаем буфер для анимации
        using (var confettiBuffer = new Bitmap(pictureBox.Width, pictureBox.Height))
        {
            while (DateTime.Now < endTime)
            {
                // Добавляем новые конфетти
                if (random.NextDouble() > 0.001) // Вероятность добавления нового конфетти
                {
                    confettiPieces.Add(new ConfettiPiece(pictureBox.Width, random));
                }

                // Очищаем буфер и рисуем фон
                using (var g = Graphics.FromImage(confettiBuffer))
                {
                    g.DrawImage(background, 0, 0);

                    // Обновляем и рисуем все конфетти
                    for (int i = confettiPieces.Count - 1; i >= 0; i--)
                    {
                        var piece = confettiPieces[i];
                        piece.Update();

                        // Удаляем конфетти, которые вышли за пределы экрана
                        if (piece.Y > pictureBox.Height)
                        {
                            confettiPieces.RemoveAt(i);
                            continue;
                        }

                        // Рисуем конфетти
                        using (var brush = new SolidBrush(piece.Color))
                        {
                            var points = new PointF[]
                            {
                                new PointF(piece.X, piece.Y - piece.Size/2),
                                new PointF(piece.X + piece.Size/2, piece.Y),
                                new PointF(piece.X, piece.Y + piece.Size/2),
                                new PointF(piece.X - piece.Size/2, piece.Y)
                            };

                            g.FillPolygon(brush, points);
                        }
                    }
                }

                // Обновляем PictureBox
                pictureBox.Image?.Dispose();
                pictureBox.Image = (Bitmap)confettiBuffer.Clone();

                // Задержка для анимации (около 60 FPS)
                await Task.Delay(16);
            }
        }

        // Восстанавливаем исходное изображение
        pictureBox.Image?.Dispose();
        pictureBox.Image = background;
    }

    private void DrawTextWithBorder(Graphics g, string text, PointF position)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

        using (GraphicsPath path = new GraphicsPath())
        {
            path.AddString(
                text,
                font.FontFamily,
                (int)font.Style,
                g.DpiY * font.Size / 72,
                position,
                StringFormat.GenericTypographic);

            using (Pen pen = new Pen(Color.White, 3))
            {
                pen.LineJoin = LineJoin.Round;
                g.DrawPath(pen, path);
            }

            g.FillPath(Brushes.Black, path);
        }
    }

    private void PlaySound(string path)
    {
        try
        {
            audioPlayer.PlaySound(path);          
        }
        catch
        {
            // Игнорируем ошибки воспроизведения звука
        }
    }

    public void CleanUp()
    {
        font?.Dispose();
        audioPlayer?.Dispose();
        bufferGraphics?.Dispose();
        bufferBitmap?.Dispose();
    }

    private class ConfettiPiece
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float Size { get; private set; }
        public Color Color { get; private set; }
        public float Speed { get; private set; }
        public float Amplitude { get; private set; }
        public float Frequency { get; private set; }
        public float InitialX { get; private set; }
        public float Time { get; private set; }

        public ConfettiPiece(int maxWidth, Random random)
        {
            X = InitialX = random.Next(0, maxWidth);
            Y = -10; // Начинаем чуть выше экрана
            Size = random.Next(10, 16);
            Color = Color.FromArgb(random.Next(150, 255), random.Next(256), random.Next(256), random.Next(256));
            Speed = random.Next(4, 13);
            Amplitude = random.Next(20, 40);
            Frequency = (float)random.NextDouble() * 0.1f;
            Time = 0;
        }

        public void Update()
        {
            Time += 2f;
            Y += Speed;
            X = InitialX + Amplitude * (float)Math.Sin(Time * Frequency);
        }
    }
}