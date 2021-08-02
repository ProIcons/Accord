using System.Collections.Generic;
using System.Drawing;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Accord.Bot.Models
{
    public record InfoMessage(
        string Content,
        List<IMessageComponent>? MessageComponents = default,
        AllowedMentions? AllowedMentions = default
    ) : StateMessage(Content, Color.Cyan, MessageComponents, AllowedMentions)
    {
        public static explicit operator InfoMessage(string message) => new(message);
    }
}