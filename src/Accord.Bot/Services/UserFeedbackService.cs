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
        private readonly IDiscordRestChannelAPI _channelAPI;
        private readonly IDiscordRestUserAPI _userAPI;
        private readonly IDiscordRestWebhookAPI _webhookAPI;

        public bool HasEditedOriginalInteraction { get; private set; }

        public UserFeedbackService(ContextInjectionService contextInjection, IDiscordRestChannelAPI channelApi, IDiscordRestUserAPI userApi, IDiscordRestWebhookAPI webhookApi)
        {
            _contextInjection = contextInjection;
            _channelAPI = channelApi;
            _userAPI = userApi;
            _webhookAPI = webhookApi;
        }

        public Task<Result<IReadOnlyList<IMessage>>> RespondAsync(IUserMessage message, CancellationToken ctx = default) => message switch
        {
            EmbedMessage embedMessage => RespondEmbedAsync(embedMessage, ctx),
            TextMessage textMessage => RespondTextAsync(textMessage, ctx),
            _ => throw new ArgumentOutOfRangeException(nameof(message), $"Message should be either of type '{nameof(EmbedMessage)}' or '{nameof(TextMessage)}'")
        };

        public async Task<Result<IReadOnlyList<IMessage>>> RespondEmbedAsync(EmbedMessage message, CancellationToken ctx = default)
        {
            IEmbed a;
            a.
        }

        public async Task<Result<IReadOnlyList<IMessage>>> RespondTextAsync(TextMessage message, CancellationToken ctx = default)
        {
            var sendResults = new List<IMessage>();

            foreach (var chunk in CreateTextContentChunks(message))
            {
                var send = await SendContextualTextAsync(chunk, ctx);
                if (!send.IsSuccess)
                {
                    return Result<IReadOnlyList<IMessage>>.FromError(send);
                }

                sendResults.Add(send.Entity);
            }

            return sendResults;
        }


        public async Task<Result<IMessage>> SendContextualEmbedAsync(List<EmbedMessage> embedMessage, CancellationToken ctx = default)
        {
            if (_contextInjection.Context is null)
            {
                return new NotSupportedError("Contextual sends require a context to be available.");
            }

            switch (_contextInjection.Context)
            {
                case MessageContext messageContext:
                    return await _channelAPI.CreateMessageAsync
                    (
                        messageContext.ChannelID,
                        embeds: embedMessage.Select(x => x.Embed).ToArray(),
                        allowedMentions: embedMessage.AllowedMentions ?? new AllowedMentions(new List<MentionType>(), new List<Snowflake>(), new List<Snowflake>()),
                        file: embedMessage.FileData ?? new Optional<FileData>(),
                        messageReference: embedMessage.MessageReference ?? new Optional<IMessageReference>(),
                        components: embedMessage.MessageComponents ?? new Optional<IReadOnlyList<IMessageComponent>>(),
                        ct: ctx
                    );
                case InteractionContext interactionContext:
                    {
                        if (this.HasEditedOriginalInteraction)
                        {
                            if (embedMessage.MessageReference != null)
                            {
                                return await _channelAPI.CreateMessageAsync
                                (
                                    interactionContext.ChannelID,
                                    embedMessage.Content,
                                    allowedMentions: embedMessage.AllowedMentions ?? new AllowedMentions(new List<MentionType>(), new List<Snowflake>(), new List<Snowflake>()),
                                    file: embedMessage.FileData ?? new Optional<FileData>(),
                                    messageReference: embedMessage.MessageReference ?? new Optional<IMessageReference>(),
                                    components: embedMessage.MessageComponents ?? new Optional<IReadOnlyList<IMessageComponent>>(),
                                    ct: ctx
                                );
                            }
                            return await _webhookAPI.CreateFollowupMessageAsync
                            (
                                interactionContext.ApplicationID,
                                interactionContext.Token,
                                embedMessage.Content,
                                allowedMentions: embedMessage.AllowedMentions ?? new AllowedMentions(new List<MentionType>(), new List<Snowflake>(), new List<Snowflake>()),
                                file: embedMessage.FileData ?? new Optional<FileData>(),
                                components: embedMessage.MessageComponents ?? new Optional<IReadOnlyList<IMessageComponent>>(),
                                ct: ctx
                            );
                        }

                        if (embedMessage.MessageReference != null || embedMessage.FileData != null)
                        {
                            throw new InvalidOperationException(
                                $"{nameof(embedMessage.MessageReference)} and {nameof(embedMessage.FileData)} cannot be defined on the original interaction response");
                        }
                        var edit = await _webhookAPI.EditOriginalInteractionResponseAsync
                        (
                            interactionContext.ApplicationID,
                            interactionContext.Token,
                            embedMessage.Content,
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
                    return await _channelAPI.CreateMessageAsync
                    (
                        messageContext.ChannelID,
                        textMessage.Content,
                        allowedMentions: textMessage.AllowedMentions ?? new AllowedMentions(new List<MentionType>(), new List<Snowflake>(), new List<Snowflake>()),
                        file: textMessage.FileData ?? new Optional<FileData>(),
                        messageReference: textMessage.MessageReference ?? new Optional<IMessageReference>(),
                        components: textMessage.MessageComponents ?? new Optional<IReadOnlyList<IMessageComponent>>(),
                        ct: ctx
                    );
                case InteractionContext interactionContext:
                    {
                        if (this.HasEditedOriginalInteraction)
                        {
                            if (textMessage.MessageReference != null)
                            {
                                return await _channelAPI.CreateMessageAsync
                                (
                                    interactionContext.ChannelID,
                                    textMessage.Content,
                                    allowedMentions: textMessage.AllowedMentions ?? new AllowedMentions(new List<MentionType>(), new List<Snowflake>(), new List<Snowflake>()),
                                    file: textMessage.FileData ?? new Optional<FileData>(),
                                    messageReference: textMessage.MessageReference ?? new Optional<IMessageReference>(),
                                    components: textMessage.MessageComponents ?? new Optional<IReadOnlyList<IMessageComponent>>(),
                                    ct: ctx
                                );
                            }
                            return await _webhookAPI.CreateFollowupMessageAsync
                            (
                                interactionContext.ApplicationID,
                                interactionContext.Token,
                                textMessage.Content,
                                allowedMentions: textMessage.AllowedMentions ?? new AllowedMentions(new List<MentionType>(), new List<Snowflake>(), new List<Snowflake>()),
                                file: textMessage.FileData ?? new Optional<FileData>(),
                                components: textMessage.MessageComponents ?? new Optional<IReadOnlyList<IMessageComponent>>(),
                                ct: ctx
                            );
                        }

                        if (textMessage.MessageReference != null || textMessage.FileData != null)
                        {
                            throw new InvalidOperationException(
                                $"{nameof(textMessage.MessageReference)} and {nameof(textMessage.FileData)} cannot be defined on the original interaction response");
                        }
                        var edit = await _webhookAPI.EditOriginalInteractionResponseAsync
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

        private IEnumerable<TextMessage> CreateTextContentChunks(TextMessage originalMessage)
        {
            if (originalMessage.Content.Length <= 2000)
            {
                yield return originalMessage;
                yield break;
            }

            var words = originalMessage.Content.Split(' ');
            var messageBuilder = new StringBuilder();
            var index = 1;
            foreach (var word in words)
            {
                if (messageBuilder.Length + word.Length >= 2000)
                {
                    yield return index == words.Length
                        ? originalMessage with { Content = messageBuilder.ToString().Trim() }
                        : (TextMessage)messageBuilder.ToString().Trim();

                    messageBuilder.Clear();
                }

                messageBuilder.Append(word);
                messageBuilder.Append(' ');
                index++;
            }

            if (messageBuilder.Length > 0)
                yield return originalMessage with { Content = messageBuilder.ToString().Trim() };
        }
    }
}