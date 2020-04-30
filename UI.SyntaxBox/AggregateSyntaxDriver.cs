using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.SyntaxBox
{
    /// <summary>
    /// Aggregates a driver collection to be used using the singular ISyntaxDriver
    /// interface.
    /// </summary>
    class AggregateSyntaxDriver : ISyntaxDriver
    {
        SyntaxDriverCollection _drivers;

        // ...................................................................
        internal AggregateSyntaxDriver(SyntaxDriverCollection Drivers)
        {
            this._drivers = Drivers ?? throw new ArgumentNullException(nameof(Drivers));
        }
        // ...................................................................

        #region ISyntaxDriver members
        // ...................................................................
        public DriverOperation Abilities
        {
            get
            {
                var abilities = this._drivers
                    .Select((x) => x.Abilities)
                    .Aggregate(DriverOperation.None, (a, b) => a | b);

                return (abilities);
            }
        }
        // ...................................................................
        public IEnumerable<FormatInstruction> Match(DriverOperation Operation, string Text)
        {
            var result = this._drivers
                .Where((driver) => driver.Abilities.HasFlag(Operation))
                .SelectMany((driver) => driver.Match(Operation, Text))
                .ToList();
            return (result);
        }
        // ...................................................................
        #endregion
    }
}
