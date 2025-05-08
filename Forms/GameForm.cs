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


        private async void GameForm_Shown(object sender, EventArgs e)
        {
         //   Console.Clear();
            _gameSession = new GameSession();
            _sessionGraphics = new SessionGraphics(pbGameField, _gameSession);
            _sessionController = new SessionController(_gameSession, _sessionGraphics, networkClient);
            _sessionController.recipientId = recipientId;
            if (StartFirst)
            {
                await Task.Delay(200);
                _ = _sessionController.SendInitialisedSession(recipientId);
            }
            else
            {
                var Timer = Task.Delay(50000);
                var completedTask = await Task.WhenAny(Timer, _sessionController.RecieveInitialisedSession());
                if (completedTask == Timer || completedTask.Status == TaskStatus.Canceled)
                {
                    MessageBox.Show("Превышено ожидание запроса от игрока!");
                    this.Close();
                    return;
                }
            }
            _sessionController.AssignControls(pbGameField, btnGrab, btnMoveTransfer, btnDecCradsPage, btnIncCardsPage);
            _sessionGraphics.UpdateGamefield(null);
            // ВАЖНО,ТАК И ДОЛЖНО БЫТЬ!!
            pbGameField.MouseMove += pbGameField_MouseMove;
            pbGameField.MouseUp += pbGameField_MouseUp;
            btnMoveTransfer.Click += btnMoveTransfer_Click;
            _ = _sessionController.AwaitForPlayerMove();
        }

        private void pbGameField_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && _gameSession.CurrPlayerMove == 0)
                _sessionGraphics.UpdateGamefield(e);
        }

        private void pbGameField_MouseUp(object sender, MouseEventArgs e)
        {       
            if (_gameSession.CurrPlayerMove == 0)
            _sessionGraphics.UpdateGamefield(null);
        }

        private async void btnMoveTransfer_Click(object sender, EventArgs e)
        {
            if (_gameSession.IsGameFinished())
            {
                if (_gameSession.GetWinner() == 0)
                    await _sessionGraphics.ShowVictoryScreen();
                else
                    await _sessionGraphics.ShowDefeatScreen();
                this.Close();
            }
            if (_gameSession.IsGameFinished())
            {
                await Task.Delay(7000);
                this.Close();
            }
        }
    }
}
