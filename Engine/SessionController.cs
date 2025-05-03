using System;
using System.DirectoryServices.ActiveDirectory;

namespace Durak_
{   
    public class SessionController
    {
        private const int WINDOW_H = 600;
        private const int WINDOW_W = 800;
        private const int CARD_H = 96;
        private const int CARD_W = 71;
        private const int GAME_CARDS_Y = 220;
        private const int DISTANCE_BETWEEN_CARDS = 16;
        private readonly PictureBox? GamePictureBox;
        private readonly SessionGraphics sessionGraphics;
        private readonly  GameSession gameSession;
        private bool IsShownScreenBetweenMoves = false;
        private bool IsShownGrabButton = false;
        private int CurrCardsPage = 0;
        public int SelectedCardNum = -1;
        public SessionController(PictureBox pb, Button GrabButton, Button MoveTransferButton, GameSession gs, Button DecCardsPageButton, Button IncCardsPageButton, SessionGraphics graphics)
        {
            GamePictureBox = pb;
            GamePictureBox.MouseDown += HandleMousedown;
            GamePictureBox.MouseUp += HandleMouseUp;
            GrabButton.Click += HandleGrabClick;
            MoveTransferButton.Click += HandleMoveTransferClick;
            sessionGraphics = graphics;
            gameSession = gs;
            DecCardsPageButton.Click += SwitchCardsPage;
            IncCardsPageButton.Click += SwitchCardsPage;
        }

        private void HandleMousedown(object? sender, EventArgs e)
        {
            int x = (e as MouseEventArgs).X;
            int y = (e as MouseEventArgs).Y;
            if (gameSession.CurrPlayerAttacker == gameSession.CurrPlayerMove)
            {
                IsShownGrabButton = false;
                SelectedCardNum = GetSelectedCard(x, y);
                if (SelectedCardNum != -1)
                {
                    SelectedCardNum = SelectedCardNum + 9*CurrCardsPage;
                    sessionGraphics.SetSelection(SelectedCardNum);
                }
            }
            else
            {
                IsShownGrabButton = false;
                SelectedCardNum = GetSelectedCard(x, y);
                if (SelectedCardNum != -1)
                {
                    SelectedCardNum = SelectedCardNum + 9 * CurrCardsPage;
                    sessionGraphics.SetSelection(SelectedCardNum);
                }
            }
        }

        private void HandleMouseUp(object? sender, EventArgs e)
        {
            int x = (e as MouseEventArgs).X;
            int y = (e as MouseEventArgs).Y;
            if (IsShownScreenBetweenMoves)
            {
                IsShownScreenBetweenMoves = false;
                return;
            }
            if (gameSession.CurrPlayerAttacker == gameSession.CurrPlayerMove)
            {
                int selectedGameStack = GetSelectedGameStack(x, y);
                if (selectedGameStack != -1)
                {
                    sessionGraphics.ClearSelection();
                    if (selectedGameStack != -1 && SelectedCardNum != -1)
                    {
                        gameSession.DoMove(gameSession.PlayerCards[gameSession.CurrPlayerMove][SelectedCardNum], selectedGameStack);
                        SelectedCardNum = -1;
                    }
                }
            }
            else
            {
                int selectedGameStack = GetSelectedGameStack(x, y);
                if (selectedGameStack != -1)
                {
                    sessionGraphics.ClearSelection();
                    if (selectedGameStack != -1 && SelectedCardNum != -1 && gameSession.CanBeat(gameSession.GameStack[selectedGameStack].First(), gameSession.PlayerCards[gameSession.CurrPlayerMove][SelectedCardNum]))
                    {
                        gameSession.DoMove(gameSession.PlayerCards[gameSession.CurrPlayerMove][SelectedCardNum], selectedGameStack);
                        SelectedCardNum = -1;
                    }
                }
            }
            sessionGraphics.ClearSelection();
        }

        private void HandleGrabClick(object? sender, EventArgs e)
        {
            gameSession.Grab();
            gameSession.GiveCardsAfterDefense();
            gameSession.TransferMove(MoveType.mtGrab);
        }

        private void HandleMoveTransferClick(object? sender, EventArgs e)
        {
            gameSession.GiveCardsAfterDefense();
            gameSession.TransferMove(MoveType.mtTransfer);
        }

        public static int GetSelectedGameStack(int x, int y)
        {
            if (y < GAME_CARDS_Y || y > GAME_CARDS_Y + CARD_H)
            {
                return -1;
            }
            else
            {
                if (x > WINDOW_W/2 - CARD_W - CARD_W - DISTANCE_BETWEEN_CARDS && x < WINDOW_W - CARD_W - DISTANCE_BETWEEN_CARDS)
                    return 0;
                if (x > WINDOW_W/2 - CARD_W && x < WINDOW_W)
                    return 1;
                if (x > WINDOW_W/2 + DISTANCE_BETWEEN_CARDS && x < WINDOW_W + DISTANCE_BETWEEN_CARDS + CARD_W)
                    return 2;
                if (x > WINDOW_W/2 + CARD_W + DISTANCE_BETWEEN_CARDS * 2 && x < WINDOW_W + 2 * CARD_W + DISTANCE_BETWEEN_CARDS * 2)
                    return 3;
                return -1;
            }
        }

        private int GetSelectedCard(int x, int y)
        {
            if (y > WINDOW_H - DISTANCE_BETWEEN_CARDS || y <  WINDOW_H - DISTANCE_BETWEEN_CARDS - CARD_H)
            {
                return -1;
            }
            for (int i = 0; i < 9; i++)
            {
                if (x > DISTANCE_BETWEEN_CARDS * (i+1) + CARD_W * i && x < DISTANCE_BETWEEN_CARDS * (i + 1) + CARD_W * (i + 1) && gameSession.PlayerCards[0].Count > i)
                    return i;
            }
            return -1;
        }

        private void SwitchCardsPage(object? sender, EventArgs e)
        {
            if ((sender as Button).Tag.ToString() != "1")
            {
                if (CurrCardsPage > 0)
                {
                    CurrCardsPage--;
                    sessionGraphics.CurrCardsPage--;
                }
                   
            }
            else 
            {
                if (CurrCardsPage < 6)
                {
                    CurrCardsPage++;
                    sessionGraphics.CurrCardsPage++;
                }
     
            }
        }

    }
}
