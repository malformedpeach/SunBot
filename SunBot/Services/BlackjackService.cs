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
        private List<Card> _dealerCards;
        private List<int> _playingDeck;
        private int _dealerPoints;
        private int _cardsDrawnCount = 0;
        private GameState _gameState = GameState.End;


        // members
        private static int IMAGE_HEIGHT = 325;
        private static int IMAGE_WIDTH = 245;
        private static int PLAYER_CAPACITY = 2;
        private static string FACE_DOWN_CARD = "Resources/PNGCards/face_down.png";
        private static Emoji HIT_EMOJI = new Emoji("👍");
        private static Emoji STAND_EMOJI = new Emoji("✋");



        private List<BlackjackPlayer> _players;
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
            for (int i = 1; i < 53; i++)
            {
                _initialDeck.Add(i);
            }

            _players = new List<BlackjackPlayer>();
        }


        #region Commands

        public async void StartGameAsync()
        {
            if (_players.Count == 0)
            {
                await _config.DefaultTextChannel.SendMessageAsync("No players at the table!");
                return;
            }

            if (_gameState == GameState.Playing) // _playing
            {
                await _config.DefaultTextChannel.SendMessageAsync($"A game is currently in progress.");
                return;
            }

            _gameState = GameState.Playing;

            ShuffleDeck();
            await _config.DefaultTextChannel.SendMessageAsync("Deck shuffled, let's play some cards!");

            // Reset players hands
            foreach (var player in _players)
            {
                player.Cards = new List<Card>();
                player.State = PlayerState.Playing;
            }

            // Reset dealer hand
            _dealerCards = new List<Card>();

            // Deal and show result
            InitialDeal();
            SendActionResult();
        }

        public async void JoinTable(SocketUser user)
        {
            // if game is already running, prevent join
            if (_gameState == GameState.Playing)
            {
                await _config.DefaultTextChannel.SendMessageAsync("Can't join a game in progress.");
                return;
            }

            // if player is already in game, prevent join
            if (_players.Any(x => x.User == user))
            {
                await _config.DefaultTextChannel.SendMessageAsync("You already joined!");
                return;
            }


            if (_players.Count == PLAYER_CAPACITY)
                await _config.DefaultTextChannel.SendMessageAsync("No seats available! :(");
            else
            {
                var newPlayer = new BlackjackPlayer
                {
                    State = PlayerState.Playing,
                    User = user,
                    Cards = new List<Card>()
                };
                _players.Add(newPlayer);
                await _config.DefaultTextChannel.SendMessageAsync("Joined table!");

                // Show players
            }
        }

        #endregion


        #region Player actions
        private void Hit(BlackjackPlayer currentPlayer)
        {
            if (_gameState == GameState.End) // !_playing
            {
                return;
            }
            else if (currentPlayer.State == PlayerState.Playing)
            {
                currentPlayer.State = PlayerState.Hit;
                Update();
            }
        }

        private void Stand(BlackjackPlayer currentPlayer)
        {
            if (_gameState == GameState.End) // !_playing
            {
                return;
            }
            else if (currentPlayer.State == PlayerState.Playing)
            {
                currentPlayer.State = PlayerState.Stand;
                Update();
            }
        }

        #endregion


        #region Update logic and result 

        private async void Update()
        {
            // Do update logic if all players have made their moves
            if (_players.All(x => x.State != PlayerState.Playing))
            {
                foreach (var player in _players)
                {
                    if (player.State == PlayerState.Hit)
                    {
                        player.Cards.Add(DrawCard());

                        if (player.Points > 21)
                            player.State = PlayerState.Bust;

                        else
                            player.State = PlayerState.Playing;
                    }
                }


                if (_players.All(x => x.State >= PlayerState.Stand))
                {
                    _gameState = GameState.PlayerStand;
                    ProcessStand();
                }
                else
                {
                    SendActionResult();
                }
            }
        }

        private async void ProcessStand()
        {
            while (_gameState == GameState.PlayerStand)
            {
                _dealerPoints = _dealerCards.Sum(x => x.Value);

                if (_dealerPoints < 17)
                {
                    foreach (var player in _players)
                    {
                        if (_dealerPoints > player.Points)
                            continue;
                        else
                        {
                            // Draw card
                            _dealerCards.Add(DrawCard());
                            _dealerPoints = _dealerCards.Sum(x => x.Value);

                            // report progress to player
                            SendActionResult();

                            // wait
                            await Task.Delay(3000);
                        }
                    }

                }
                else // bust or satisfied with cards
                {
                    _gameState = GameState.End;
                    SendActionResult();
                }
            }
        }

        private async void SendActionResult()
        {
            StringBuilder builder = new StringBuilder();


            // Dealer cards and points
            builder.AppendLine("**Dealer cards**");

            //for (int i = 0; i < _dealerCards.Count; i++)
            //{
            //    var card = _dealerCards[i];

            //    if (_gameState == GameState.Playing && i == 0)
            //    {
            //        builder.AppendLine($"Face down card = ?");
            //    }
            //    else
            //    {
            //        builder.AppendLine($"{card.Rank} of {card.Suit} = {card.Value}");
            //    }
            //}

            if (_gameState == GameState.Playing)
            {
                builder.AppendLine($"Dealer points: {_dealerCards[1].Value}");
            }
            else builder.AppendLine($"Dealer points: {_dealerPoints}");
            // -----------------------


            // Player cards and points
            foreach (var player in _players)
            {
                builder.AppendLine($"{player.User.Mention} **{player.State}**");
                //foreach (var card in player.Cards)
                //{
                //    builder.AppendLine($"{card.Rank} of {card.Suit} = {card.Value}");
                //}
                builder.AppendLine($"Total points: {player.Points}");
            }
            // -----------------------
            

            builder.AppendLine(); // padding line

            // Gamestate
            if (_gameState == GameState.End)
            {
                // report result
                builder.AppendLine("**Results!**");

                foreach (var player in _players)
                {
                    if (player.Points > 21) 
                        builder.AppendLine($"{player.User.Mention} **Bust! House wins :^)**");

                    else if (_dealerPoints > 21) 
                        builder.AppendLine($"{player.User.Mention} **Bust! You win :^)**");

                    else if (player.Points > _dealerPoints) 
                        builder.AppendLine($"{player.User.Mention} **Your hand wins! :^)**");

                    else if (player.Points < _dealerPoints) 
                        builder.AppendLine($"{player.User.Mention} **House hand wins! :^)**");

                    else if (player.Points == _dealerPoints) 
                        builder.AppendLine($"{player.User.Mention} **Push! refunds galore! :^)**");
                }
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

            if (_theMessage != null)
            {
                await _theMessage.DeleteAsync();
            }
            _theMessage = await _config.DefaultTextChannel.SendFileAsync(memoryStream, "myimage.png", "", embed: embed.Build());

            if (_gameState == GameState.Playing)
            {
                await _theMessage.AddReactionAsync(HIT_EMOJI);
                await _theMessage.AddReactionAsync(STAND_EMOJI);
            }
        }

        #endregion
        

        #region Deck functions

        private void InitialDeal()
        {
            // reset points
            _dealerPoints = 0;

            // Initial card draws
            for (int i = 0; i < 2; i++)
            {
                foreach (var player in _players)
                {
                    player.Cards.Add(DrawCard());
                }
                _dealerCards.Add(DrawCard());
            }
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

        #endregion




        private MemoryStream CreateCardImageMemoryStream()
        {
            //int highestCardCount = _playerCards.Count > _dealerCards.Count ? _playerCards.Count : _dealerCards.Count;

            // Determine card count (width of composite img)
            var highestPlayerCardCount = 0;

            foreach (var player in _players)
            {
                if (player.Cards.Count > highestPlayerCardCount)
                    highestPlayerCardCount = player.Cards.Count;
            }

            int highestCardCount = _dealerCards.Count > highestPlayerCardCount ? 
                _dealerCards.Count : highestPlayerCardCount;

            // determine player count (height of composite img)
            int playerCount = _players.Count + 1; // + 1 (dealer)

            // create bitmap with size values
            using Bitmap compositeBitmap = new Bitmap(IMAGE_WIDTH * highestCardCount, IMAGE_HEIGHT * playerCount);
            using Graphics canvas = Graphics.FromImage(compositeBitmap);

            // draw dealer hand
            for (int i = 0; i < _dealerCards.Count; i++)
            {
                if (i == 0 && _gameState == GameState.Playing)
                {
                    using Image image = Image.FromFile(FACE_DOWN_CARD);
                    canvas.DrawImage(image, new Point(IMAGE_WIDTH * i, 0));
                }
                else
                {
                    using Image image = Image.FromFile(_dealerCards[i].LocalImageUrl);
                    canvas.DrawImage(image, new Point(IMAGE_WIDTH * i, 0));
                }
            }

            // draw players hands
            for (int x = 0; x < _players.Count; x++)
            {
                for (int y = 0; y < _players[x].Cards.Count; y++)
                {
                    using Image image = Image.FromFile(_players[x].Cards[y].LocalImageUrl);
                    canvas.DrawImage(image, new Point(IMAGE_WIDTH * y, IMAGE_HEIGHT * (x + 1)));
                }
            }
            //for (int i = 0; i < _playerCards.Count; i++)
            //{
            //    using Image image = Image.FromFile(_playerCards[i].LocalImageUrl);
            //    canvas.DrawImage(image, new Point(IMAGE_WIDTH * i, 0));
            //}

            // TEXT TESTING
            //Font font = new Font("Arial", 20.0f);
            //SolidBrush brush = new SolidBrush(System.Drawing.Color.Red);
            //canvas.DrawString("foobar!", font, brush, new PointF(0, 0));
            // ------------


            canvas.Save();
            MemoryStream memoryStream = new MemoryStream();
            compositeBitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;

            return memoryStream;
        }

        // Testing
        public async Task React(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot) return;
            else if (_players.Any(x => x.User.Id == reaction.User.Value.Id))
            {
                var player = _players.First(x => x.User.Id == reaction.User.Value.Id);
                if (reaction.Emote.Name == HIT_EMOJI.Name) Hit(player);
                else if (reaction.Emote.Name == STAND_EMOJI.Name) Stand(player);
            }
        }
    }
}