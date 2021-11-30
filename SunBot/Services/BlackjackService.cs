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

using Image = System.Drawing.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace SunBot.Services
{
    public class BlackjackService
    {
        private static int IMAGE_HEIGHT = 325;
        private static int IMAGE_WIDTH = 230;
        private static int PLAYER_CAPACITY = 6;
        private static string FACE_DOWN_CARD = "Resources/PNGCards/face_down.png";
        
        // In-game emojis
        private static Emoji HIT_EMOJI = new Emoji("👍");
        private static Emoji STAND_EMOJI = new Emoji("✋");

        // Menu emojis
        private static Emoji START_EMOJI = new Emoji("✔️"); // Start/Play again
        private static Emoji END_EMOJI = new Emoji("❌"); // End session
        private static Emoji RETURN_EMOJI = new Emoji("🔙"); // return to session menu
        private static Emoji JOIN_TABLE_EMOJI = new Emoji("⬆️");// Join table
        private static Emoji LEAVE_TABLE_EMOJI = new Emoji("⬇️"); // Leave table

        private List<Card> _dealerCards;
        private List<int> _playingDeck;
        private int _dealerPoints;
        private int _cardsDrawnCount = 0;
        private GameState _gameState = GameState.SessionEnded;

        private List<BlackjackPlayer> _players;
        private List<int> _initialDeck;
        private Configuration _config;

        private RestUserMessage _sessionMessage;


        public BlackjackService(Configuration config, DiscordSocketClient client)
        {
            _config = config;

            client.ReactionAdded += HandleReaction;

            _initialDeck = new List<int>();
            for (int i = 1; i < 53; i++)
            {
                _initialDeck.Add(i);
            }

            _players = new List<BlackjackPlayer>();
        }


        #region Commands

        public async void StartSession()
        {
            if (_gameState == GameState.SessionStarted)
                return;

            // Get menu embed
            var embed = await GetMenuEmbedAsync();

            if (_gameState == GameState.End)
            {
                await _sessionMessage.DeleteAsync();
                _sessionMessage = await _config.DefaultTextChannel.SendMessageAsync(embed: embed);
            }
            else if (_sessionMessage != null && _sessionMessage.Reference != null)
                await _sessionMessage.ModifyAsync(x => x.Embed = embed);
            else
                _sessionMessage = await _config.DefaultTextChannel.SendMessageAsync(embed: embed);

            _gameState = GameState.SessionStarted;

            // Add reactions
            await _sessionMessage.AddReactionAsync(JOIN_TABLE_EMOJI);
            await _sessionMessage.AddReactionAsync(LEAVE_TABLE_EMOJI);
            await _sessionMessage.AddReactionAsync(START_EMOJI);
            await _sessionMessage.AddReactionAsync(END_EMOJI);
        }

        #endregion

        #region Menu actions

        private async void StartGame()
        {
            _gameState = GameState.Playing;

            ResetGame();
            InitialDeal();
            Draw();
        }

        private async void EndSession()
        {
            await _sessionMessage.DeleteAsync();
            _gameState = GameState.SessionEnded;

            _players.Clear();

            // reset other stuff
        }

        private async void JoinTable(IUser user)
        {
            if (_players.Count >= PLAYER_CAPACITY)
                return;

            if (_players.Any(x => x.User == user))
                return;

            // Add new player
            var player = new BlackjackPlayer
            {
                State = PlayerState.Playing,
                User = user,
                Cards = new List<Card>(),
            };
            _players.Add(player);

            // Modify session message
            var embed = await GetMenuEmbedAsync();
            await _sessionMessage.ModifyAsync(x => x.Embed = embed);
        }

        private async void LeaveTable(ulong userId)
        {
            if (_players.Any(x => x.User.Id == userId)) 
            {
                _players.RemoveAt(_players.FindIndex(x => x.User.Id == userId));

                var embed = await GetMenuEmbedAsync();
                await _sessionMessage.ModifyAsync(x => x.Embed = embed);
            }
        }

        #endregion

        #region Game actions
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


        
        #region Game logic & Draw method

        private void Update()
        {
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
                    Draw();
                }
            }
        }

        private async void ProcessStand()
        {
            int highestPlayerScore = 0;

            foreach (var player in _players)
            {
                if (player.Points > highestPlayerScore && player.Points < 21)
                    highestPlayerScore = player.Points;
            }

            // Reveal down card
            Draw();
            await Task.Delay(5000);

            while (_gameState == GameState.PlayerStand)
            {
                _dealerPoints = _dealerCards.Sum(x => x.Value);

                if (_players.All(x => x.State == PlayerState.Bust))
                {
                    _gameState = GameState.End;
                    Draw();
                }
                else if (_dealerPoints < 17 && _dealerPoints < highestPlayerScore)
                {
                    //Draw card
                    _dealerCards.Add(DrawCard());
                    _dealerPoints = _dealerCards.Sum(x => x.Value);

                    // report progress to player
                    Draw();

                    // wait
                    await Task.Delay(5000);
                }
                else // bust or satisfied with cards
                {
                    _gameState = GameState.End;
                    Draw();
                }
            }
        }

        private async void Draw()
        {
            var embed = await GetGameEmbedAsync();
            await using MemoryStream memoryStream = CreateCardImageMemoryStream();

            if (_sessionMessage != null)
            {
                await _sessionMessage.DeleteAsync();
            }
            _sessionMessage = await _config.DefaultTextChannel.SendFileAsync(memoryStream, "myimage.png", "", embed: embed);

            // Add reactions
            if (_gameState == GameState.End)
            {
                await _sessionMessage.AddReactionAsync(RETURN_EMOJI); // return to menu
                await _sessionMessage.AddReactionAsync(START_EMOJI); // Play again
                await _sessionMessage.AddReactionAsync(END_EMOJI); // End session
            }
            if (_gameState == GameState.Playing)
            {
                await _sessionMessage.AddReactionAsync(HIT_EMOJI);
                await _sessionMessage.AddReactionAsync(STAND_EMOJI);
            }
        }

        #endregion

        #region Deck functions

        private async void InitialDeal()
        {
            // reset points
            _dealerPoints = 0;

            // Initial card draws
            foreach (var player in _players)
            {
                player.Cards.Add(DrawCard());
            }

            _dealerCards.Add(DrawCard());
            _dealerPoints = _dealerCards.Sum(x => x.Value);
        }

        private void ResetGame()
        {
            // Reset and shuffle deck
            Random random = new Random();
            _playingDeck = new List<int>();
            _playingDeck = _initialDeck.OrderBy(x => random.Next()).ToList();
            _cardsDrawnCount = 0;

            // Reset player hands
            foreach (var player in _players)
            {
                player.Cards = new List<Card>();
                player.State = PlayerState.Playing;
            }

            // Reset dealer hand
            _dealerCards = new List<Card>();
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


        private async Task<Embed> GetMenuEmbedAsync()
        {
            var builder = new StringBuilder();
            builder.AppendLine("**Blackjack** (With slightly modified rules!)");
            builder.AppendLine();
            builder.AppendLine("**Table rules**");
            builder.AppendLine("When starting a new game i will deal 1 card to each player (face up) and 1 card to myself (face down).\n" +
                               "You can then ask for more cards (**hit**) until you are satisfied with your hand (**stand**) or go over 21 (**bust**).\n" +
                               "When all players have gone **bust** or choose to **stand** i will begin drawing cards in a attempt to beat your hand(s).");
            builder.AppendLine();
            builder.AppendLine("**Point values**");
            builder.AppendLine("1    = Ace\n" +
                               "10   = Face card (Jack, Queen, King)\n" +
                               "2-10 = Numbered cards");
            builder.AppendLine();
            builder.AppendLine("**Win conditions**");
            builder.AppendLine("If you go bust, the house wins! (even if i go bust trying to beat other players hands)\n" +
                               "If i go bust, you win!\n" +
                               "If your hand beats my hand, you win!\n" +
                               "If my hand beats your hand, the house wins!");
            builder.AppendLine();
            builder.AppendLine("Have fun!");
            builder.AppendLine();

            builder.AppendLine($"**Table** (Player capacity: {PLAYER_CAPACITY})");

            if (_players.Count == 0)
                builder.AppendLine("No players at the table!");
            else
            {
                foreach (var player in _players)
                {
                    builder.AppendLine($"{player.User.Mention}");
                }
            }

            builder.AppendLine();

            var footer = new EmbedFooterBuilder
            {
                Text = $"{JOIN_TABLE_EMOJI} Join Table | {LEAVE_TABLE_EMOJI} Leave table | {START_EMOJI} Start game | {END_EMOJI} End session"
            };
            var embed = new EmbedBuilder();
            embed.Title = "Blackjack session";
            embed.Description = builder.ToString();
            embed.Color = Discord.Color.Gold;
            embed.Footer = footer;

            return embed.Build();
        }

        private async Task<Embed> GetGameEmbedAsync()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var player in _players)
            {
                builder.AppendLine($"{player.User.Mention}: {player.Points} | **{player.State}**");
            }

            builder.AppendLine(); // padding line

            if (_gameState == GameState.End)
            {
                builder.AppendLine("**Results!**");

                foreach (var player in _players)
                {
                    if (player.Points > 21)
                        builder.AppendLine($"{player.User.Mention} **Bust! House wins**");

                    else if (_dealerPoints > 21)
                        builder.AppendLine($"{player.User.Mention} **Bust! You win**");

                    else if (player.Points > _dealerPoints)
                        builder.AppendLine($"{player.User.Mention} **Your hand wins!**");

                    else if (player.Points < _dealerPoints)
                        builder.AppendLine($"{player.User.Mention} **House hand wins!**");

                    else if (player.Points == _dealerPoints)
                        builder.AppendLine($"{player.User.Mention} **Push! refunds galore!**");
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
                Title = "Blackjack session",
                Description = $"{builder}",
                ImageUrl = "attachment://myimage.png",
                Color = Discord.Color.Gold
            };
            var footer = new EmbedFooterBuilder();

            // Footer info
            if (_gameState == GameState.End)
            {
                footer.Text = $"{RETURN_EMOJI} Return to menu | {START_EMOJI} Play again | {END_EMOJI} End session";
            }
            else if (_gameState == GameState.Playing)
            {
                footer.Text = $"{HIT_EMOJI} Hit! | {STAND_EMOJI} Stand.";
            }
            embed.Footer = footer;


            return embed.Build();
        }



        private MemoryStream CreateCardImageMemoryStream()
        {
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
            using Bitmap compositeBitmap = new Bitmap(IMAGE_WIDTH * highestCardCount, (IMAGE_HEIGHT + 50) * playerCount);
            using Graphics canvas = Graphics.FromImage(compositeBitmap);

            // Font and brush for text
            Font font = new Font("Arial", 30.0f);
            SolidBrush brush = new SolidBrush(System.Drawing.Color.Gold);


            // draw dealer hand
            for (int i = 0; i < _dealerCards.Count; i++)
            {
                canvas.DrawString("Dealer", font, brush, new PointF(0, 0));

                if (i == 0 && _gameState == GameState.Playing)
                {
                    using Image image = Image.FromFile(FACE_DOWN_CARD);
                    canvas.DrawImage(image, new Point(IMAGE_WIDTH * i, 50));
                }
                else
                {
                    using Image image = Image.FromFile(_dealerCards[i].LocalImageUrl);
                    canvas.DrawImage(image, new Point(IMAGE_WIDTH * i, 50));
                }
            }

            // draw players hands
            for (int x = 0; x < _players.Count; x++)
            {
                for (int y = 0; y < _players[x].Cards.Count; y++)
                {
                    canvas.DrawString($"{_players[x].User.Username}", font, brush, new PointF(0, IMAGE_HEIGHT * (x + 1) + 50));
                    using Image image = Image.FromFile(_players[x].Cards[y].LocalImageUrl);
                    canvas.DrawImage(image, new Point(IMAGE_WIDTH * y, (IMAGE_HEIGHT * (x + 1)) + 100));
                }
            }

            canvas.Save();
            MemoryStream memoryStream = new MemoryStream();
            compositeBitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;

            return memoryStream;
        }

        public async Task HandleReaction(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.User.Value.IsBot)
                return;
            else if (cacheable.Id != _sessionMessage.Id)
                return;
            else if (_gameState == GameState.SessionStarted)
            {
                if (reaction.Emote.Name == START_EMOJI.Name)
                    StartGame();
                else if (reaction.Emote.Name == END_EMOJI.Name)
                    EndSession();
                else if (reaction.Emote.Name == JOIN_TABLE_EMOJI.Name)
                    JoinTable(reaction.User.Value);
                else if (reaction.Emote.Name == LEAVE_TABLE_EMOJI.Name)
                    LeaveTable(reaction.User.Value.Id);
            }
            else if (_gameState == GameState.Playing &&
                     _players.Any(x => x.User.Id == reaction.User.Value.Id))
            {
                var player = _players.First(x => x.User.Id == reaction.User.Value.Id);

                if (reaction.Emote.Name == HIT_EMOJI.Name)
                    Hit(player);
                else if (reaction.Emote.Name == STAND_EMOJI.Name)
                    Stand(player);
            }
            else if (_gameState == GameState.End &&
                     _players.Any(x => x.User.Id == reaction.User.Value.Id))
            {
                if (reaction.Emote.Name == RETURN_EMOJI.Name)
                    StartSession();
                else if (reaction.Emote.Name == START_EMOJI.Name)
                    StartGame();
                else if (reaction.Emote.Name == END_EMOJI.Name)
                    EndSession();
            }
        }
    }
}