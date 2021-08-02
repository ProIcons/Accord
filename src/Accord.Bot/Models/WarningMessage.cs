using System.Collections.Generic;
using System.Drawing;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Accord.Bot.Models
{
    public record WarningMessage(
        string Content,
        List<IMessageComponent>? MessageComponents = default,
        AllowedMentions? AllowedMentions = default
    ) : StateMessage(Content, Color.Orange, MessageComponents, AllowedMentions)
    {
        public static explicit operator WarningMessage(string message) => new(message);
    }
}