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

        private void GameForm_Shown(object sender, EventArgs e)
        {
            _gameSession = new GameSession();
            _sessionGraphics = new SessionGraphics(pbGameField, _gameSession);
            _sessionController = new SessionController(pbGameField, btnGrab, btnMoveTransfer, _gameSession, btnDecCradsPage, btnIncCardsPage, _sessionGraphics);
            _sessionGraphics.UpdateGamefield(null);
            // ВАЖНО,ТАК И ДОЛЖНО БЫТЬ!!
            pbGameField.MouseMove += pbGameField_MouseMove;
            pbGameField.MouseUp += pbGameField_MouseUp;
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
