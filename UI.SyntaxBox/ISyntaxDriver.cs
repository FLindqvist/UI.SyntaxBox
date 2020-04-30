using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace UI.SyntaxBox
{
    public interface ISyntaxDriver
    {
        /// <summary>
        /// Gets or sets the abilities of the driver.
        /// </summary>
        /// <value>
        /// The type of the driver.
        /// </value>
        DriverOperation Abilities { get; }

        /// <summary>
        /// Applies the driver logic to the supplied text. 
        /// This will be called repeatedly 
        /// </summary>
        /// <param name="Operation">The operation.</param>
        /// <param name="Text">The text.</param>
        /// <returns></returns>
        IEnumerable<FormatInstruction> Match(DriverOperation Operation, string Text);
    }
    public class FormatInstruction
    {
        public int RuleId;
        public int FromChar;
        public int Length;
        public Brush Background;
        public Brush Foreground;
        public Pen Outline;
    }
    [Flags]
    public enum DriverOperation : byte
    {
        None = 0,

        /// <summary>
        /// Matches single lines.
        /// </summary>
        Line = 1,

        /// <summary>
        /// Matches a block of text/multiline.
        /// </summary>
        Block = 2,

        /// <summary>
        /// Matches on the entire text.
        /// </summary>
        FullText = 4
    }
}
