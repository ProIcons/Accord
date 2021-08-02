using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Accord.Bot.Models;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Accord.Bot.Services
{
    public class UserFeedbackService
    {
        private readonly ContextInjectionService _contextInjection;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestWebhookAPI _webhookApi;

        public bool HasEditedOriginalInteraction { get; private set; }

        public UserFeedbackService(ContextInjectionService contextInjection, IDiscordRestChannelAPI channelApi, IDiscordRestUserAPI userApi, IDiscordRestWebhookAPI webhookApi)
        {
            _contextInjection = contextInjection;
            _channelApi = channelApi;
            _webhookApi = webhookApi;
        }


        public Task<Result<IReadOnlyList<IMessage>>> RespondAsync(IUserMessage message, CancellationToken ctx = default) => message switch
        {
            EmbedMessage embedMessage => RespondEmbedAsync(embedMessage, ctx),
            TextMessage textMessage => RespondTextAsync(textMessage, ctx),
            StateMessage stateMessage => RespondStateAsync(stateMessage, ctx),
            _ => throw new ArgumentOutOfRangeException(nameof(message), $"Message should be either of type '{nameof(EmbedMessage)}' or '{nameof(TextMessage)}'")
        };

        public async Task<Result<IReadOnlyList<IMessage>>> RespondEmbedAsync(EmbedMessage message, CancellationToken ctx = default) =>
            await RespondEmbedAsync(EmbedGroupMessage.ToGroupMessage(message), ctx);

        public async Task<Result<IReadOnlyList<IMessage>>> RespondEmbedAsync(EmbedGroupMessage message, CancellationToken ctx = default)
        {
            var result = await SendContextualEmbedAsync(message, ctx);

            return !result.IsSuccess
                ? Result<IReadOnlyList<IMessage>>.FromError(result)
                : Result<IReadOnlyList<IMessage>>.FromSuccess(new List<IMessage> { result.Entity });
        }

        public async Task<Result<IReadOnlyList<IMessage>>> RespondTextAsync(TextMessage message, CancellationToken ctx = default)
        {
            var sendResults = new List<IMessage>();

            foreach (var chunk in CreateTextContentChunks(message.Content))
            {
                var send = await SendContextualTextAsync(chunk.IsLast
                    ? message with { Content = chunk.Content }
                    : new TextMessage(chunk.Content), ctx);
                if (!send.IsSuccess)
                {
                    return Result<IReadOnlyList<IMessage>>.FromError(send);
                }

                sendResults.Add(send.Entity);
            }

            return sendResults;
        }

        public async Task<Result<IReadOnlyList<IMessage>>> RespondStateAsync(StateMessage message, CancellationToken ctx = default)
        {
            var embedGroupMessage = new EmbedGroupMessage(
                CreateTextContentChunks(message.Content).Select(x => (IEmbed)new Embed { Description = x.Content, Colour = message.Color }).ToList(),
                message.MessageComponents,
                message.AllowedMentions
            );

            var result = await SendContextualEmbedAsync(embedGroupMessage, ctx);

            return !result.IsSuccess
                ? Result<IReadOnlyList<IMessage>>.FromError(result)
                : Result<IReadOnlyList<IMessage>>.FromSuccess(new List<IMessage> { result.Entity });
        }

        public Task<Result<IMessage>> SendEmbedAsync(
            Snowflake channel,
            Embed embed,
            MessageReference? messageReference = default,
            AllowedMentions? allowedMentions = default,
            List<IMessageComponent>? messageComponents = default,
            FileData? fileData = default,
            CancellationToken ct = default
        ) => SendEmbedsAsync(channel, new List<Embed> { embed }, messageReference, allowedMentions, messageComponents, fileData, ct);

        public Task<Result<IMessage>> SendEmbedsAsync(
            Snowflake channel,
            List<Embed> embeds,
            MessageReference? messageReference = default,
            AllowedMentions? allowedMentions = default,
            List<IMessageComponent>? messageComponents = default,
            FileData? fileData = default,
            CancellationToken ct = default
        )
        {
            return _channelApi.CreateMessageAsync(
                channel,
                embeds: embeds,
                messageReference: messageReference ?? new Optional<IMessageReference>(),
                allowedMentions: allowedMentions ?? new Optional<IAllowedMentions>(),
                components: messageComponents ?? new Optional<IReadOnlyList<IMessageComponent>>(),
                file: fileData ?? new Optional<FileData>(),
                ct: ct
            );
        }

        public Task<Result<IMessage>> SendMessageAsync(
            Snowflake channel,
            string content,
            MessageReference? messageReference = default,
            AllowedMentions? allowedMentions = default,
            List<IMessageComponent>? messageComponents = default,
            FileData? fileData = default,
            CancellationToken ct = default
        )
        {
            return _channelApi.CreateMessageAsync(
                channel,
                content,
                messageReference: messageReference ?? new Optional<IMessageReference>(),
                allowedMentions: allowedMentions ?? new Optional<IAllowedMentions>(),
                components: messageComponents ?? new Optional<IReadOnlyList<IMessageComponent>>(),
                file: fileData ?? new Optional<FileData>(),
                ct: ct
            );
        }

        public async Task<Result<IMessage>> SendContextualEmbedAsync(EmbedGroupMessage embedMessage, CancellationToken ctx = default)
        {
            if (_contextInjection.Context is null)
            {
                return new NotSupportedError("Contextual sends require a context to be available.");
            }

            switch (_contextInjection.Context)
            {
                case MessageContext messageContext:
                    return await _channelApi.CreateMessageAsync
                    (
                        messageContext.ChannelID,
                        embeds: embedMessage.Embeds,
                        allowedMentions: embedMessage.AllowedMentions ?? new AllowedMentions(new List<MentionType>(), new List<Snowflake>(), new List<Snowflake>()),
                        components: embedMessage.MessageComponents ?? new Optional<IReadOnlyList<IMessageComponent>>(),
                        ct: ctx
                    );
                case InteractionContext interactionContext:
                    {
                        if (this.HasEditedOriginalInteraction)
                        {
                            return await _webhookApi.CreateFollowupMessageAsync
                            (
                                interactionContext.ApplicationID,
                                interactionContext.Token,
                                embeds: embedMessage.Embeds,
                                allowedMentions: embedMessage.AllowedMentions ?? new AllowedMentions(new List<MentionType>(), new List<Snowflake>(), new List<Snowflake>()),
                                components: embedMessage.MessageComponents ?? new Optional<IReadOnlyList<IMessageComponent>>(),
                                ct: ctx
                            );
                        }

                        var edit = await _webhookApi.EditOriginalInteractionResponseAsync
                        (
                            interactionContext.ApplicationID,
                            interactionContext.Token,
                            embeds: embedMessage.Embeds,
                            allowedMentions: embedMessage.AllowedMentions ?? new AllowedMentions(new List<MentionType>(), new List<Snowflake>(), new List<Snowflake>()),
                            components: embedMessage.MessageComponents ?? new Optional<IReadOnlyList<IMessageComponent>>(),
                            ct: ctx
                        );

                        if (edit.IsSuccess)
                        {
                            this.HasEditedOriginalInteraction = true;
                        }

                        return edit;
                    }
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
        }

        public async Task<Result<IMessage>> SendContextualTextAsync(TextMessage textMessage, CancellationToken ctx = default)
        {
            if (_contextInjection.Context is null)
            {
                return new NotSupportedError("Contextual sends require a context to be available.");
            }

            switch (_contextInjection.Context)
            {
                case MessageContext messageContext:
                    return await _channelApi.CreateMessageAsync
                    (
                        messageContext.ChannelID,
                        textMessage.Content,
                        allowedMentions: textMessage.AllowedMentions ?? new AllowedMentions(new List<MentionType>(), new List<Snowflake>(), new List<Snowflake>()),
                        ct: ctx
                    );
                case InteractionContext interactionContext:
                    {
                        if (this.HasEditedOriginalInteraction)
                        {
                            return await _webhookApi.CreateFollowupMessageAsync
                            (
                                interactionContext.ApplicationID,
                                interactionContext.Token,
                                textMessage.Content,
                                allowedMentions: textMessage.AllowedMentions ?? new AllowedMentions(new List<MentionType>(), new List<Snowflake>(), new List<Snowflake>()),
                                components: textMessage.MessageComponents ?? new Optional<IReadOnlyList<IMessageComponent>>(),
                                ct: ctx
                            );
                        }

                        var edit = await _webhookApi.EditOriginalInteractionResponseAsync
                        (
                            interactionContext.ApplicationID,
                            interactionContext.Token,
                            textMessage.Content,
                            allowedMentions: textMessage.AllowedMentions ?? new AllowedMentions(new List<MentionType>(), new List<Snowflake>(), new List<Snowflake>()),
                            components: textMessage.MessageComponents ?? new Optional<IReadOnlyList<IMessageComponent>>(),
                            ct: ctx
                        );

                        if (edit.IsSuccess)
                        {
                            this.HasEditedOriginalInteraction = true;
                        }

                        return edit;
                    }
                default:
                    {
                        throw new InvalidOperationException();
                    }
            }
        }

        private IEnumerable<(string Content, bool IsLast)> CreateTextContentChunks(string originalMessage, int chunkSize = 2000)
        {
            if (originalMessage.Length <= chunkSize)
            {
                yield return (originalMessage, true);
                yield break;
            }

            var words = originalMessage.Split(' ');
            var messageBuilder = new StringBuilder();
            var index = 1;
            foreach (var word in words)
            {
                if (messageBuilder.Length + word.Length >= chunkSize)
                {
                    yield return (messageBuilder.ToString().Trim(), index == words.Length);
                    messageBuilder.Clear();
                }

                messageBuilder.Append(word);
                messageBuilder.Append(' ');
                index++;
            }

            if (messageBuilder.Length > 0)
                yield return (messageBuilder.ToString().Trim(), true);
        }
    }
}