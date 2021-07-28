using System.Collections.Generic;
using System.Drawing;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Accord.Bot.Models
{
    public record ErrorEmbedMessage(
        IEmbed Embed,
        FileData? FileData = default,
        MessageReference? MessageReference = default,
        List<IMessageComponent>? MessageComponents = default,
        AllowedMentions? AllowedMentions = default
    ) : EmbedMessage(Embed, Color.DarkRed, FileData, MessageReference, MessageComponents, AllowedMentions)
    {
        public static explicit operator ErrorEmbedMessage(Embed message) => new(message);
    }
}