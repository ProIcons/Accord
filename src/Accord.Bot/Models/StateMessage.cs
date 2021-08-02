using System.Collections.Generic;
using System.Drawing;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Accord.Bot.Models
{
    public record StateMessage(
        string Content,
        Color Color,
        List<IMessageComponent>? MessageComponents = default,
        AllowedMentions? AllowedMentions = default
    ) : BaseMessage<StateMessage>(MessageComponents, AllowedMentions)
    {
        public static explicit operator EmbedMessage(StateMessage message) => new(
            new Embed
            {
                Description = message.Content,
                Colour = message.Color
            },
            message.MessageComponents,
            message.AllowedMentions
        );
    }
}