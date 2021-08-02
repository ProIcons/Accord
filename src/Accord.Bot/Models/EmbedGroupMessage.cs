using System.Collections.Generic;
using System.Linq;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Accord.Bot.Models
{
    public record EmbedGroupMessage(List<IEmbed> Embeds,
        List<IMessageComponent>? MessageComponents = default,
        AllowedMentions? AllowedMentions = default
    ) : BaseMessage<EmbedMessage>(MessageComponents, AllowedMentions)
    {
        public static EmbedGroupMessage ToGroupMessage(params EmbedMessage[] embeds) =>
            ToGroupMessage(embeds.ToList());

        public static EmbedGroupMessage ToGroupMessage(List<EmbedMessage> embeds) => new(
            embeds.Select(x => x.Embed).ToList(),
            embeds.FirstOrDefault()?.MessageComponents,
            embeds.FirstOrDefault()?.AllowedMentions
        );
    }
}