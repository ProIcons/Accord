using System.Collections.Generic;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Accord.Bot.Models
{
    public record EmbedMessage(
        IEmbed Embed,
        List<IMessageComponent>? MessageComponents = default,
        AllowedMentions? AllowedMentions = default
    ) : BaseMessage<EmbedMessage>(MessageComponents, AllowedMentions)
    {
        public static explicit operator EmbedMessage(Embed message) => new(message);
    }
}