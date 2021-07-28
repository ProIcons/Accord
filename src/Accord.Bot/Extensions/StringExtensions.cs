using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Accord.Bot.Extensions
{
    public static class StringExtensions
    {
        private static readonly string[] SpecialCharacters = { "\\", "`", "|" };
        private static readonly Regex UrlMatch = new(@"(?<Url>(?<Protocol>\w+)\:\/\/(?<Domain>[\w@][\w.:@]+)\/*(?<Path>[\w\.?=%&=\-@\/$,]*)?)");
        public static string DiscordSanitize(this string text)
        {
            foreach (var character in SpecialCharacters)
            {
                text = text.Replace(character, $"\\{character}");
            }

            Match match;
            if ((match = UrlMatch.Match(text)).Success)
            {
                text = text.Replace(match.Groups["Url"].Value, $"<{match.Groups["Url"].Value}>");
            }
            return text;
        }

        public static string UnquoteAgentReportText(this string text)
        {
            var lines = text.Split(Environment.NewLine);
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.AppendLine(line[0] == '>' ? line[1..].Trim() : line.Trim());
            }

            return sb.ToString();
        }
    }
}