using SunBot.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SunBot.Models
{
    public class Card
    {
        public CardSuit Suit { get; set; }
        public CardRank Rank { get; set; }
        public int Value { get; set; }
        public string LocalImageUrl { get; set; }

        public Card(float cardValue)
        {
            // determine suit
            var suitValue = Math.Ceiling(cardValue / 13);

            // determine rank
            var rankValue = cardValue - (suitValue - 1) * 13;

            Suit = (CardSuit)suitValue;
            Rank = (CardRank)rankValue;

            // set value
            if (Rank == CardRank.Jack ||
                Rank == CardRank.King ||
                Rank == CardRank.Queen)
            {
                Value = 10;
            }
            else if (Rank == CardRank.Ace)
            {
                Value = 11;
            }
            else
            {
                Value = (int)Rank;
            }

            // set local img url
            if (Rank != CardRank.Ace &&
                Rank != CardRank.Jack &&
                Rank != CardRank.King &&
                Rank != CardRank.Queen)
            {
                LocalImageUrl = $"Resources/PNGCards/{((int)Rank).ToString().ToLower()}_of_{Suit.ToString().ToLower()}.png";
            }
            else
            {
                LocalImageUrl = $"Resources/PNGCards/{Rank.ToString().ToLower()}_of_{Suit.ToString().ToLower()}.png";
            }
        }
    }
}
