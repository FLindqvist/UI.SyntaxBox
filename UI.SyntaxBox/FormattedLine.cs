using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.SyntaxBox
{
    /// <summary>
    /// A formatted line of text.
    /// </summary>
    public class FormattedLine
    {
        /// <summary>
        /// Text of the line. No strictly necessary to maintain,
        /// but good for debugging.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// List of formats to apply to the line
        /// </summary>
        public List<FormatInstruction> LineFormatInstructions { get; set; }

        public List<FormatInstruction> BlockFormatInstructions { get; set; }
    }
}
