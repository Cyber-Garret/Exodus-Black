using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.Addons.Interactive
{
    /// <summary>
    /// The paginated message callback.
    /// </summary>
    public class PaginatedMessageCallback : IReactionCallback
    {
        /// <summary>
        /// The run mode.
        /// </summary>
        public RunMode RunMode => RunMode.Default;

        /// <summary>
        /// The timeout.
        /// </summary>
        public TimeSpan? Timeout => Options.Timeout;

        /// <summary>
        /// The options.
        /// </summary>
        private PaginatedAppearanceOptions Options => pager.Options;

        /// <summary>
        /// The page count.
        /// </summary>
        private readonly int pages;

        /// <summary>
        /// The current page.
        /// </summary>
        private int page = 1;

        /// <summary>
        /// The paginated message
        /// </summary>
        private readonly PaginatedMessage pager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedMessageCallback"/> class.
        /// </summary>
        /// <param name="interactive">
        /// The interactive.
        /// </param>
        /// <param name="sourceContext">
        /// The source context.
        /// </param>
        /// <param name="pager">
        /// The pager.
        /// </param>
        /// <param name="criterion">
        /// The criterion.
        /// </param>
        public PaginatedMessageCallback(InteractiveService interactive,
            SocketCommandContext sourceContext,
            PaginatedMessage pager,
            ICriterion<SocketReaction> criterion = null)
        {
            Interactive = interactive;
            Context = sourceContext;
            Criterion = criterion ?? new EmptyCriterion<SocketReaction>();
            this.pager = pager;
            pages = this.pager.Pages.Count();
        }

        /// <summary>
        /// Gets the command context.
        /// </summary>
        public SocketCommandContext Context { get; }

        /// <summary>
        /// Gets the interactive service.
        /// </summary>
        public InteractiveService Interactive { get; }

        /// <summary>
        /// Gets the criterion.
        /// </summary>
        public ICriterion<SocketReaction> Criterion { get; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public IUserMessage Message { get; private set; }

        /// <summary>
        /// The display async.
        /// </summary>
        /// <param name="reactionList">
        /// The reactions.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task DisplayAsync(ReactionList reactionList)
        {
            var embed = BuildEmbed();
            var message = await Context.Channel.SendMessageAsync(pager.Content, embed: embed).ConfigureAwait(false);
            Message = message;
            Interactive.AddReactionCallback(message, this);

            // reactionList take a while to add, don't wait for them
            _ = Task.Run(async () =>
            {
                if (reactionList.First) await message.AddReactionAsync(Options.First);
                if (reactionList.Backward) await message.AddReactionAsync(Options.Back);
                if (reactionList.Forward) await message.AddReactionAsync(Options.Next);
                if (reactionList.Last) await message.AddReactionAsync(Options.Last);


                var manageMessages = Context.Channel is IGuildChannel guildChannel &&
                                     (Context.User as IGuildUser).GetPermissions(guildChannel).ManageMessages;

                if (reactionList.Jump)
                {
                    if (Options.JumpDisplayOptions == JumpDisplayOptions.Always || (Options.JumpDisplayOptions == JumpDisplayOptions.WithManageMessages && manageMessages))
                    {
                        await message.AddReactionAsync(Options.Jump);
                    }
                }

                if (reactionList.Trash)
                {
                    await message.AddReactionAsync(Options.Stop);
                }

                if (reactionList.Info)
                {
                    if (Options.DisplayInformationIcon) await message.AddReactionAsync(Options.Info);
                }
            });
            if (Timeout.HasValue)
            {
                DisplayTimeout(message, Message);
            }
        }

        /// <summary>
        /// Ensures that is removed all reactions on timeout
        /// </summary>
        public void DisplayTimeout(RestUserMessage restUserMessage, IUserMessage userMessage)
        {
            if (Timeout.HasValue)
            {
                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {
                    Interactive.RemoveReactionCallback(restUserMessage);
                    userMessage.RemoveAllReactionsAsync();
                });
            }
        }

        /// <summary>
        /// Handles a reaction callback
        /// </summary>
        /// <param name="reaction">
        /// The reaction.
        /// </param>
        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(Options.First))
                page = 1;
            else if (emote.Equals(Options.Next))
            {
                if (page >= pages)
                    return false;
                ++page;
            }
            else if (emote.Equals(Options.Back))
            {
                if (page <= 1)
                    return false;
                --page;
            }
            else if (emote.Equals(Options.Last))
                page = pages;
            else if (emote.Equals(Options.Stop))
            {
                await Message.RemoveAllReactionsAsync().ConfigureAwait(false);
                return true;
            }
            else if (emote.Equals(Options.Jump))
            {
                _ = Task.Run(async () =>
                {
                    var criteria = new Criteria<SocketMessage>()
                        .AddCriterion(new EnsureSourceChannelCriterion())
                        .AddCriterion(new EnsureFromUserCriterion(reaction.UserId))
                        .AddCriterion(new EnsureIsIntegerCriterion());
                    var response = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(15));
                    var request = int.Parse(response.Content);
                    if (request < 1 || request > pages)
                    {
                        _ = response.DeleteAsync().ConfigureAwait(false);
                        await Interactive.ReplyAndDeleteAsync(Context, Options.Stop.Name);
                        return;
                    }

                    page = request;
                    _ = response.DeleteAsync().ConfigureAwait(false);
                    await RenderAsync().ConfigureAwait(false);
                });
            }
            else if (emote.Equals(Options.Info))
            {
                await Interactive.ReplyAndDeleteAsync(Context, Options.InformationText, timeout: Options.InfoTimeout);
                return false;
            }

            _ = Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            await RenderAsync().ConfigureAwait(false);
            return false;
        }

        /// <summary>
        /// The build embed.
        /// </summary>
        protected Embed BuildEmbed()
        {
            var current = pager.Pages.ElementAt(page - 1);

            var builder = new EmbedBuilder
            {
                Author = current.Author ?? pager.Author,
                Title = current.Title ?? pager.Title,
                Url = current.Url ?? pager.Url,
                Description = current.Description ?? pager.Description,
                ImageUrl = current.ImageUrl ?? pager.ImageUrl,
                Color = current.Color ?? pager.Color,
                Fields = current.Fields ?? pager.Fields,
                Footer = new EmbedFooterBuilder
                {
                    Text = string.Format(current.FooterTextOverride ?? pager.FooterTextOverride ?? Options.FooterFormat, page, pages)
                },
                ThumbnailUrl = current.ThumbnailUrl ?? pager.ThumbnailUrl,
                Timestamp = current.TimeStamp ?? pager.TimeStamp
            };

            /*var builder = new EmbedBuilder()
                .WithAuthor(pager.Author)
                .WithColor(pager.Color)
                .WithDescription(pager.Pages.ElementAt(page - 1).Description)
                .WithImageUrl(current.ImageUrl ?? pager.DefaultImageUrl)
                .WithUrl(current.Url)
                .WithFooter(f => f.Text = string.Format(options.FooterFormat, page, pages))
                .WithTitle(current.Title ?? pager.Title);*/
            builder.Fields = pager.Pages.ElementAt(page - 1).Fields;

            return builder.Build();
        }

        /// <summary>
        /// Renders an embed page
        /// </summary>
        private Task RenderAsync()
        {
            var embed = BuildEmbed();
            return Message.ModifyAsync(m => m.Embed = embed);
        }
    }
}
