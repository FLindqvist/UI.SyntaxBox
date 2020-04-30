using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.SyntaxBox
{
    /// <summary>
    /// Implements the Boyer-Moore pattern matching algorithm used with 
    /// the keyword driver. The algorithm preprocesses the search pattern
    /// and the class should be re-used rather than re-created.
    /// </summary>
    public class BoyerMooreSearch
    {
        private string _pattern;

        #region Constructors
        // ...................................................................
        /// <summary>
        /// Initiates a new instance of BoyerMooreSearch 
        /// </summary>
        /// <param name="Pattern">The pattern to match (not null or empty)</param>
        public BoyerMooreSearch(string Pattern)
        {
            this._pattern = Pattern ?? throw new ArgumentNullException(nameof(Pattern));
            if (Pattern.Length == 0)
            {
                throw new ArgumentException("Patterns cannot be an empty string", nameof(Pattern));
            }
        }
        // ...................................................................
        #endregion

        #region Public members
        // ...................................................................
        /// <summary>
        /// Finds all occurrances of the pattern in Text.
        /// </summary>
        /// <param name="Text">The text to search</param>
        /// <returns>
        /// A range of character start positions in text where the pattern
        /// occurrs.
        /// </returns>
        public IEnumerable<int> FindAll(string Text)
        {
            if (false)
                yield return (0); // TODO: Dummy
        }
        // ...................................................................
        #endregion
    }
}
