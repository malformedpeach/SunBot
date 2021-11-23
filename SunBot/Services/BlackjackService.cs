using Discord;
using SunBot.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

// TODO: Seperate class for image related stuff?
using Image = System.Drawing.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace SunBot.Services
{
    public class BlackjackService
    {
        // TODO: 'Game' class
        private List<Card> _dealerCards;
        private List<Card> _playerCards;
        private List<int> _playingDeck;
        private bool _playing = false;
        private int _cardsDrawnCount = 0;

        // members
        private static int IMAGE_HEIGHT = 325;
        private static int IMAGE_WIDTH = 245;
        
        private List<int> _initialDeck;
        private Configuration _config;

        public BlackjackService(Configuration config)
        {
            _config = config;

            _initialDeck = new List<int>();
            for(int i = 1; i < 53; i++)
            {
                _initialDeck.Add(i);
            }
        }

        public async void StartGameAsync()
        {
            if (_playing)
            {
                await _config.DefaultTextChannel.SendMessageAsync("You already have a game running!");
                return;
            }

            _playing = true;
            ShuffleDeck();
            await _config.DefaultTextChannel.SendMessageAsync("Deck shuffled, let's play some cards!");


            // Draw card for player
            _playerCards = new List<Card>();
            _playerCards.Add(DrawCard());

            // Draw card for dealer, face down, display?
            _dealerCards = new List<Card>();
            _dealerCards.Add(DrawCard());

            // Send card composite
            SendActionResult(_playerCards);
        }

        public async void HitAsync()
        {
            if (!_playing)
            {
                await _config.DefaultTextChannel.SendMessageAsync("Please start a game before calling this action!");
                return;
            }

            // Draw card
            _playerCards.Add(DrawCard());

            // Check if bust


            // Send card composite
            SendActionResult(_playerCards);
        }

        public async void StandAsync()
        {
            if (!_playing)
            {
                await _config.DefaultTextChannel.SendMessageAsync("Please start a game before calling this action!");
                return;
            }

            // Draw cards according to the rulesthingamajig
            throw new NotImplementedException();
        }


        private async void SendActionResult(List<Card> drawnCards)
        {
            using Bitmap compositeBitmap = new Bitmap(IMAGE_WIDTH * drawnCards.Count, IMAGE_HEIGHT);
            using Graphics canvas = Graphics.FromImage(compositeBitmap);

            // Compose image
            for (int i = 0; i < drawnCards.Count; i++)
            {
                using Image image = Image.FromFile(drawnCards[i].LocalImageUrl);
                canvas.DrawImage(image, new Point(IMAGE_WIDTH * i, 0));
            }
            canvas.Save();

            // load image into memory stream
            await using MemoryStream memoryStream = new MemoryStream();
            compositeBitmap.Save(memoryStream, ImageFormat.Png);


            // Create embed
            int playerPoints = 0;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Your cards: ");

            foreach (var card in drawnCards)
            {
                builder.AppendLine($"{card.Rank} of {card.Suit}");
                playerPoints += card.Value;
            }
            builder.AppendLine($"Current points: {playerPoints}");

            if (playerPoints > 21) 
            {
                builder.AppendLine("Bust! house wins :^)");
                _playing = false;
            }
            else builder.AppendLine("Hit or stand?");
            
            var embed = new EmbedBuilder
            {
                Title = "Testing!",
                Description = $"{builder}",
                ImageUrl = "attachment://myimage.png"
            };

            // 'Rewind' stream and send
            memoryStream.Position = 0;
            await _config.DefaultTextChannel.SendFileAsync(memoryStream, "myimage.png", "", embed: embed.Build());
        }

        private void ShuffleDeck()
        {
            Random random = new Random();
            _playingDeck = new List<int>();
            _playingDeck = _initialDeck.OrderBy(x => random.Next()).ToList();
            _cardsDrawnCount = 0;
        }

        private Card DrawCard()
        {
            if (_cardsDrawnCount > _playingDeck.Count)
            {
                Console.WriteLine("BlackjackService.DrawCard: Deck is empty.");
                return null;
            }

            float cardValue = _playingDeck[_cardsDrawnCount];
            Card card = new Card(cardValue);
            
            _cardsDrawnCount++;

            return card;
        }
    
        // Testing
        public async void Foo()
        {
            ShuffleDeck();
            List<Card> cardList = new List<Card>();

            for(int i = 0; i < 10; i++)
            {
                cardList.Add(DrawCard());
            }

            //using Image firstImage = Image.FromFile(cardList[0].LocalImageUrl);
            using Bitmap compositeBitmap = new Bitmap(IMAGE_WIDTH * cardList.Count, IMAGE_HEIGHT);
            using Graphics canvas = Graphics.FromImage(compositeBitmap);

            // Compose image
            for (int i = 0; i < cardList.Count; i++)
            {
                using Image image = Image.FromFile(cardList[i].LocalImageUrl);
                canvas.DrawImage(image, new Point(IMAGE_WIDTH * i, 0));
            }
            canvas.Save();

            // load image into memory stream
            await using MemoryStream memoryStream = new MemoryStream();
            compositeBitmap.Save(memoryStream, ImageFormat.Png);

            var embed = new EmbedBuilder
            {
                Title = "Testing!",
                Description = $"Composite image test",
                ImageUrl = "attachment://myimage.png"
            };

            // 'Rewind' stream and send
            memoryStream.Position = 0;
            await _config.DefaultTextChannel.SendFileAsync(memoryStream, "myimage.png", "foobar", embed: embed.Build());
        }
    }
}
