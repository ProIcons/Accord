using System.Collections.Generic;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Accord.Bot.Models
{
    public record TextMessage(
        string Content,
        FileData? FileData = default,
        MessageReference? MessageReference = default,
        List<IMessageComponent>? MessageComponents = default,
        AllowedMentions? AllowedMentions = default
    ) : BaseMessage<TextMessage>(FileData, MessageReference, MessageComponents, AllowedMentions)
    {
        public static explicit operator TextMessage(string message) => new(message);
    }
}