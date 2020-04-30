using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.SyntaxBox
{
    /// <summary>
    /// Impements a regex-based syntax driver that is configurable directly
    /// within XAML.
    /// </summary>
    public class SyntaxConfig 
        : List<ISyntaxRule>
        , ISyntaxDriver
    {
        private Lazy<Dictionary<DriverOperation, List<ISyntaxRule>>> _ruleIndex;

        #region Public members
        // ...................................................................
        public SyntaxConfig()
        {
            // Lazy-build an index of all rules so we can get the right
            // kind of rule quickly.
            this._ruleIndex = new Lazy<Dictionary<DriverOperation, List<ISyntaxRule>>>(
                () =>
                    {
                        // Set the ID of each rule to it's position.
                        // This is used for sorting the matches later.
                        for (int i = 0; i < this.Count; i++)
                            this[i].RuleId = i;

                        return this.GroupBy((rule) => rule.Op)
                            .ToDictionary(
                                (group) => group.Key,
                                (group) => group.ToList());
                    },
                System.Threading.LazyThreadSafetyMode.PublicationOnly);
        }
        // ...................................................................
        #endregion

        #region ISyntaxDriver members
        // ...................................................................
        public DriverOperation Abilities => this
            .Select((rule) => rule.Op)
            ?.Aggregate(DriverOperation.None, (a, b) => a | b)
            ?? DriverOperation.None;
        // ...................................................................
        public IEnumerable<FormatInstruction> Match(DriverOperation Operation, string Text)
        {
            if (!this._ruleIndex.Value.TryGetValue(Operation, out List<ISyntaxRule> rules))
                return (new FormatInstruction[0]);

            List<FormatInstruction> matches = rules
                .SelectMany((rule) => rule.Match(Text))
                .ToList();

            return (matches);
        }
        // ...................................................................
        #endregion
    }
}
