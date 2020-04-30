using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UI.SyntaxBox
{
    public class SyntaxRenderer : FrameworkElement
    {
        private const int LN_MARGIN = 10;
        private const char INDENT_CHAR = ' ';
        private const int BLOCK_EXTEND = 200;
        private ScrollViewer _scrollViewer;
        private Canvas _lineNumbers;
        private Brush _defaultFg, _lineNumbersFg;
        private double _numWidth = 0d;
        private int _numDigits = 0;
#if DEBUG
        private TimeSpan own = new TimeSpan(0), ms = new TimeSpan(0);
#endif
        private long count = 0;
        private static MethodInfo _getLineHeight;
        private LineBuffer _lineBuffer = new LineBuffer();
        private ReadOnlyCollection<FormatInstruction> _noBlockInstr = new List<FormatInstruction>(0).AsReadOnly();

        #region Constructors
        // ...................................................................
        static SyntaxRenderer()
        {
            _getLineHeight = typeof(TextBox)
                .GetMethod("GetLineHeight", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        // ...................................................................
        #endregion

        #region Dependency properties
        // ...................................................................
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
            "Target",
            typeof(TextBox),
            typeof(SyntaxRenderer),
            new PropertyMetadata(null, new PropertyChangedCallback(OnTargetChanged))
        );
        // ...................................................................
        public TextBox Target
        {
            get => ((TextBox)this.GetValue(TargetProperty));
            set => this.SetValue(TargetProperty, value);
        }
        // ...................................................................
        public static readonly DependencyProperty DefaultForegroundProperty = DependencyProperty.Register(
            "DefaultForeground",
            typeof(Brush),
            typeof(SyntaxRenderer),
            new PropertyMetadata(null, new PropertyChangedCallback(OnDefaultForegroundChanged))
        );
        public Brush DefaultForeground
        {
            get => ((Brush)this.GetValue(DefaultForegroundProperty) ?? Brushes.Red);
            set => this.SetValue(DefaultForegroundProperty, value);
        }
        // ...................................................................
        public static readonly DependencyProperty LineNumbersForegroundProperty = DependencyProperty.Register(
           "LineNumbersForeground",
           typeof(Brush),
           typeof(SyntaxRenderer),
           new PropertyMetadata(null, new PropertyChangedCallback(OnLineNumbersForegroundChanged))
        );
        public Brush LineNumbersForeground
        {
            get => ((Brush)this.GetValue(LineNumbersForegroundProperty) ?? Brushes.Red);
            set => this.SetValue(LineNumbersForegroundProperty, value);
        }
        // ...................................................................
        #endregion

        #region Overrides
        // ...................................................................
        /// <summary>
        /// Renders the visible parts of the syntax text and line numbers 
        /// directly to the drawing context.
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            Canvas lineNumbers = this.GetLineNumbersCanvas();
            Size scrollBarSize = this.GetScrollBarSizes();

            Brush synForeground = this._defaultFg ?? Brushes.Red;
            Brush lnForeground = this._lineNumbersFg ?? Brushes.Brown;
            
            Typeface typeface = new Typeface(
                this.Target.FontFamily,
                this.Target.FontStyle,
                this.Target.FontWeight,
                this.Target.FontStretch);

            // Measure the currently visible area.
            double lineHeight = this.GetLineHeight();
            // Check that the underlying textbox still draws transparent text,
            // otherwise save the current value as original foreground and reset 
            // it to transparent.
            if (this.Target.Foreground != Brushes.Transparent)
            {
                this.Target.SetValue(SyntaxBox.OriginalForegroundProperty, this.Target.Foreground);
                this.Target.Foreground = Brushes.Transparent;
            }

            // Index of the FIRST visible line, relative to the entire text.
            int firstVisible = (int)(this.Target.VerticalOffset / lineHeight);
            // Index of the LAST visible line, relative to the entire text.
            int lastVisible = (int)((this.Target.VerticalOffset + this.ActualHeight) / lineHeight);

            // Number of prefix lines in the block. (Used for block highligting operations).
            int prefix = Math.Min(firstVisible, BLOCK_EXTEND);
            
            // Max number of postfix line in the block. (Used for block highligting operations).
            int maxPostfix = BLOCK_EXTEND;

            // Extract the block (visible +100) text lines.
            List<TextLine> lines = this.Target.Text.GetLines(
                firstVisible - prefix,
                lastVisible + maxPostfix,
                out int totalLines);
            List<FormattedLine> formattedLines = this._lineBuffer.GetLines(
                firstVisible - prefix,
                lastVisible + maxPostfix);

            // Calculate the width needed to display line numbers and adjust 
            // part width accordingly.
            double requiredWidth = CalculateRequiredLineNumberWidth(
                totalLines,
                typeface);
            if (requiredWidth != lineNumbers.Width)
                lineNumbers.Width = requiredWidth;

            // Extract the visible lines from the block.
            List<TextLine> visibleLines = lines
                .Where((x) => x.LineNumber >= firstVisible && x.LineNumber <= lastVisible)
                .ToList();

            List<FormattedLine> visibleFormats = this._lineBuffer.GetLines(firstVisible, lastVisible);
#if DEBUG

            // Dummy for testing!
            List<string> visitext__ = visibleLines
                .Select((x) => x.Text)
                .ToList();

            List<string> buffertext__ = visibleFormats
                .Select((x) => x.Text)
                .ToList();

            if (this.Target.Text != this._lineBuffer.ToString())
            {
                this.Target.Background = Brushes.Red;
            }
            else if (this._lineBuffer.Count != totalLines)
            {
                this.Target.Background = Brushes.Red;
            }
            else if (!visitext__.SequenceEqual(buffertext__))
            {
                this.Target.Background = Brushes.Red;
            }
            else
            {
                this.Target.Background = Brushes.White;
            }
#endif
            // Stop if scrolled completely out of view.
            // (This is caused e.g. by deleting all text while line 0 is scrolled out of view).
            if (visibleLines.Count == 0)
            {
                return;
            }

            // If visible, render the line numbers.
            if (requiredWidth > 0)
            {
                // Set a drawing clip to match the line numbers part.
                Rect numbersRect = new Rect(
                    new Point(-requiredWidth, 0),
                    new Size(requiredWidth, this.ActualHeight - this.Target.Padding.Bottom - scrollBarSize.Height)
                );
                drawingContext.PushClip(new RectangleGeometry(numbersRect));

                // Create the line numbers text
                string numbers = String.Join(Environment.NewLine, 
                    visibleLines.Select((l) => (l.LineNumber + 1).ToString()).ToArray());
                FormattedText numbersText = new FormattedText(numbers,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    this.Target.FontSize,
                    lnForeground)
                {
                    LineHeight = lineHeight,
                    TextAlignment = TextAlignment.Right
                };

                // Draw the line numbers. Since the text is right-aligned,
                // the origin point is top-right rather than top-right.
                drawingContext.DrawText(
                    numbersText,
                    new Point(
                        -LN_MARGIN,
                        (firstVisible) * lineHeight - this.Target.VerticalOffset)
                );
                
                drawingContext.Pop();
            }

            // Draw the syntax text.
            // Set a drawing clip matching the renderer size, sans the scrollbars.
            Rect clipRect = new Rect(
                new Size(this.ActualWidth - this.Target.Padding.Right - scrollBarSize.Width,
                this.ActualHeight - this.Target.Padding.Bottom - scrollBarSize.Height)
            );
            drawingContext.PushClip(new RectangleGeometry(clipRect));

            // Join the visible lines into a single text.
            string visibleText = String.Join("", visibleLines.Select((l) => l.Text).ToArray());
            FormattedText syntaxText = new FormattedText(visibleText,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                this.Target.FontSize,
                synForeground)
            {
                LineHeight = lineHeight
            };

            ISyntaxDriver syntaxDriver = this.GetSyntaxDriver();
            if (syntaxDriver != null)
            {
                // Transform the BLOCK instructions to LINE instructions
                if (syntaxDriver.Abilities.HasFlag(DriverOperation.Block)

                    // If any line in the block is invalid (has no format)
                    // we have to rerun the multi-line operations.
                    && formattedLines.Any((ln) => ln.BlockFormatInstructions == null))
                {
                    // Join the BLOCK lines into a single text.
                    string blockText = String.Join("", lines.Select((l) => l.Text).ToArray());
                    int blockOffset = lines[0].StartIndex,
                        blockVisibleOffset = visibleLines[0].StartIndex - lines[0].StartIndex;

                    Dictionary<int, List<FormatInstruction>> convertedBlockInstructions = syntaxDriver
                        // Match all BLOCK operations
                        .Match(DriverOperation.Block, blockText)

                        // Optimization ignoring multiline matches outside of visible area
                        .Where((x) =>
                            (x.FromChar + blockOffset) < visibleLines[visibleLines.Count - 1].EndIndex
                            && (x.FromChar + blockOffset + x.Length) > visibleLines[0].StartIndex)

                        // Cross with all lines in the block
                        .SelectMany((instr) => lines, (instr, line) => new { 
                            instr, 
                            line, 
                            instrStart = instr.FromChar + blockOffset, 
                            instrEnd = instr.FromChar + instr.Length + blockOffset })
                        .Where((x) => x.instrEnd > x.line.StartIndex 
                            && x.instrStart < x.line.StartIndex + x.line.Text.Length)

                        // Convert to line instructions
                        .Select((x) => new
                            {
                                lineno = x.line.LineNumber,
                                instruction = new FormatInstruction
                                {
                                    RuleId = x.instr.RuleId,
                                    Foreground = x.instr.Foreground,
                                    Background = x.instr.Background,
                                    Outline = x.instr.Outline,
                                    FromChar = Math.Max(0, x.instrStart - x.line.StartIndex),
                                    Length = Math.Min(x.instrEnd - x.line.StartIndex, x.line.Text.Length) - Math.Max(0, x.instrStart - x.line.StartIndex)
                                }
                            }
                        )
                        .GroupBy((x) => x.lineno, (x) => x.instruction)
                        .ToDictionary((x) => x.Key, (x) => x.ToList());

                    // Apply the line instructions to the block
                    //var none = new List<FormatInstruction>(0);
                    for (int i = 0; i < formattedLines.Count; i++)
                    {
                        int lineno = lines[i].LineNumber;
                        convertedBlockInstructions.TryGetValue(lineno, out List<FormatInstruction> converted);
                        formattedLines[i].BlockFormatInstructions = converted ?? this._noBlockInstr.ToList();
                    }
                }
                
                // Apply Line syntax operations
                int visibleOffset = visibleLines[0].StartIndex;
                for (int i = 0; i < visibleLines.Count; i++)
                {
                    TextLine line = visibleLines[i];
                    FormattedLine format = visibleFormats[i];
                    int localOffset = line.StartIndex - visibleOffset;
                    if (format.LineFormatInstructions == null)
                    {
                        format.LineFormatInstructions = syntaxDriver.Match(DriverOperation.Line, line.Text).ToList();
                    }
                    
                    // Concatenate the line formats with the block formats.
                    // Since block formats re last, they will have precedence.
                    var lineInstructions = format.LineFormatInstructions.Concat(format.BlockFormatInstructions 
                        ?? (IEnumerable<FormatInstruction>)this._noBlockInstr).ToList();

                    // Sort the rules so they are applied in the order they were declared.
                    lineInstructions.Sort((a, b) => a.RuleId.CompareTo(b.RuleId));
                    foreach (var instruction in lineInstructions)
                    {
                        if (instruction.Foreground != null)
                        {

                            syntaxText.SetForegroundBrush(
                                instruction.Foreground,
                                instruction.FromChar + localOffset,
                                instruction.Length);
                        }
                        if (instruction.Background != null || instruction.Outline != null)
                        {
                            Rect highlight = GetSubstringRect(
                                line.Text,
                                instruction.FromChar,
                                instruction.Length,
                                typeface,
                                this.Target.FontSize,
                                FlowDirection.LeftToRight);

                            highlight.Offset(
                                2d - this.Target.HorizontalOffset,
                                (line.LineNumber) * lineHeight - this.Target.VerticalOffset);
                            drawingContext.DrawRectangle(instruction.Background, instruction.Outline, highlight);
                        }
                    }
                }
            }
           
            // Draw the text onto the context.
            drawingContext.DrawText(
                syntaxText,
                new Point(
                    2 - this.Target.HorizontalOffset,
                    (firstVisible) * lineHeight - this.Target.VerticalOffset)
            );
            drawingContext.Pop();
#if DEBUG
            sw.Stop();
            ms += sw.Elapsed;
            count++;
#endif
            base.OnRender(drawingContext);
        }
        // ...................................................................
        #endregion

        #region Private members
        // ...................................................................
        /// <summary>
        /// Gets the configured syntax drivers as an aggregate driver.
        /// </summary>
        /// <returns>A syntax driver or null</returns>
        private ISyntaxDriver GetSyntaxDriver()
        {
            SyntaxDriverCollection driverCollection = SyntaxBox.GetSyntaxDrivers(this.Target);
            if (driverCollection == null)
                return (null);

            var aggregateDriver = new AggregateSyntaxDriver(driverCollection);
            return (aggregateDriver);
        }
        // ...................................................................
        /// <summary>
        /// Calculates a rectangle for highlighting a specific part of a text line.
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="From"></param>
        /// <param name="Count"></param>
        /// <param name="Typeface"></param>
        /// <param name="FontSize"></param>
        /// <param name="FlowDirection"></param>
        /// <returns></returns>
        private static Rect GetSubstringRect(string Line, int From, int Count, Typeface Typeface, double FontSize, FlowDirection FlowDirection)
        {
            double left = 0d, width = 0d;
            if (From > 0)
            {
                FormattedText prefix = new FormattedText(
                    Line.Substring(0, From),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection,
                    Typeface,
                    FontSize,
                    Brushes.Black);
                left = prefix.WidthIncludingTrailingWhitespace;
            }
            FormattedText highlight = new FormattedText(
                    Line.Substring(From, Count),
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection,
                    Typeface,
                    FontSize,
                    Brushes.Black);
            width = highlight.Width;
            return (new Rect(
                new Point(left -1, 0d),
                new Size(Math.Round(width + 2), highlight.Height)
                )
            );
        }
        // ...................................................................
        /// <summary>
        /// Calculates the required width of the line number canvas.
        /// </summary>
        /// <param name="LineCount">The line count.</param>
        /// <param name="Typeface">The typeface.</param>
        /// <returns></returns>
        private double CalculateRequiredLineNumberWidth(int LineCount, Typeface Typeface)
        {
            bool showLineNumbers = SyntaxBox.GetShowLineNumbers(this.Target);
            if (!showLineNumbers)
                return (0d);

            int digits = (int)Math.Floor(Math.Log10(Math.Max(LineCount, 1)) + 1);
            if (digits != this._numDigits)
            {
                this._numDigits = digits;
                string requiredChars = String.Empty.PadLeft(digits, '0');
                FormattedText requiredText = new FormattedText(requiredChars,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    Typeface,
                    this.Target.FontSize,
                    Brushes.Black);
                this._numWidth = requiredText.Width + LN_MARGIN * 2;
            }
            
            return (this._numWidth);
        }
        // ...................................................................
        /// <summary>
        /// Gets the scroll viewer part. 
        /// </summary>
        /// <returns></returns>
        private ScrollViewer GetScrollViewer()
        {
            this.AttachScrollViewer();
            return (this._scrollViewer);
        }
        // ...................................................................
        /// <summary>
        /// Attaches the scroll viewer ScrollChanged event. Since the ScrollViewer
        /// isn't created untill after the renderer is initialized, the scroll 
        /// viewer events can't be attached by the attahced property. Instead
        /// this method has to be called sometime after the initialization process.
        /// (OnRender or when the scroll viewer is first used).
        /// </summary>
        private void AttachScrollViewer()
        {
            if (this._scrollViewer == null)
            {
                this._scrollViewer = (ScrollViewer)this.Target?.Template?.FindName("PART_ContentHost", this.Target);
                if (this._scrollViewer != null)
                {
                    this._scrollViewer.ScrollChanged += this.SyntaxScrollChanged;
                }
            }
        }
        // ...................................................................
        /// <summary>
        /// Gets visible size of the scroll bars as a size. 
        /// Width => vertical scroll bar Width,
        /// Height => horizontal scroll bar Height.
        /// 
        /// This will return 0 for any dimension that is currently hidden.
        /// </summary>
        /// <returns></returns>
        private Size GetScrollBarSizes()
        {
            return (new Size(
                (this.GetScrollViewer()?.ComputedVerticalScrollBarVisibility ?? Visibility.Collapsed) == Visibility.Collapsed
                    ? 0d
                    : SystemParameters.VerticalScrollBarWidth,
                (this.GetScrollViewer()?.ComputedHorizontalScrollBarVisibility ?? Visibility.Collapsed) == Visibility.Collapsed
                    ? 0d
                    : SystemParameters.HorizontalScrollBarHeight
                ));
        }
        // ...................................................................
        private Canvas GetLineNumbersCanvas()
        {
            if (this._lineNumbers == null)
            {
                this._lineNumbers = (Canvas)this.Target?.Template?.FindName("PART_LineNumbers", this.Target);
            }
            return (this._lineNumbers);
        }
        // ...................................................................
        /// <summary>
        /// Calls the TextBox.GetLineHeight of Target via reflection.
        /// </summary>
        /// <returns></returns>
        private double GetLineHeight()
        {
            // The built-in TextBox method takes scaling into account
            // and should be more correct.
            // Using this, the synchronization doesn't seem to be needed.
            return ((double)_getLineHeight.Invoke(this.Target, null));
        }
        // ...................................................................
        /// <summary>
        /// Decreases the indentation of the current text selection.
        /// </summary>
        private void DecreaseBlockIndent()
        {
            int 
                selStart = this.Target.SelectionStart, 
                selLength = this.Target.SelectionLength, 
                selEnd = selStart + selLength, 
                firstLine = this.Target.GetLineIndexFromCharacterIndex(selStart), 
                lastLine = this.Target.GetLineIndexFromCharacterIndex(selStart + selLength - 1),
                indentCount = SyntaxBox.GetIndentCount(this.Target);

            // More perfroamnt than using built-in TextBox functions.
            var affectedLines = this.Target.Text.GetLines(firstLine, lastLine, out int totalLines).ToList();

            // These are the offset of the selection start/end from the END
            // of the first/last affected lines. They are used to reset the
            // selection after the operation.
            int selStartOffset = affectedLines[0].EndIndex - selStart;
            int selEndOffset = affectedLines[affectedLines.Count - 1].EndIndex - selEnd;

            // Decrease indent for the affected block.
            var unindentedBlock = String.Join("", affectedLines.Select((line) =>
            {
                string indent = new String(line.Text.TakeWhile((c) => c == INDENT_CHAR).ToArray());

                int newCount = (indent.Length % indentCount) != 0
                    ? (indent.Length / indentCount) * indentCount
                    : Math.Max(0, ((indent.Length / indentCount) - 1) * indentCount);

                string newIndent = String.Empty.PadLeft(newCount, INDENT_CHAR);
                string unindented = newIndent + line.Text.Substring(indent.Length);
                return (unindented);
            })
            .ToArray());

            // Update the text
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Target.Text.Substring(0, affectedLines[0].StartIndex));
            sb.Append(unindentedBlock);
            sb.Append(this.Target.Text.Substring(affectedLines[affectedLines.Count - 1].StartIndex + affectedLines[affectedLines.Count - 1].Text.Length));
            this.Target.Text = sb.ToString();

            // Reset the selection and caret
            var firstAffected = this.Target.Text.GetLines(firstLine, firstLine, out totalLines).Single();
            var lastAffected = this.Target.Text.GetLines(lastLine, lastLine, out totalLines).Single();
            selStart = Math.Max(firstAffected.StartIndex, firstAffected.EndIndex - selStartOffset);
            selEnd = Math.Max(
                lastAffected.StartIndex,
                lastAffected.EndIndex - selEndOffset);
            selLength = selEnd - selStart;
            this.Target.Select(selStart, selLength);
        }
        // ...................................................................
        /// <summary>
        /// Increases the indentation of the current text selection.
        /// </summary>
        private void IncreaseBlockIndent()
        {
            int
                selStart = this.Target.SelectionStart,
                selLength = this.Target.SelectionLength,
                selEnd = selStart + selLength,
                firstLine = this.Target.GetLineIndexFromCharacterIndex(selStart),
                lastLine = this.Target.GetLineIndexFromCharacterIndex(selStart + selLength - 1),
                indentCount = SyntaxBox.GetIndentCount(this.Target);

            // More perforamnt than using built-in TextBox functions.
            var affectedLines = this.Target.Text.GetLines(firstLine, lastLine, out int totalLines).ToList();

            // These are the offset of the selection start/end from the END
            // of the first/last affected lines. They are used to reset the
            // selection after the operation.
            int selStartOffset = affectedLines[0].EndIndex - selStart;
            int selEndOffset = affectedLines[affectedLines.Count - 1].EndIndex - selEnd;

            // Increase indent for the affected block.
            var indentedBlock = String.Join("", affectedLines.Select((line) =>
            {
                string indent = new String(line.Text.TakeWhile((c) => c == INDENT_CHAR).ToArray());

                int addCount = ((indent.Length % indentCount) != 0)
                    ? indentCount - (indent.Length % indentCount)
                    : indentCount;

                string addString = String.Empty.PadLeft(addCount, INDENT_CHAR);
                string indented = addString + line.Text;
                return (indented);
            })
            .ToArray());

            // Update the text
            StringBuilder sb = new StringBuilder();
            sb.Append(this.Target.Text.Substring(0, affectedLines[0].StartIndex));
            sb.Append(indentedBlock);
            sb.Append(this.Target.Text.Substring(affectedLines[affectedLines.Count - 1].StartIndex + affectedLines[affectedLines.Count - 1].Text.Length));
            this.Target.Text = sb.ToString();

            // Reset the selection and caret
            var firstAffected = this.Target.Text.GetLines(firstLine, firstLine, out totalLines).Single();
            var lastAffected = this.Target.Text.GetLines(lastLine, lastLine, out totalLines).Single();
            selStart = firstAffected.EndIndex - selStartOffset;
            selEnd = lastAffected.EndIndex - selEndOffset;
            selLength = selEnd - selStart;
            this.Target.Select(selStart, selLength);
        }
        // ...................................................................
        /// <summary>
        /// Decreases the indentation at the current caret position.
        /// </summary>
        private void DecreaseCaretIndent()
        {
            int indentCount = SyntaxBox.GetIndentCount(this.Target);
            int caretIndex = this.Target.CaretIndex;
            // Decrease indent
            TextLine line = this.Target.Text.GetLineAtPosition(caretIndex);
            if (line != null)
            {
                int currentIndent;
                for (currentIndent = 0; (caretIndex - currentIndent) > line.StartIndex; currentIndent++)
                {
                    if (this.Target.Text[caretIndex - currentIndent - 1] != INDENT_CHAR)
                        break;
                }

                if (currentIndent > 0)
                {
                    string newIndent = string.Empty;
                    int removeChars = currentIndent % indentCount;
                    if (removeChars == 0)
                    {
                        removeChars = indentCount;
                    }
                    this.Target.Text = this.Target.Text.Substring(0, caretIndex - removeChars)
                            + this.Target.Text.Substring(caretIndex);
                    this.Target.CaretIndex = caretIndex - removeChars;
                }
            }
        }
        // ...................................................................
        /// <summary>
        /// Icreases the indentation at the current caret position.
        /// </summary>
        private void IncreaseCaretIndent()
        {
            int indentCount = SyntaxBox.GetIndentCount(this.Target);
            int caretIndex = this.Target.CaretIndex;
            TextLine line = this.Target.Text.GetLineAtPosition(caretIndex);
            if (line != null)
            {
                int currentIndent;
                for (currentIndent = 0; (caretIndex - currentIndent) > line.StartIndex; currentIndent++)
                {
                    if (this.Target.Text[caretIndex - currentIndent - 1] != INDENT_CHAR)
                        break;
                }

                int nextIndent = ((currentIndent % indentCount) != 0)
                    ? indentCount - (currentIndent % indentCount)
                    : indentCount;

                // Increase indent
                TextCompositionManager.StartComposition(
                    new TextComposition(
                        InputManager.Current,
                        this.Target,
                        String.Empty.PadLeft(nextIndent, INDENT_CHAR))
                );
            }
        }
        // ...................................................................
        #endregion

        #region Event handlers
        // ...................................................................
        /// <summary>
        /// Triggered when the target textbox changes. Detaches from any old 
        /// textbox and attaches events on the new instance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnTargetChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SyntaxRenderer renderer = (SyntaxRenderer)sender;
            TextBox oldTextBox = (e.OldValue as TextBox);
            TextBox newTextBox = (e.NewValue as TextBox);

            if (oldTextBox != null)
            {
                oldTextBox.TextChanged -= renderer.TargetTextChanged;
                oldTextBox.PreviewKeyDown -= renderer.TargetPreviewKeyDown;
                renderer._scrollViewer = (ScrollViewer)oldTextBox.Template.FindName("PART_ContentHost", oldTextBox);
                if (renderer._scrollViewer != null)
                {
                    renderer._scrollViewer.ScrollChanged -= renderer.SyntaxScrollChanged;
                    
                }
            }
            if (newTextBox != null)
            {
                newTextBox.TextChanged += renderer.TargetTextChanged;
                newTextBox.PreviewKeyDown += renderer.TargetPreviewKeyDown;
                renderer._lineBuffer = new LineBuffer(newTextBox.Text);
            }
            else
            {
                renderer._lineBuffer = new LineBuffer();
            }
        }
        // ...................................................................
        public static void OnDefaultForegroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SyntaxRenderer renderer = (SyntaxRenderer)sender;
            renderer._defaultFg = e.NewValue as Brush;
        }
        // ...................................................................
        public static void OnLineNumbersForegroundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SyntaxRenderer renderer = (SyntaxRenderer)sender;
            renderer._lineNumbersFg = e.NewValue as Brush;
        }
        // ...................................................................
        private void SyntaxScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.InvalidateVisual();
        }
        // ...................................................................
        /// <summary>
        /// Invalidates the control on TextChanged, allowing syntax highlighting 
        /// to execute.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TargetTextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (var change in e.Changes)
            {
                string addedText = this.Target.Text.Substring(change.Offset, change.AddedLength);
                this._lineBuffer.ApplyChange(change, addedText);
            }
            this.InvalidateVisual();
        }
        // ...................................................................
        /// <summary>
        /// Handles keyboard input like tab indent expansion.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TargetPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.Target != (TextBox)sender)
                return;

            if (e.Key == System.Windows.Input.Key.Tab)
            {
                if (SyntaxBox.GetExpandTabs(this.Target))
                {
                    // Handles single-point indent/unindent from current caret position
                    if (this.Target.SelectionLength == 0)
                    {
                        if (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift))
                        {
                            this.DecreaseCaretIndent();
                        }
                        else
                        {
                            this.IncreaseCaretIndent();
                        }
                    }

                    // Handle line/multi-line indent from current selection
                    else
                    {
                        if (e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift))
                        {
                            // Decrease indent
                            this.DecreaseBlockIndent();
                        }
                        else
                        {
                            // Increase indent
                            this.IncreaseBlockIndent();
                        }
                    }
                    e.Handled = true;
                }
            }
            else if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (SyntaxBox.GetAutoIndent(this.Target))
                {
                    TextLine line = this.Target.Text.GetLineAtPosition(this.Target.CaretIndex);
                    if (line != null)
                    {
                        int len;
                        for (len = 0; len < line.Text.Length; len++)
                        {
                            char c = line.Text[len];
                            if (!(Char.IsWhiteSpace(c) && c != '\n' && c != '\r'))
                                break;
                        }
                        string prefix = line.Text.Substring(0, len);
                        System.Windows.Documents.EditingCommands.EnterLineBreak.Execute(null, this.Target);
                        TextCompositionManager.StartComposition(
                                new TextComposition(
                                    InputManager.Current,
                                    this.Target,
                                    prefix));
                        e.Handled = true;
                    }
                }
            }
        }
        // ...................................................................
        #endregion
    }
}

