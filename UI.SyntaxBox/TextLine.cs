using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UI.SyntaxBox
{
    public class TextLine
    {
        public int LineNumber;
        /// <summary>
        /// The start index of the line relative to the entire text.
        /// </summary>
        public int StartIndex;
        public string Text;
        public int EndIndex => StartIndex + Text?.Length ?? 0;

    }
}
