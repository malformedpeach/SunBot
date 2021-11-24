using Discord;
using Discord.Rest;
using Discord.WebSocket;
using SunBot.Models;
using SunBot.Models.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private int _playerPoints;
        private int _dealerPoints;
        private int _cardsDrawnCount = 0;

        //private ulong _playerId;
        private SocketUser _user;
        private GameState _gameState = GameState.End;

        // members
        private static int IMAGE_HEIGHT = 325;
        private static int IMAGE_WIDTH = 245;
        private static string FACE_DOWN_CARD = "Resources/PNGCards/face_down.png";
        private static Emoji HIT_EMOJI = new Emoji("👍");
        private static Emoji STAND_EMOJI = new Emoji("✋");

        //private List<BlackjackGame> _games;
        private List<int> _initialDeck;
        private Configuration _config;
        private DiscordSocketClient _client;

        // Test
        private RestUserMessage _theMessage;

        public BlackjackService(Configuration config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;

            client.ReactionAdded += React;

            _initialDeck = new List<int>();
            for(int i = 1; i < 53; i++)
            {
                _initialDeck.Add(i);
            }
        }

        public async void StartGameAsync(SocketUser user)
        {
            if (_gameState == GameState.Playing) // _playing
            {
                await _config.DefaultTextChannel.SendMessageAsync($"Game running: {_user.Username} is hogging the seat!");
                return;
            }

            //_playing = true;
            _gameState = GameState.Playing;
            //_playerId = playerId;
            _user = user;
            

            ShuffleDeck();
            await _config.DefaultTextChannel.SendMessageAsync("Deck shuffled, let's play some cards!");

            // Reset player hand
            _playerCards = new List<Card>();

            // Reset dealer hand
            _dealerCards = new List<Card>();

            // Deal and show result
            InitialDeal();
            SendActionResult();
        }

        public void Hit()
        {
            if (_gameState == GameState.End) // !_playing
            {
                return;
            }

            // Draw card
            _playerCards.Add(DrawCard());
            _playerPoints = _playerCards.Sum(x => x.Value);
            if (_playerPoints > 21) _gameState = GameState.End;

            // Send card composite
            SendActionResult();
        }

        public void Stand()
        {
            if (_gameState == GameState.End) // !_playing
            {
                return;
            }

            // Draw cards according to the rulesthingamajig
            //_playerStand = true;
            _gameState = GameState.PlayerStand;

            // Process stand
            ProcessStand();
        }

        private async void ProcessStand()
        {
            while (_gameState == GameState.PlayerStand)
            {
                _dealerPoints = _dealerCards.Sum(x => x.Value);

                if (_dealerPoints < 17 && _dealerPoints < _playerPoints)
                {
                    // Draw card
                    _dealerCards.Add(DrawCard());
                    _dealerPoints = _dealerCards.Sum(x => x.Value);

                    // report progress to player
                    SendActionResult();

                    // wait
                    await Task.Delay(3000);
                }
                else // bust or satisfied with cards
                {
                    //_dealerStand = true;
                    _gameState = GameState.End;
                    SendActionResult();
                }
            }
        }

        private async void SendActionResult()
        {
            StringBuilder builder = new StringBuilder();
            
            // Player cards and points
            builder.AppendLine("**Your cards**");
            foreach (var card in _playerCards)
            {
                builder.AppendLine($"{card.Rank} of {card.Suit} = {card.Value}");
            }
            builder.AppendLine($"Player points: {_playerPoints}");
            // -----------------------

            // Dealer cards and points
            builder.AppendLine("**Dealer cards**");
            
            for(int i = 0; i < _dealerCards.Count; i++)
            {
                var card = _dealerCards[i];

                if (_gameState == GameState.Playing && i == 0)
                {
                    builder.AppendLine($"Face down card = ?");
                }
                else
                {
                    builder.AppendLine($"{card.Rank} of {card.Suit} = {card.Value}");
                }
            }
            
            if (_gameState == GameState.Playing)
            {
                builder.AppendLine($"Dealer points: {_dealerCards[1].Value}");
            }
            else builder.AppendLine($"Dealer points: {_dealerPoints}");
            // -----------------------

            builder.AppendLine(); // padding line

            // Gamestate
            if (_gameState == GameState.End)
            {
                // report result
                if (_playerPoints > 21) builder.AppendLine("**Bust! House wins :^)**");
                else if (_dealerPoints > 21) builder.AppendLine("**Bust! You win :^)**");
                else if (_playerPoints > _dealerPoints) builder.AppendLine("**Your hand wins! :^)**");
                else if (_dealerPoints > _playerPoints) builder.AppendLine("**House hand wins! :^)**");
                else if (_dealerPoints == _playerPoints) builder.AppendLine("**Push! refunds galore! :^)**");
            }
            else if (_gameState == GameState.PlayerStand)
            {
                builder.AppendLine("**Processing..**");
            }
            else
            {
                builder.AppendLine("**Hit or stand?**");
            }

            // Create embed
            var embed = new EmbedBuilder
            {
                Title = "Testing!",
                Description = $"{builder}",
                ImageUrl = "attachment://myimage.png"
            };
            embed.Footer = new EmbedFooterBuilder
            {
                Text = $"{HIT_EMOJI} Hit! | {STAND_EMOJI} Stand."
            };

            await using MemoryStream memoryStream = CreateCardImageMemoryStream();
            _theMessage = await _config.DefaultTextChannel.SendFileAsync(memoryStream, "myimage.png", "", embed: embed.Build());
            await _theMessage.AddReactionAsync(HIT_EMOJI);
            await _theMessage.AddReactionAsync(STAND_EMOJI);
        }



        private void InitialDeal()
        {
            _playerPoints = 0;
            _dealerPoints = 0;

            _playerCards.Add(DrawCard());
            _dealerCards.Add(DrawCard());
            _playerCards.Add(DrawCard());
            _dealerCards.Add(DrawCard());

            _playerPoints = _playerCards.Sum(x => x.Value);
            _dealerPoints = _dealerCards.Sum(x => x.Value);
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
    
        private MemoryStream CreateCardImageMemoryStream()
        {
            int highestCardCount = _playerCards.Count > _dealerCards.Count ? _playerCards.Count : _dealerCards.Count;
            using Bitmap compositeBitmap = new Bitmap(IMAGE_WIDTH * highestCardCount, IMAGE_HEIGHT * 2);
            using Graphics canvas = Graphics.FromImage(compositeBitmap);

            for (int i = 0; i < _playerCards.Count; i++)
            {
                using Image image = Image.FromFile(_playerCards[i].LocalImageUrl);
                canvas.DrawImage(image, new Point(IMAGE_WIDTH * i, 0));
            }

            for (int i = 0; i < _dealerCards.Count; i++)
            {
                if (i == 0 && _gameState == GameState.Playing)
                {
                    using Image image = Image.FromFile(FACE_DOWN_CARD);
                    canvas.DrawImage(image, new Point(IMAGE_WIDTH * i, IMAGE_HEIGHT));
                }
                else
                {
                    using Image image = Image.FromFile(_dealerCards[i].LocalImageUrl);
                    canvas.DrawImage(image, new Point(IMAGE_WIDTH * i, IMAGE_HEIGHT));
                }
            }

            canvas.Save();
            MemoryStream memoryStream = new MemoryStream();
            compositeBitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;

            return memoryStream;
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
            _theMessage = await _config.DefaultTextChannel.SendFileAsync(memoryStream, "myimage.png", "foobar", embed: embed.Build());
            await _theMessage.AddReactionAsync(HIT_EMOJI);
            await _theMessage.AddReactionAsync(STAND_EMOJI);
        }

        public async Task React(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) return;
            else if (reaction.User.Value.Id == _user.Id)
            {
                if (reaction.Emote.Name == HIT_EMOJI.Name) Hit();
                else if (reaction.Emote.Name == STAND_EMOJI.Name) Stand();
            }
        }
    }
}