using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Durak_.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace Durak_
{

    public enum MoveType
    {
        mtGrab,
        mtTransfer,
    }
    public struct Card
    {
        public int Suit;
        public int Power;
        public Card(int suit, int power)
        {
            this.Suit = suit;
            this.Power = power;
        }

        public override string ToString()
        {
            return $"{Suit},{Power};";

        }
    }

    public class GameSession
    {
        public List<Card>[] PlayerCards = new List<Card>[2];
        public Stack<Card> Deck;
        public Stack<Card>[] GameStack = new Stack<Card>[4];

        public int Trump;
        public int CurrPlayerMove;
        public int CurrPlayerAttacker;
        public Card SelectedCard;
        public bool SelectingAttacker = true;

        #region InitProcedures
        public GameSession()
        {
            InitializeDeck();
            GiveCards();
        }

        private void GiveCards()
        {
            for (int i = 0; i < PlayerCards.Length; i++)
            {
                PlayerCards[i] = [];
            }
                

            for (int i = 0; i < 6; i++)
            {
                foreach (var playerCards in PlayerCards)
                {
                    playerCards.Add(Deck.Pop());
                }
                Trump = Deck.ElementAt(Deck.Count() - 1).Suit;
            }
        }

        private void InitializeDeck()
        {
            int[] cardPairs = {
            1,6, 1,7, 1,8, 1,9, 1,10, 1,11, 1,12, 1,13, 1,14,
            2,6, 2,7, 2,8, 2,9, 2,10, 2,11, 2,12, 2,13, 2,14,
            3,6, 3,7, 3,8, 3,9, 3,10, 3,11, 3,12, 3,13, 3,14,
            0,6, 0,7, 0,8, 0,9, 0,10, 0,11, 0,12, 0,13, 0,14,
            0,0
        };

            var cards = new List<Card>();
            for (int i = 0; i < cardPairs.Length - 1; i += 2)
            {
                int suit = cardPairs[i];
                int power = cardPairs[i + 1];
                if (suit == 0 && power == 0)
                    continue;

                cards.Add(new Card(suit, power));
            }
            for(int i = 0; i < 4; ++i)
                GameStack[i] = [];
            Deck = new Stack<Card>(cards);
            Deck = new Stack<Card>(cards.OrderBy(x => Guid.NewGuid()));
        }

        #endregion

        #region GameLogic

        public bool CanPush(Card card, Stack<Card> stack)
        {
            if (CurrPlayerAttacker == CurrPlayerMove)
            {
                if (stack.Count % 2 == 0)
                {
                    foreach (var currStack in GameStack)
                    {
                        foreach (var stackCard in currStack)
                        {
                            if (stackCard.Power == card.Power)
                                return true;
                        }
                    }
                    foreach (var currStack in GameStack)
                        if (currStack.Count != 0)
                            return false;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (stack.Count % 2 == 1)
                {
                    if (CanBeat(card, stack.FirstOrDefault()))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool CanBeat(Card attacker, Card target)
        {
            // Если атакующая карта - козырь
            if (attacker.Suit == Trump)
            {
                // Если цель тоже козырь
                if (target.Suit == Trump)
                {
                    // Сравниваем по силе
                    return attacker.Power > target.Power;
                }
                // Если цель не козырь - атакующая бьёт
                return true;
            }

            // Если масти одинаковые (и не козырь)
            if (attacker.Suit == target.Suit)
            {
                // Сравниваем по силе
                return attacker.Power > target.Power;
            }

            // Если разные масти (и атакующая не козырь) - не может побить
            return false;
        }

        public void Grab()
        {
            foreach(Stack<Card> stack in GameStack)  
                while (stack.Count > 0) 
                    PlayerCards[CurrPlayerMove].Add(stack.Pop());
        }

        public void GiveCardsAfterDefense()
        {
            foreach(var player in PlayerCards)
            {
                while (player.Count < 6)
                    player.Add(Deck.Pop());
            }
        }

        #endregion

        public void DoMove(Card card, int gameStackNumber)
        {
            PlayerCards[CurrPlayerMove].Remove(card);
            GameStack[gameStackNumber].Push(card);
        }
        public void TransferMove(MoveType moveType)
        {
            if (IsGameFinished())
            {
                return;
            }
            if (moveType == MoveType.mtTransfer)
            {
                if (CurrPlayerMove == CurrPlayerAttacker) 
                {
                    if (CurrPlayerMove < PlayerCards.Length - 1)
                        CurrPlayerMove++;
                    else
                        CurrPlayerMove = 0; 
                }
                else
                {
                    CurrPlayerMove = CurrPlayerAttacker;
                }
            }
            else
            {
                if (CurrPlayerMove < PlayerCards.Length - 1)
                    CurrPlayerMove++;
                else
                    CurrPlayerMove = 0;
                CurrPlayerAttacker = CurrPlayerMove;
            }
        }

        public bool IsGameFinished()
        {
            foreach (var playerCards in PlayerCards)
            {
                if (playerCards.Count() == 0)
                    return true;
            }
            return false;
        }


    }
}
