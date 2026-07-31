using System.Collections.Generic;

namespace src
{
    /// <summary>
    /// A single &lt;expression&gt; replacement span located in raw object text.
    /// Offsets index into the scanned string. <see cref="ExpressionStart"/> is the
    /// first character after the opening '&lt;', and <see cref="CloseIndex"/> is the
    /// closing '&gt;'.
    /// </summary>
    public sealed record ReplacementSpan(int OpenIndex, int ExpressionStart, int ExpressionLength)
    {
        public int CloseIndex => ExpressionStart + ExpressionLength;

        public string ExpressionText(string text) => text.Substring(ExpressionStart, ExpressionLength);
    }

    /// <summary>
    /// Locates &lt;expression&gt; replacement spans in object text. This is the single
    /// source of truth for the replacement rules: '&lt;&lt;' and '&gt;&gt;' are escaped literal
    /// characters, a lone '&lt;' opens a replacement, the first non-doubled '&gt;' closes it,
    /// and nesting is not supported. Both runtime evaluation (ObjectVariableReplacer)
    /// and the language server use this scanner so their notions of where a replacement
    /// begins and ends cannot drift apart.
    /// </summary>
    public static class ReplacementScanner
    {
        /// <summary>
        /// Returns the spans found, plus the index of the first nested '&lt;' when one is
        /// encountered inside an open replacement (scanning stops there; runtime
        /// evaluation treats that as an error, the language server just ignores it).
        /// </summary>
        public static (List<ReplacementSpan> Spans, int? NestedOpenIndex) Scan(string text)
        {
            List<ReplacementSpan> spans = new();
            int? openIndex = null;

            for (int i = 0; i < text.Length; i++)
            {
                char current = text[i];
                if (current != '<' && current != '>')
                {
                    continue;
                }

                bool doubled = i + 1 < text.Length && text[i + 1] == current;
                if (doubled)
                {
                    i++;
                    continue;
                }

                if (current == '<')
                {
                    if (openIndex is not null)
                    {
                        return (spans, i);
                    }

                    openIndex = i;
                }
                else if (openIndex is int start)
                {
                    spans.Add(new ReplacementSpan(start, start + 1, i - start - 1));
                    openIndex = null;
                }
            }

            return (spans, null);
        }
    }
}
