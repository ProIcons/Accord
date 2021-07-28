using System.Collections.Generic;
using System.Drawing;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Accord.Bot.Models
{
    public record EmbedMessage(
        IEmbed Embed,
        Color Color = default,
        FileData? FileData = default,
        MessageReference? MessageReference = default,
        List<IMessageComponent>? MessageComponents = default,
        AllowedMentions? AllowedMentions = default
    ) : BaseMessage<EmbedMessage>(FileData, MessageReference, MessageComponents, AllowedMentions)
    {
        public static explicit operator EmbedMessage(Embed message) => new(message);
    }
}