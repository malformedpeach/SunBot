using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SunBot.Services
{
    public class BlackjackService
    {
        private int _dealerCardValue;
        private int _playerCardValue;
        private List<int> _playingDeck;
        private List<int> _initialDeck;
        private int _cardsDrawnCount = 0;

        public BlackjackService()
        {
            _initialDeck = new List<int>();
            for(int i = 0; i < 52; i++)
            {
                _initialDeck.Add(i);
            }

            Random random = new Random();

            _playingDeck = new List<int>();
            _playingDeck = _initialDeck.OrderBy(x => random.Next()).ToList();
        }

        public Card DrawCard()
        {
            float cardValue = _playingDeck[_cardsDrawnCount];
            Card card = new Card(cardValue);
            
            _cardsDrawnCount++;

            return card;
        }
    }

    public class Card
    {
        public CardSuit Suit { get; set; }
        public CardRank Rank { get; set; }
        public int Value { get; set; }

        public Card(float cardValue)
        {
            // determine suit
            var suitValue = Math.Ceiling(cardValue / 13);

            // determine rank
            var rankValue = cardValue - (suitValue - 1) * 13;

            Suit = (CardSuit)suitValue;
            Rank = (CardRank)rankValue;

            switch (Rank)
            {
                case CardRank.Ace:
                    Value = 11;
                    break;
                case CardRank.Two:
                    Value = 2;
                    break;
                case CardRank.Three:
                    Value = 3;
                    break;
                case CardRank.Four:
                    Value = 4;
                    break;
                case CardRank.Five:
                    Value = 5;
                    break;
                case CardRank.Six:
                    Value = 6;
                    break;
                case CardRank.Seven:
                    Value = 7;
                    break;
                case CardRank.Eight:
                    Value = 8;
                    break;
                case CardRank.Nine:
                    Value = 9;
                    break;
                case CardRank.Ten:
                    Value = 10;
                    break;
                case CardRank.Jack:
                    Value = 10;
                    break;
                case CardRank.Queen:
                    Value = 10;
                    break;
                case CardRank.King:
                    Value = 10;
                    break;
            }
        }

        public enum CardSuit
        {
            Hearts = 1,
            Diamonds = 2,
            Spades = 3,
            Clubs = 4
        }
        public enum CardRank
        {
            Ace = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6,
            Seven = 7,
            Eight = 8,
            Nine = 9,
            Ten = 10,
            Jack = 11,
            Queen = 12,
            King = 13
        }
    }
}
