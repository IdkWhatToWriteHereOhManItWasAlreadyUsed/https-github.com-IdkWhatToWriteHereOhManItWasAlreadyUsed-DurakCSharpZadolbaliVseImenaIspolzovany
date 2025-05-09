using System;
using System.Drawing.Drawing2D;
using NAudio.Utils;
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
                await Task.Delay(205);
            }
        });

        await Task.Delay(530);
        for (int i = 1; i <= "You Lose".Length; i++)
        {          
            string currentText = "You Lose".Substring(0, i);
            if (!currentText.EndsWith(' '))
                await Task.Delay(200);
            bufferGraphics.Clear(Color.DarkGreen);
            DrawTextWithBorder(bufferGraphics, currentText, position);

            pictureBox.Image?.Dispose();
            pictureBox.Image = (Bitmap)bufferBitmap.Clone();         
        }

        await Task.Delay(3000);
        PlaySound("text.wav");
        await Task.Delay(205);
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
        Bitmap background = new Bitmap(pictureBox.Image ?? new Bitmap(pictureBox.Width, pictureBox.Height));
        audioPlayer.PlaySound("win.wav");
        List<ConfettiPiece> confettiPieces = new List<ConfettiPiece>();
        Random random = new Random();

        // Создаем таймер для анимации
        var startTime = DateTime.Now;
        var endTime = startTime.AddSeconds(durationSeconds);

        float angle1 = (float)(random.NextDouble() * Math.PI * 2);
        float angle2 = (float)(random.NextDouble() * Math.PI * 2);
        float angle3 = (float)(random.NextDouble() * Math.PI * 2);
        float angle4 = (float)(random.NextDouble() * Math.PI * 2);
        var points = new PointF[4];
        ConfettiPiece piece = new ConfettiPiece(20, random);

        // Создаем буфер для анимации
        using (var confettiBuffer = new Bitmap(pictureBox.Width, pictureBox.Height))
        {
            while (DateTime.Now < endTime)
            {
                confettiPieces.Add(new ConfettiPiece(pictureBox.Width, random));
               // confettiPieces.Add(new ConfettiPiece(pictureBox.Width, random));


                // Очищаем буфер и рисуем фон
                using (var g = Graphics.FromImage(confettiBuffer))
                {
                    g.DrawImage(background, 0, 0);

                    // Обновляем и рисуем все конфетти
                    for (int i = confettiPieces.Count - 1; i >= 0; i--)
                    {
                        piece = confettiPieces[i];
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
                            float horizontal = piece.Size / 2; // Горизонтальный радиус (без изменений)
                            float vertical = piece.Size / 2 * piece.VerticalCompression; // Вертикальный радиус со сжатием

                            points =
                                [
                                new PointF(piece.X, piece.Y - vertical),         // Верхняя вершина
                                new PointF(piece.X + horizontal, piece.Y),       // Правая вершина
                                new PointF(piece.X, piece.Y + vertical),         // Нижняя вершина
                                new PointF(piece.X - horizontal, piece.Y)        // Левая вершина
                                ];
                            g.FillPolygon(brush, points);
                        }
                    }
                }
                
                // Обновляем PictureBox
                pictureBox.Image?.Dispose();
                pictureBox.Image = (Bitmap)confettiBuffer.Clone();
                // Задержка для анимации (около 60 FPS)
                await Task.Delay(12);
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
        Random random = new Random();

        static Color[] brightColors = new Color[]
        {
            Color.FromArgb(255, 255, 0, 0),   // Красный
            Color.FromArgb(255, 0, 255, 0),   // Зеленый
            Color.FromArgb(255, 0, 0, 255),   // Синий
            Color.FromArgb(255, 255, 255, 0), // Желтый
            Color.FromArgb(255, 255, 0, 255), // Розовый
            Color.FromArgb(255, 0, 255, 255), // Бирюзовый
            Color.FromArgb(255, 255, 165, 0)   // Оранжевый
        };
        public float VerticalCompression { get; set; }
        public float TargetCompression { get; set; }

        public ConfettiPiece(int maxWidth, Random random)
        {
            X = InitialX = random.Next(0, maxWidth);
            Y = -10; // Начинаем чуть выше экрана
            Size = random.Next(12, 17);
            Color = brightColors[random.Next(brightColors.Length)];
            Speed = random.Next(5, 10);
            Amplitude = random.Next(5, 10);
            Frequency = (float)random.NextDouble() * 0.1f;
            Time = 0;
            VerticalCompression = 1;
            TargetCompression = VerticalCompression;
        }

        public void Update()
        {
            Time += 2f;
            Y += Speed;
            X = InitialX + Amplitude * (float)Math.Sin(Time * Frequency);
            if (random.Next() % 3 == 2)
            {
                VerticalCompression += (TargetCompression - VerticalCompression);
                TargetCompression = (float)random.NextDouble() * 0.6f + 0.4f;
            }     
        }
    }
}