using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Durak_.Forms
{
    public partial class GameForm : Form
    {
        public GameForm()
        {
            InitializeComponent();
        }

        public bool StartFirst = true;
        public string senderId;
        public string recipientId;
        public NetworkClient networkClient;
        private GameSession _gameSession;
        private SessionController _sessionController;
        private SessionGraphics _sessionGraphics;

        private void GameForm_Load(object sender, EventArgs e)
        {
            //
        }

        private void BtnGrabClick(object sender, EventArgs e)
        {
            //
        }

        private void BtnMoveTransfer_Click(object sender, EventArgs e)
        {
            //
        }

        private async void GameForm_Shown(object sender, EventArgs e)
        {     
            if (StartFirst)
            {
                _gameSession = new GameSession();
                await Task.Delay(200);
                _ = SendInitialisedSession();
            }
            else
            {
                var Timer = Task.Delay(50000);
                var completedTask = await Task.WhenAny(Timer, RecieveInitialisedSession());
                if (completedTask == Timer || completedTask.Status == TaskStatus.Canceled)
                {
                    MessageBox.Show("Превышено ожидание запроса от игрока!");
                    this.Close();
                    return;
                }
            }
           
            _sessionGraphics = new SessionGraphics(pbGameField, _gameSession);
            _sessionController = new SessionController(pbGameField, btnGrab, btnMoveTransfer, _gameSession, btnDecCradsPage, btnIncCardsPage, _sessionGraphics);
            _sessionGraphics.UpdateGamefield(null);
            // ВАЖНО,ТАК И ДОЛЖНО БЫТЬ!!
            pbGameField.MouseMove += pbGameField_MouseMove;
            pbGameField.MouseUp += pbGameField_MouseUp;

        }

        private async Task SendInitialisedSession()
        {
            string sessionData = "";
            for (int j = 0; j < _gameSession.PlayerCards.Length; j++)
            {
                sessionData += ",PLAYER" + j.ToString() + ':';
                for (int i = 0; i < _gameSession.PlayerCards[j].Count; i++)
                {
                    sessionData += _gameSession.PlayerCards[j][i].ToString();
                }
            }
            sessionData += ",DECK:";
            for (int i = 0; i < _gameSession.Deck.Count; i++)
            {
                sessionData += _gameSession.Deck.ElementAt(i).ToString();
            }
            _ = networkClient.SendMessageAsync(recipientId, sessionData);
        }

        private async Task RecieveInitialisedSession()
        {
            var waitTask = networkClient.WaitForMessageAsync(
               msg => msg.Contains("DECK"), 4900);
            var response = await waitTask;
            InitialiseSessionFromMessage(response);
        }

        private void InitialiseSessionFromMessage(string message)
        {
            _gameSession = new GameSession();
            Dictionary<string, List<Card>> keyValuePairs = ParseCardsMessage(message);
            for (int i = 0; i < _gameSession.PlayerCards.Length; i++)
            {
                _gameSession.PlayerCards[i] = keyValuePairs["PLAYER" + i.ToString()];
            }
            _gameSession.Deck.Clear();
            for (int i = 0; i < _gameSession.Deck.Count; i++)
            {
                _gameSession.Deck.Push(keyValuePairs["DECK"][_gameSession.Deck.Count - 1 - i]);
            }
            _gameSession.Trump = _gameSession.Deck.ElementAt(_gameSession.Deck.Count - 1).Suit;
            // поменять на 1 оба!!!!!!!!!!
            _gameSession.CurrPlayerMove = 0; // ну потому что игроков всего два
            _gameSession.CurrPlayerAttacker = 0;
        }

        public static Dictionary<string, List<Card>> ParseCardsMessage(string input)
        {
            var result = new Dictionary<string, List<Card>>();

            // Разделяем строку по шаблону "PLAYERx:" или "DECK:"
            var parts = input.Split(new[] { "PLAYER0:", "PLAYER1:", "DECK:" },
                                   StringSplitOptions.RemoveEmptyEntries);

            // Если строка начинается не с PLAYER/DECK, пропускаем мусор
            int startIndex = input.IndexOf("PLAYER0:");
            if (startIndex != -1)
            {
                input = input.Substring(startIndex);
            }

            // Обрабатываем каждый блок
            var sections = input.Split(new[] { "PLAYER0:", "PLAYER1:", "DECK:" },
                                     StringSplitOptions.RemoveEmptyEntries);

            foreach (var section in sections)
            {
                if (string.IsNullOrWhiteSpace(section))
                    continue;

                // Определяем ключ (PLAYER0, PLAYER1 или DECK)
                string key = input.Substring(0, input.IndexOf(section) - 1);
                key = key.Split(':').Last();

                // Разбиваем карты
                var cards = new List<Card>();
                var cardEntries = section.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var entry in cardEntries)
                {
                    var values = entry.Split(',');
                    if (values.Length == 2 &&
                        int.TryParse(values[0], out int suit) &&
                        int.TryParse(values[1], out int power))
                    {
                        cards.Add(new Card { Suit = suit, Power = power });
                    }
                }

                result[key] = cards;
            }

            return result;
        }

        private void pbGameField_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _sessionGraphics.UpdateGamefield(e);
        }

        private void pbGameField_MouseUp(object sender, MouseEventArgs e)
        {
            _sessionGraphics.ClearSelection();
            _sessionGraphics.UpdateGamefield(null);
        }
    }
}
