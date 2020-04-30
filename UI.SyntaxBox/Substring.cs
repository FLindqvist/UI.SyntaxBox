using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.SyntaxBox
{
    /// <summary>
    /// A POCO object describing a matched substring.
    /// </summary>
    public class Substring
    {
        /// <summary>
        /// The first character in the match
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// The length of the matched string
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// The matched substring value.
        /// </summary>
        public string Value { get; set; }
    }
}
