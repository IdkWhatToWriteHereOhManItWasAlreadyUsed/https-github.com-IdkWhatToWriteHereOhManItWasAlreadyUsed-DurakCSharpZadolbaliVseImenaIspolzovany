using System;
using System.DirectoryServices.ActiveDirectory;
using System.Text.RegularExpressions;

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
        private PictureBox? GamePictureBox;
        private readonly SessionGraphics sessionGraphics;
        private GameSession gameSession;
        private readonly NetworkClient networkClient;
        private int CurrCardsPage = 0;
        public int SelectedCardNum = -1;
        public string recipientId;

        public SessionController(GameSession gs, SessionGraphics graphics, NetworkClient network)
        {
            sessionGraphics = graphics;
            gameSession = gs;
            networkClient = network;
        }

        public void AssignControls(PictureBox pb, Button GrabButton, Button MoveTransferButton, Button DecCardsPageButton, Button IncCardsPageButton)
        {
            GamePictureBox = pb;
            GamePictureBox.MouseDown += HandleMousedown;
            GamePictureBox.MouseUp += HandleMouseUp;
            GrabButton.Click += HandleGrabClick;
            MoveTransferButton.Click += HandleMoveTransferClick;
            DecCardsPageButton.Click += SwitchCardsPage;
            IncCardsPageButton.Click += SwitchCardsPage;
        }

        private void HandleMousedown(object? sender, EventArgs e)
        {
            if (gameSession.CurrPlayerMove != 0)
                return;
            int x = (e as MouseEventArgs).X;
            int y = (e as MouseEventArgs).Y;
            if (gameSession.CurrPlayerAttacker == gameSession.CurrPlayerMove)
            {
                SelectedCardNum = GetSelectedCard(x, y);
                if (SelectedCardNum != -1)
                {
                    SelectedCardNum = SelectedCardNum + 9*CurrCardsPage;
                    sessionGraphics.SetSelection(SelectedCardNum);
                }
            }
            else
            {
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
            if (gameSession.CurrPlayerMove != 0)
                return;
            int x = (e as MouseEventArgs).X;
            int y = (e as MouseEventArgs).Y;
            if (gameSession.CurrPlayerAttacker == gameSession.CurrPlayerMove)
            {
                int selectedGameStack = GetSelectedGameStack(x, y);
                if (selectedGameStack != -1)
                {
                    sessionGraphics.ClearSelection();
                    if (selectedGameStack != -1 && SelectedCardNum != -1 && gameSession.CanPush(gameSession.PlayerCards[gameSession.CurrPlayerMove][SelectedCardNum], gameSession.GameStack[selectedGameStack]))
                    {
                        gameSession.DoMove(gameSession.PlayerCards[gameSession.CurrPlayerMove][SelectedCardNum], selectedGameStack);
                        _ = SendMoveToPlayer(SelectedCardNum, selectedGameStack);
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
                    if 
                    (
                        gameSession.GameStack[selectedGameStack].Count % 2 == 1 && selectedGameStack != -1 && SelectedCardNum != -1 && 
                        gameSession.CanBeat
                        (
                            gameSession.PlayerCards[gameSession.CurrPlayerMove][SelectedCardNum], 
                            gameSession.GameStack[selectedGameStack].First()
                        )
                    )
                    {
                        gameSession.DoMove(gameSession.PlayerCards[gameSession.CurrPlayerMove][SelectedCardNum], selectedGameStack);
                        _ = SendMoveToPlayer(SelectedCardNum, selectedGameStack);
                        SelectedCardNum = -1;
                    }
                }
            }
            sessionGraphics.ClearSelection();
        }

        private void HandleGrabClick(object? sender = null, EventArgs? e = null)
        {
            // если при нажатии кнопки не наш ход или мы атакуем
            if (sender != null && (gameSession.CurrPlayerAttacker == 0 || gameSession.CurrPlayerMove != 0))
                return;
            if (sender != null) // мы нажали на кнопку и мы защищаемся получается
                _ = SendMoveToPlayer(MoveType.mtGrab);
   
            gameSession.Grab();
            gameSession.GiveCardsAfterDefense();      
            gameSession.TransferMove(MoveType.mtGrab);
            sessionGraphics.UpdateGamefield(null);
        }

        private async void HandleMoveTransferClick(object? sender = null, EventArgs? e = null)
        {
            // УБРАТЬ ПОТОМ!!!!!!
            gameSession.PlayerCards[0].Clear();
            if (gameSession.IsGameFinished())
            {
                if (gameSession.GetWinner() == 0)
                    await sessionGraphics.ShowVictoryScreen();
                else
                    await sessionGraphics.ShowDefeatScreen();
                return;
            }

            // если при нажатии кнопки не наш ход 
            if (sender != null && gameSession.CurrPlayerMove != 0)
                return;
            if (sender == null) // сообщение от соперника
            {
                if (gameSession.CurrPlayerMove == gameSession.CurrPlayerAttacker) // игрок передает нам возможность отбиваться
                {
                   //
                }
                else // игрок отбился
                {
                    gameSession.GiveCardsAfterDefense();
                }                            
            }
            else // мы нажали кнопку передачи хода
            {            
                if (gameSession.CurrPlayerMove == gameSession.CurrPlayerAttacker) // если мы сейчас атаковали
                {
                    if (gameSession.AllStacksEmpty()) // если не положили ни одной карты
                        return;
                    //
                }
                else // если мы сейчас отбивались
                {
                    // если мы сейчас отбивались но всё не отбили
                    if (!gameSession.AllCardsBeaten())
                        return;
                    gameSession.GiveCardsAfterDefense();
                }
                _ = SendMoveToPlayer(MoveType.mtTransfer);
            }
           
            gameSession.TransferMove(MoveType.mtTransfer);
            sessionGraphics.UpdateGamefield(null);
        }

        public static int GetSelectedGameStack(int x, int y)
        {
            if (y < GAME_CARDS_Y || y > GAME_CARDS_Y + CARD_H)
            {
                return -1;
            }
            else
            {
                if (x > WINDOW_W/2 - CARD_W - CARD_W - DISTANCE_BETWEEN_CARDS && x < WINDOW_W/2 - CARD_W - DISTANCE_BETWEEN_CARDS)
                    return 0;
                if (x > WINDOW_W/2 - CARD_W && x < WINDOW_W/2)
                    return 1;
                if (x > WINDOW_W/2 + DISTANCE_BETWEEN_CARDS && x < WINDOW_W/2 + DISTANCE_BETWEEN_CARDS + CARD_W)
                    return 2;
                if (x > WINDOW_W/2 + CARD_W + DISTANCE_BETWEEN_CARDS * 2 && x < WINDOW_W/2 + CARD_W *2 + DISTANCE_BETWEEN_CARDS * 2)
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

        public async Task SendInitialisedSession(string recipient)
        {
            string sessionData = "";
            recipientId = recipient;
            for (int j = 0; j < gameSession.PlayerCards.Length; j++)
            {
                sessionData += "PLAYER" + j.ToString() + ':';
                for (int i = 0; i < gameSession.PlayerCards[j].Count; i++)
                {
                    sessionData += gameSession.PlayerCards[j][i].ToString();
                }
            }
            sessionData += "DECK:";
            for (int i = 0; i < gameSession.Deck.Count; i++)
            {
                sessionData += gameSession.Deck.ElementAt(i).ToString();
              //  Console.WriteLine(gameSession.Deck.ElementAt(i).ToString());
            }
            _ = networkClient.SendMessageAsync(recipient, sessionData);
        }

        public async Task RecieveInitialisedSession()
        {
            var waitTask = networkClient.WaitForMessageAsync(
               msg => msg.Contains("DECK"), 4900);
            var response = await waitTask;
            InitialiseSessionFromMessage(response);
        }

        public void InitialiseSessionFromMessage(string message)
        {
            Dictionary<string, List<Card>> keyValuePairs = ParseCardsMessage(message);
            for (int i = 0; i < gameSession.PlayerCards.Length; i++)
            {
                gameSession.PlayerCards[1-i] = keyValuePairs["PLAYER" + i.ToString()];
            }
            gameSession.Deck.Clear();
            for (int i = 0; i < 24; i++)
            {
                gameSession.Deck.Push(keyValuePairs["DECK"][23 - i]);
              //  Console.WriteLine(keyValuePairs["DECK"][23 -i].ToString()); 
            }
            gameSession.Trump = gameSession.Deck.ElementAt(gameSession.Deck.Count - 1).Suit;
            gameSession.CurrPlayerMove = 1; // ну потому что игроков всего два пока что, так что сойдет
            gameSession.CurrPlayerAttacker = 1;
        }

        private static Dictionary<string, List<Card>> ParseCardsMessage(string input)
        {
            var result = new Dictionary<string, List<Card>>();
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
            var keyMarkers = new[] { "PLAYER0:", "PLAYER1:", "DECK:", "CARD:", "STACK:" };
            var positions = keyMarkers
                .Select(marker => input.IndexOf(marker))
                .Where(pos => pos != -1)
                .ToList();

            return positions.Count == 0 ? -1 : positions.Min();
        }

        public async Task AwaitForPlayerMove()
        {
            A:
            var waitTask = networkClient.WaitForMessageAsync(
             msg => msg.Contains("STACK") || msg.Contains("GRAB") || msg.Contains("MOVE"), 60000);
            var response = await waitTask;
            ProcessRecievedMove(response);
            sessionGraphics.UpdateGamefield(null);
            goto A;
        }

        public async Task SendMoveToPlayer(int CardNum, int StackNum)
        {
            _ = networkClient.SendMessageAsync(recipientId, $"CARD:{CardNum.ToString()};STACK:{StackNum}");
        }

        public async Task SendMoveToPlayer(MoveType mt)
        {
            if (mt == MoveType.mtTransfer)
                await networkClient.SendMessageAsync(recipientId, "MOVE");
            else
                await networkClient.SendMessageAsync(recipientId, "GRAB");
        }

        private void ProcessRecievedMove(string response)
        {
            _ = networkClient.SendMessageAsync(recipientId, "GOT IT!");
            Console.WriteLine(response);
            if (response.Contains("GRAB"))
                HandleGrabClick(null);
            else
            {
                if (response.Contains("MOVE"))
                    HandleMoveTransferClick(null);
                else
                {
                    gameSession.DoMove(gameSession.PlayerCards[gameSession.CurrPlayerMove][GetFirstTwoNumbers(response)[0]], GetFirstTwoNumbers(response)[1]);
                }
            }              
        }

        private static int[] GetFirstTwoNumbers(string input)
        {
            input = input.Substring(FindNextKeyPosition(input));
            var matches = Regex.Matches(input, @"\d+");
            int[] numbers = new int[Math.Min(2, matches.Count)];

            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = int.Parse(matches[i].Value);
            }

            return numbers;
        }
    }
}
