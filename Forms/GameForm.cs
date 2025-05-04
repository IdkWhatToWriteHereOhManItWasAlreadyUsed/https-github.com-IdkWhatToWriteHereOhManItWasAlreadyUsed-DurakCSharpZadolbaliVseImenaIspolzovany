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
                sessionData += "PLAYER" + j.ToString() + ':';
                for (int i = 0; i < _gameSession.PlayerCards[j].Count; i++)
                {
                    sessionData += _gameSession.PlayerCards[j][i].ToString();
                }
            }
            sessionData += "DECK:";
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
            for (int i = 0; i < 24; i++)
            {
                _gameSession.Deck.Push(keyValuePairs["DECK"][23 - i]);
            }
            _gameSession.Trump = _gameSession.Deck.ElementAt(_gameSession.Deck.Count - 1).Suit;
            _gameSession.CurrPlayerMove = 1; // ну потому что игроков всего два пока что, так что сойдет
            _gameSession.CurrPlayerAttacker = 1;
        }

        public static Dictionary<string, List<Card>> ParseCardsMessage(string input)
        {
            var result = new Dictionary<string, List<Card>>();
            // Удаляем начальный мусор (если есть)
            int startIndex = FindNextKeyPosition(input);
            if (startIndex == -1)
                return result; 

            input = input.Substring(startIndex);

            while (input.Length > 0)
            {
                int colonPos = input.IndexOf(':');
                if (colonPos == -1)
                    break;

                string key = input.Substring(0, colonPos);
                input = input.Substring(colonPos + 1);

                int nextKeyPos = FindNextKeyPosition(input);

                string cardsStr;
                if (nextKeyPos == -1)
                {
                    cardsStr = input;
                    input = "";
                }
                else
                {
                    cardsStr = input.Substring(0, nextKeyPos);
                    input = input.Substring(nextKeyPos);
                }

                var cards = new List<Card>();
                var cardEntries = cardsStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

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

        private static int FindNextKeyPosition(string input)
        {
            var keyMarkers = new[] { "PLAYER0:", "PLAYER1:", "DECK:" };
            var positions = keyMarkers
                .Select(marker => input.IndexOf(marker))
                .Where(pos => pos != -1)
                .ToList();

            return positions.Count == 0 ? -1 : positions.Min();
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
