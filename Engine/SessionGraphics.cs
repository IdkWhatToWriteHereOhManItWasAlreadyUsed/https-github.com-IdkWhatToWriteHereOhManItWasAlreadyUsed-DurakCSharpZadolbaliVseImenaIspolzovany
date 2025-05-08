namespace Durak_
{
    public class SessionGraphics
    {
        private const int WINDOW_H = 600;
        private const int WINDOW_W = 800;
        private const int CARD_H = 96;
        private const int CARD_W = 71;
        private const int GAME_CARDS_Y = 220;
        private const int DISTANCE_BETWEEN_CARDS = 16;
        private PictureBox _gamePictureBox;
        private GameSession _gameSession;
        public int CurrCardsPage = 0;
        private int SelectedCard = -1;
        private Bitmap[,] Cards; 
        private Bitmap Back;

       
        public SessionGraphics(PictureBox pb, GameSession session) 
        {
            _gamePictureBox = pb;
            _gamePictureBox.BackColor = Color.DarkGreen;
            _gameSession = session;
            var AllCards = new Bitmap("Cards.png");
            Cards = SplitBitmap(AllCards);
            Back = new Bitmap("Back.png");
            Back = SetDpi(Back, 120);
        }

        public static Bitmap SetDpi(Bitmap bitmap, float dpi)
        {
            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            newBitmap.SetResolution(dpi, dpi);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                g.DrawImage(bitmap, 0, 0);
            }
            return newBitmap;
        }

        public void SetSelection(int cardNum)
        {
            SelectedCard = cardNum == -1 ? -1: cardNum % 9;
        }

        public void ClearSelection()
        {
            SelectedCard = -1;
        }

        public void UpdateGamefield(MouseEventArgs? e)
        {
            Bitmap bmp = new(_gamePictureBox.Width, _gamePictureBox.Height);
            using Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.DarkGreen);          
            DrawGameStacks(g);
            DrawDeck(g);
            DrawOpponentCards(g);
            DrawPlayerCards(g);
            DrawSelectedCard(g, e);
            _gamePictureBox.Image = bmp;
            _gamePictureBox.Invalidate();
        }   

        private void DrawGameStacks(Graphics g)
        {
            Card card = _gameSession.GameStack[0].FirstOrDefault();
            if (_gameSession.GameStack[0].Count > 0)
                g.DrawImage(Cards[card.Power - 6, card.Suit], WINDOW_W / 2 - CARD_W - CARD_W - DISTANCE_BETWEEN_CARDS, GAME_CARDS_Y);
            card = _gameSession.GameStack[1].FirstOrDefault();
            if (_gameSession.GameStack[1].Count > 0)
                g.DrawImage(Cards[card.Power - 6, card.Suit], WINDOW_W / 2 - CARD_W, GAME_CARDS_Y);
            card = _gameSession.GameStack[2].FirstOrDefault();
            if (_gameSession.GameStack[2].Count > 0)
                g.DrawImage(Cards[card.Power - 6, card.Suit], WINDOW_W / 2 + DISTANCE_BETWEEN_CARDS, GAME_CARDS_Y);
            card = _gameSession.GameStack[3].FirstOrDefault();
            if (_gameSession.GameStack[3].Count > 0)
                g.DrawImage(Cards[card.Power - 6, card.Suit], WINDOW_W / 2 + CARD_W + DISTANCE_BETWEEN_CARDS * 2, GAME_CARDS_Y);       
        }

        private void DrawDeck(Graphics g)
        {
            if (_gameSession.Deck.Count == 0)
                return;
            Card card = _gameSession.Deck.ElementAt(_gameSession.Deck.Count() - 1);
            g.DrawImage(Cards[card.Power - 6, card.Suit], WINDOW_W - CARD_W - DISTANCE_BETWEEN_CARDS, GAME_CARDS_Y);
            if (_gameSession.Deck.Count > 1)
                g.DrawImage(Back, WINDOW_W - CARD_W - DISTANCE_BETWEEN_CARDS, GAME_CARDS_Y - 20, Back.Width, Back.Height);
        }

        private void DrawOpponentCards(Graphics g)
        {
            for (int i = 0; i < _gameSession.PlayerCards[1].Count; i++)
                g.DrawImage(Back, WINDOW_W / 2 + DISTANCE_BETWEEN_CARDS * (_gameSession.PlayerCards[1].Count / 2 - i), - 20);
        }

        private void DrawPlayerCards(Graphics g)
        {
            int i = 0;
            foreach (var card in _gameSession.PlayerCards[0])
                if (i++ != SelectedCard)
                    g.DrawImage(Cards[card.Power - 6, card.Suit], DISTANCE_BETWEEN_CARDS + (i-1)*(DISTANCE_BETWEEN_CARDS + CARD_W),WINDOW_H - CARD_H - DISTANCE_BETWEEN_CARDS);
        }

        private void DrawSelectedCard(Graphics g, MouseEventArgs? e)
        {
            if ( e != null && SelectedCard != -1)
            {
                g.DrawImage(Cards[_gameSession.PlayerCards[0][SelectedCard].Power - 6, _gameSession.PlayerCards[0][SelectedCard].Suit], e.X - CARD_W/2, e.Y - CARD_H/2);
            }
        }

        private static Bitmap[,] SplitBitmap(Bitmap originalImage)
        {
            int originalWidth = originalImage.Width;
            int originalHeight = originalImage.Height;
            int horizontalParts = 9;
            int verticalParts = 4;
            int partWidth = originalWidth / horizontalParts;
            int partHeight = originalHeight / verticalParts;
            Bitmap[,] imageGrid = new Bitmap[horizontalParts, verticalParts];
            for (int y = 0; y < verticalParts; y++)
            {
                for (int x = 0; x < horizontalParts; x++)
                {
                    Rectangle cropArea = new Rectangle(
                        x * partWidth,
                        y * partHeight,
                        partWidth,
                        partHeight);
                    Bitmap part = new Bitmap(partWidth, partHeight);
                    using (Graphics g = Graphics.FromImage(part))
                    {
                        g.DrawImage(
                            originalImage,
                            new Rectangle(0, 0, partWidth, partHeight),
                            cropArea,
                            GraphicsUnit.Pixel);
                    }
                    imageGrid[x, y] = part;
                }
            }
            return imageGrid;
        }

        public async void ShowVictoryScreen()
        {
            TextAnimator animator = new TextAnimator(_gamePictureBox);
            await animator.ShowVictoryText();
            await animator.AnimateConfetti(5);
            animator.CleanUp();
        }
        public async void ShowDefeatScreen()
        {
            TextAnimator animator = new TextAnimator(_gamePictureBox);
            await animator.ShowDefeatText();
            animator.CleanUp();
        }
    }
}
