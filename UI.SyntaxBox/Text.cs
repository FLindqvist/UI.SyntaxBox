using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UI.SyntaxBox
{
    /// <summary>
    /// Extension methods dealing with text.
    /// </summary>
    public static class Text
    {
        static readonly AhoCorasickSearch _search = new AhoCorasickSearch(
                new List<string> { "\r\n", "\n" },
                false);

        // ...................................................................
        /// <summary>
        /// Gets a TextLine representing the line at a specific char position 
        /// in Text.
        /// </summary>
        /// <param name="Text">The text to inspect.</param>
        /// <param name="Position"></param>
        /// <returns>A TextLine or null.</returns>
        public static TextLine GetLineAtPosition(this string Text, int Position)
        {
            if (Text == null || Position < 0 || Position > Text.Length)
                return (null);
            string nlstr = Environment.NewLine.Last().ToString();
            int nlen = nlstr.Length;
            char nl = nlstr[0];

            int start = -1, end = -1;

            //Search left
            for (int i = Position - 1; i > nlen; i--)
            {
                if (Text[i - nlen] == nl && (nlen == 1 || Text.Substring(i - nlen, nlen) == nlstr))
                {
                    start = i;
                    break;
                }
            }
            if (start < 0)
                start = 0;

            // Search right
            for (int i = Position; i < Text.Length; i++)
            {
                if (Text[i] == nl && (nlen == 1 || Text.Length > i + 1 && Text[i + 1] == nlstr[1]))
                {
                    end = i + nlen;
                    break;
                }
            }
            if (end < 0)
                end = Text.Length;

            TextLine line = new TextLine
            {
                Text = Text.Substring(start, end - start),
                StartIndex = start,
                LineNumber = -1
            };

            return (line);
        }
        // ...................................................................
        /// <summary>
        /// Parses the text returning a chunk of lines as TextLines starting at
        /// First line and ending on Last line or at the end of the text.
        /// This can be done using built-in TextBox functions, but this is >5x faster.
        /// </summary>
        /// <param name="Text">The text to parse.</param>
        /// <param name="First">The first line to include in the chunk.</param>
        /// <param name="Last">The last line to include in the chunk.</param>
        /// <returns></returns>
        public static List<TextLine> GetLines(this string Text, int First, int Last, out int TotalLines)
        {
            string nlstr = Environment.NewLine;
            int nlen = nlstr.Length;
            char nl = nlstr[nlen-1];

            List<TextLine> lines = new List<TextLine>(Math.Min(1000, Last - First));
            int start = 0;
            int foundNewlines = 0;
            for (int i = 0; i < Text.Length; i++)
            {
                if (Text[i] == nl)
                {
                    if (foundNewlines >= First && foundNewlines <= Last)
                    {
                        TextLine line = new TextLine
                        {
                            Text = Text.Substring(start, i +1 - start),
                            StartIndex = start,
                            LineNumber = foundNewlines
                        };
                        lines.Add(line);
                    }

                    foundNewlines++;
                    start = i + 1;
                }
            }
            if (start <= Text.Length && foundNewlines >= First && foundNewlines <= Last)
            {
                TextLine tailLine = new TextLine
                {
                    Text = Text.Substring(start),
                    StartIndex = start,
                    LineNumber = foundNewlines
                };
                lines.Add(tailLine);
            }
            TotalLines = foundNewlines + 1;
            return (lines);
        }
        // ...................................................................
        /// <summary>
        /// Parses the text returning a chunk of lines as TextLines starting at
        /// First line and ending on Last line or at the end of the text.
        /// This can be done using built-in TextBox functions, but this is >5x faster.
        /// </summary>
        /// <param name="Text">The text to parse.</param>
        /// <param name="First">The first line to include in the chunk.</param>
        /// <param name="Last">The last line to include in the chunk.</param>
        /// <returns></returns>
        public static List<TextLine> GetLines2(this string Text, int First, int Last, out int TotalLines)
        {
            string nlstr = Environment.NewLine;
            int nlen = nlstr.Length;
            char nl = nlstr[0];

            List<TextLine> lines = new List<TextLine>(Math.Min(1000, Last - First));
            int start = 0;
            int foundNewlines = 0;

            var matches = _search.FindAll(Text)
                .OrderBy((x) => x.Position)
                .ToList();

            foreach (var nwln in matches)
            {
                if (foundNewlines >= First && foundNewlines <= Last)
                {
                    TextLine line = new TextLine
                    {
                        Text = Text.Substring(start, nwln.Position + nwln.Length - start),
                        StartIndex = start,
                        LineNumber = foundNewlines
                    };
                    lines.Add(line);
                }

                foundNewlines++;
                start = nwln.Position + nwln.Length;
            }

            if (start <= Text.Length && foundNewlines >= First && foundNewlines <= Last)
            {
                TextLine tailLine = new TextLine
                {
                    Text = Text.Substring(start),
                    StartIndex = start,
                    LineNumber = foundNewlines
                };
                lines.Add(tailLine);
            }
            TotalLines = foundNewlines + 1;
            return (lines);
        }
        // ...................................................................
        /// <summary>
        /// Determines if the Position in Text is is on a start-of-word-boundary,
        /// i.e. that the position is at the beginning of the string or that 
        /// the prefix character is a non-word character. It assumes the position is
        /// already on a word character.
        /// </summary>
        /// <param name="Text">The text to inspect</param>
        /// <param name="Position">The a position in Text pointing at a word character.</param>
        /// <returns></returns>
        public static bool IsStartWordBoundary(this string Text, int Position)
        {
            if (Text is null)
            {
                throw new ArgumentNullException(nameof(Text));
            }

            return (
                // Beginning of input is always a word boundary.
                Position == 0 ||

                // Alphanumeric characters and underscore are word digits
                !Char.IsLetterOrDigit(Text, Position - 1) && Text[Position - 1] != '_');
        }
        // ...................................................................
        /// <summary>
        /// Determines if the Position in Text is is on a start-of-word-boundary,
        /// i.e. that the position is at the end of the string or that 
        /// the postfix character is a non-word character. It assumes the position is
        /// already on a word character.
        /// </summary>
        /// <param name="Text">The text to inspect</param>
        /// <param name="Position">The a position in Text pointing at a word character.</param>
        /// <returns></returns>
        public static bool IsEndWordBoundary(this string Text, int Position)
        {
            if (Text is null)
            {
                throw new ArgumentNullException(nameof(Text));
            }

            return (
                // End of input is always a word boundary.
                Position >= Text.Length - 1 ||

                // Alphanumeric characters and underscore are word digits
                !Char.IsLetterOrDigit(Text, Position + 1) && Text[Position + 1] != '_');
        }
        // ...................................................................
        public static void AddRange(this HashSet<int> Target, IEnumerable<int> Items)
        {
            foreach (var item in Items)
                Target.Add(item);
        }
        // ...................................................................
    }
}
