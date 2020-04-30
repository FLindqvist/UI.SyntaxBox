using System.Collections.Generic;
using System.Windows.Media;

namespace UI.SyntaxBox
{
    public interface ISyntaxRule
    {
        int RuleId { get; set; }
        DriverOperation Op { get; set; }
        IEnumerable<FormatInstruction> Match(string Text);
    }
}