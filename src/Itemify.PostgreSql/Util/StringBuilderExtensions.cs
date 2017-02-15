using System.Linq;
using System.Text;

namespace Itemify.Core.PostgreSql.Util
{

    public static class StringBuilderExtensions
    {

        public static StringBuilder TrimEnd(this StringBuilder builder, char character)
        {
            while (builder.Length > 0 && builder[builder.Length - 1] == character)
                builder.Length -= 1;

            return builder;
        }

        public static StringBuilder TrimEndLineBreaks(this StringBuilder builder)
        {
            return TrimEnd(builder, '\n');
        }


        public static StringBuilder TrimEnd(this StringBuilder builder, params char[] character)
        {
            while (builder.Length > 0 && character.Contains(builder[builder.Length - 1]))
                builder.Length -= 1;

            return builder;
        }

        public static StringBuilder NewLine(this StringBuilder builder)
        {
            return builder.Append('\n');
        }

        public static StringBuilder WriteLine(this StringBuilder builder, string source)
        {
            return builder.Append(source).NewLine();
        }

        public static StringBuilder Write(this StringBuilder builder, string source)
        {
            return builder.Append(source);
        }

        public static StringBuilder WriteTabbed(this StringBuilder builder, int tabs, string source)
        {
            return builder.Append(new string(' ', tabs * 4)).Append(source);
        }

        public static StringBuilder WriteTabbedLine(this StringBuilder builder, int tabs, string source)
        {
            return builder.WriteTabbed(tabs, source).NewLine();
        }

        public static StringBuilder WriteIf(this StringBuilder builder, bool assertion, string source)
        {
            if (assertion)
                builder.Write(source);

            return builder;
        }
    }
}
