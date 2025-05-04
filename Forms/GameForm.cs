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
                _ = SendInitialisedSession();
            }
            else
            {
                var Timer = Task.Delay(5000);
                var completedTask = await Task.WhenAny(Timer, RecieveInitialisedSession());
                if (completedTask == Timer)
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
               msg => msg.Contains("PLAYER") || msg.Contains("DECK"), 4900);
            var response = await waitTask;
            InitialiseSessionFromMessage(response);
        }

        private void InitialiseSessionFromMessage(string message)
        {
            _gameSession = new GameSession();
            Dictionary<string, Card[]> keyValuePairs = ParseCardsMessage(message);
            for (int i = 0; i < _gameSession.PlayerCards.Length; i++)
            {
                _gameSession.PlayerCards[i] = keyValuePairs["PLAYER" + i.ToString()].ToList();
            }
            for (int i = 0; i < _gameSession.Deck.Count; i++)
            {
                _gameSession.Deck.Push(keyValuePairs["DECK"][i]);
            }
            _gameSession.Trump = _gameSession.Deck.ElementAt(_gameSession.Deck.Count() - 1).Suit;
            _gameSession.CurrPlayerMove = 1; // ну потому что игроков всего два
            _gameSession.CurrPlayerAttacker = 1;
        }

        public static Dictionary<string, Card[]> ParseCardsMessage(string message)
        {
            var result = new Dictionary<string, Card[]>();

            var sections = message.Split([','], StringSplitOptions.RemoveEmptyEntries)
                                  .Where(s => s.Contains(':'))
                                  .ToArray();

            foreach (var section in sections)
            {
                var parts = section.Split(':');
                if (parts.Length != 2)
                    continue;

                string key = parts[0].Trim();
                string[] cardStrings = parts[1].Split(';', StringSplitOptions.RemoveEmptyEntries);

                var cards = new List<Card>();
                foreach (var cardStr in cardStrings)
                {
                    var numbers = cardStr.Split(',');
                    if (numbers.Length == 2 &&
                        int.TryParse(numbers[0], out int suit) &&
                        int.TryParse(numbers[1], out int power))
                    {
                        cards.Add(new Card(suit, power));
                    }
                }

                result[key] = cards.ToArray();
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
