using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace UI.SyntaxBox.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string LINE_COMMENT = "//";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool value = (bool)this.textBox.GetValue(SyntaxBox.EnabledProperty);

            this.textBox.SetValue(SyntaxBox.EnabledProperty, !value);
        }

        #region Private members
        // ...................................................................
        /// <summary>
        /// Finds the position of the first non-whitespace character in a 
        /// TextLine.
        /// </summary>
        /// <param name="Line"></param>
        /// <returns></returns>
        private static int FindLineStart(TextLine Line)
        {
            for (int i = 0; i < Line.Text.Length; i++)
            {
                if (!Char.IsWhiteSpace(Line.Text[i]))
                {
                    return (i);
                }
            }
            return (Line.Text.Length);
        }
        // ...................................................................
        #endregion

        #region Command handlers
        // ...................................................................
        /// <summary>
        /// Commnts out the currently selected block.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnCommentCommand(object sender, ExecutedRoutedEventArgs args)
        {
            int
                selStart = this.textBox.SelectionStart,
                selLength = this.textBox.SelectionLength,
                selEnd = selStart + selLength,
                firstLine = this.textBox.GetLineIndexFromCharacterIndex(selStart),
                lastLine = selLength > 0
                    ? this.textBox.GetLineIndexFromCharacterIndex(selStart + selLength - 1)
                    : firstLine;

            // Gets the currently selected lines
            var affectedLines = this.textBox.Text.GetLines(firstLine, lastLine, out int totalLines).ToList();

            // These are the offset of the selection start/end from the END
            // of the first/last affected lines. They are used to reset the
            // selection after the operation.
            int selStartOffset = affectedLines[0].EndIndex - selStart;
            int selEndOffset = affectedLines[affectedLines.Count - 1].EndIndex - selEnd;

            // Find the smallest line start among the affected lines.
            // THis is where we'll throw in the line comments.
            int insPos = affectedLines
                .Select(FindLineStart)
                .Min();

            // Increase indent for the affected block.
            var indentedBlock = String.Join("", affectedLines
                .Select((line) => line.Text.Substring(0, insPos) 
                    + LINE_COMMENT 
                    + line.Text.Substring(insPos))
                .ToArray());

            // Update the text
            StringBuilder sb = new StringBuilder();
            sb.Append(this.textBox.Text.Substring(0, affectedLines[0].StartIndex));
            sb.Append(indentedBlock);
            sb.Append(this.textBox.Text.Substring(affectedLines[affectedLines.Count - 1].StartIndex + affectedLines[affectedLines.Count - 1].Text.Length));
            this.textBox.Text = sb.ToString();

            // Reset the selection and caret
            var firstAffected = this.textBox.Text.GetLines(firstLine, firstLine, out totalLines).Single();
            var lastAffected = this.textBox.Text.GetLines(lastLine, lastLine, out totalLines).Single();
            selStart = firstAffected.EndIndex - selStartOffset;
            selEnd = lastAffected.EndIndex - selEndOffset;
            selLength = selEnd - selStart;
            this.textBox.Select(selStart, selLength);
        }
        // ...................................................................
        /// <summary>
        /// Uncomments the currently selected block, where applicable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnUncommentCommand(object sender, ExecutedRoutedEventArgs args)
        {
            int
                selStart = this.textBox.SelectionStart,
                selLength = this.textBox.SelectionLength,
                selEnd = selStart + selLength,
                firstLine = this.textBox.GetLineIndexFromCharacterIndex(selStart),
                lastLine = selLength > 0
                    ? this.textBox.GetLineIndexFromCharacterIndex(selStart + selLength - 1)
                    : firstLine;

            var affectedLines = this.textBox.Text.GetLines(firstLine, lastLine, out int totalLines).ToList();

            // These are the offset of the selection start/end from the END
            // of the first/last affected lines. They are used to reset the
            // selection after the operation.
            int selStartOffset = affectedLines[0].EndIndex - selStart;
            int selEndOffset = affectedLines[affectedLines.Count - 1].EndIndex - selEnd;

            // Remove any comment prefix for the affected block.
            var unindentedBlock = String.Join("", affectedLines
                .Select((line) =>
                {
                    int start = FindLineStart(line);
                    if (line.Text.Substring(start).StartsWith(LINE_COMMENT))
                    {
                        return (line.Text.Substring(0, start)
                            + line.Text.Substring(start + LINE_COMMENT.Length));
                    }
                    return (line.Text);
                })
                .ToArray());

            // Update the text
            StringBuilder sb = new StringBuilder();
            sb.Append(this.textBox.Text.Substring(0, affectedLines[0].StartIndex));
            sb.Append(unindentedBlock);
            sb.Append(this.textBox.Text.Substring(affectedLines[affectedLines.Count - 1].StartIndex + affectedLines[affectedLines.Count - 1].Text.Length));
            this.textBox.Text = sb.ToString();

            // Reset the selection and caret
            var firstAffected = this.textBox.Text.GetLines(firstLine, firstLine, out totalLines).Single();
            var lastAffected = this.textBox.Text.GetLines(lastLine, lastLine, out totalLines).Single();
            selStart = Math.Max(firstAffected.StartIndex, firstAffected.EndIndex - selStartOffset);
            selEnd = Math.Max(
                lastAffected.StartIndex,
                lastAffected.EndIndex - selEndOffset);
            selLength = selEnd - selStart;
            this.textBox.Select(selStart, selLength);
        }
        // ...................................................................
        #endregion
    }
}
