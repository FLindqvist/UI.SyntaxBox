using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace UI.SyntaxBox
{
    /// <summary>
    /// Canonical algorithm for quickly finding all occurances of muiltiple 
    /// keywords in a longer text.
    /// </summary>
    public class AhoCorasickSearch
    {
        // Maximum number of states
        private readonly int _maxStates;
            
        // Number of terminal tokens (chars) in the alphabet.
        private const int MAX_CHARS = 256;

        // Automation tables
        private int[] _fail;
        private HashSet<int>[] _output;
        private List<int[]> _goto;

        private List<string> _dictionary;

        private bool _matchWholeWords = true;
        private bool _overlappingMatches = false;

        #region Constructors
        // ...................................................................
        /// <summary>
        /// Creates an Aho-Corasick keyword search automation.
        /// </summary>
        /// <param name="Dictionary">The list of words t osearch for.</param>
        /// <param name="MatchWholeWords">
        /// Ensures that matches start and end on a word boundary (alphanumeric or '_').
        /// Default = <c>true</c>.
        /// </param>
        /// <param name="OverlappingMatches">
        /// If enabled, overlapping matches are allowed, e.g. elsif => elsif, if.
        /// Default = <c>false</c>.
        /// </param>
        public AhoCorasickSearch(
            List<string> Dictionary,
            bool MatchWholeWords = true,
            bool OverlappingMatches = false)
        {
            this._matchWholeWords = MatchWholeWords;
            this._overlappingMatches = OverlappingMatches;

            this._dictionary = Dictionary
                .OrderByDescending((x) => x.Length)
                .ToList();

            // The needed number of states is the sum length of the words in the 
            // dictionary + 1 (for the initial state).
            this._maxStates = this._dictionary.Sum((word) => word.Length) + 1;

            this.InitiateTables();
            this.InitiateAutomation();
        }
        // ...................................................................
        #endregion

        #region Private members
        // ...................................................................
        /// <summary>
        /// Creates and initiates all automation tables
        /// </summary>
        private void InitiateTables()
        {
            // Create and initiate the OUTPUT table to 0
            this._output = new HashSet<int>[_maxStates];
            for (int i = 0; i < this._maxStates; i++)
                this._output[i] = new HashSet<int>();

            // Create and initiate the FAIL table to 1
            this._fail = new int[_maxStates];
            for (int i = 0; i < _maxStates; i++)
                this._fail[i] = -1;

            // Create and initiate the GOTO table to -1.
            int[] template = new int[MAX_CHARS];
            for (int i = 0; i < MAX_CHARS; i++)
                template[i] = -1;

            this._goto = new List<int[]>(this._maxStates);
            this._goto.Add(template);
            for (int i = 1; i < _maxStates; i++)
                this._goto.Add((int[])template.Clone());
        }
        // ...................................................................
        /// <summary>
        /// Initiates the automation, loading it into the created tables.
        /// </summary>
        private void InitiateAutomation()
        {
            // Starting state
            int states = 1;

            // Build the GOTO table forward states.
            // This will result in a table where state 0 contains all first 
            // characters in all keywords, with gotos to the next character
            // state in each keyword: (ax, by)
            //   GOTO            | OUTPUT | FAIL   |
            // -------------------------------------
            //   | a | b | x | y |        |        |
            // -------------------------------------
            // 0 | 1 | 3 | -1| -1| -      | -1     |
            // 1 | -1| -1|(2)| -1| -      | -1     |
            // 2 | -1| -1| -1| -1| 0 (ax) | -1     |
            // 3 | -1| -1| -1|(4)| -      | -1     |
            // 4 | -1| -1| -1| -1| 1 (by) | -1     |
            // -------------------------------------
            for (int i = 0; i < this._dictionary.Count; i++)
            {
                string word = this._dictionary[i];

                // Always start at the root state
                int currentState = 0;
                foreach (char ch in word)
                {
                    if (ch > MAX_CHARS)
                        throw new InvalidOperationException($"Only the first {MAX_CHARS} characters are allowed!");

                    // If the current state (0) doesn't contain the first character,
                    // create a goto (0) -> (new state).
                    if (_goto[currentState][ch] == -1)
                    {
                        _goto[currentState][ch] = states++;
                    }
                    currentState = _goto[currentState][ch];
                }

                // Add the current keyword final state as output state
                _output[currentState].Add(i);
            }

            Queue<int> queue = new Queue<int>();


            //   GOTO            | OUTPUT | FAIL   |
            // -------------------------------------
            //   | a | b | x | y |        |        |
            // -------------------------------------
            // 0 | 1 | 3 |  0|  0| -      | -1     |
            // 1 | -1| -1|(2)| -1| -      |  0     |
            // 2 | -1| -1| -1| -1| 0 (ax) | -1     |
            // 3 | -1| -1| -1|(4)| -      |  0     |
            // 4 | -1| -1| -1| -1| 1 (by) | -1     |
            // -------------------------------------
            // q: [ 1, 3 ]
            for (int i = 0; i < MAX_CHARS; i++)
            {
                // Reset remaining upopulated goto of state 0 to revert to itself,
                // meaning try with  the next character.
                if (this._goto[0][i] == -1)
                {
                    this._goto[0][i] = 0;
                }

                // Set the 1-depth states to fail back to state 0,
                // meaning try again from scratch wit hthe current character.
                else
                {
                    int gotoValue = this._goto[0][i];
                    this._fail[gotoValue] = 0;
                    queue.Enqueue(gotoValue);
                }
            }

            // Populate the remaining fail and goto states.
            // Fail back to last previous substring (where different keywords 
            // have the same prefix) and add outputs.
            //   GOTO            | OUTPUT | FAIL   |
            // -------------------------------------
            //   | a | b | x | y |        |        |
            // -------------------------------------
            // 0 | 1 | 3 |  0|  0| -      | -1     |
            // 1 | -1| -1|(2)| -1| -      |  0     |
            // 2 | -1| -1| -1| -1| 0 (ax) |  0     |
            // 3 | -1| -1| -1|(4)| -      |  0     |
            // 4 | -1| -1| -1| -1| 1 (by) |  0     |
            // -------------------------------------
            // q: [ 1, 3 ]
            while (queue.Count > 0)
            {
                int state = queue.Dequeue();

                // Find the the failure function for the state
                // for which a goto is not defined.
                for (int i = 0; i < MAX_CHARS; i++)
                {
                    // If a goto is defined:
                    if (this._goto[state][i] != -1)
                    {

                        int failure = this._fail[state];

                        while (this._goto[failure][i] == -1)
                        {
                            failure = this._fail[failure];
                        }

                        failure = this._goto[failure][i];
                        this._fail[this._goto[state][i]] = failure;

                        this._output[this._goto[state][i]].AddRange(this._output[failure]);

                        queue.Enqueue(this._goto[state][i]);
                    }
                }
            }
        }
        // ...................................................................
        /// <summary>
        /// Finds the next state based on the current state and the next input 
        /// character.
        /// </summary>
        /// <param name="CurrentState"></param>
        /// <param name="InputChar"></param>
        /// <returns></returns>
        private int NextState(int CurrentState, int InputChar)
        {
            int current = CurrentState;
            while (this._goto[current][InputChar] == -1)
            {
                current = this._fail[current];
            }

            return (this._goto[current][InputChar]);
        }
        // ...................................................................
        #endregion

        #region Public members
        // ...................................................................
        /// <summary>
        /// Scans Text returning all occurances of any keyword in the dictionary.
        /// </summary>
        /// <param name="Text"></param>
        public IEnumerable<Substring> FindAll(string Text)
        {
            // This is used when OverlappingMatches = false,
            // to produce the longest possible match where keywords
            // have common prefixes (if, ifthen).
            // Instead of emitting the match directly, the match is 
            // cached and emitted on the next failed match.
            Substring prevMatch = null;

            int current = 0;
            for (int i = 0; i < Text.Length; i++)
            {
                current = this.NextState(current, Text[i]);
                
                if (this._output[current].Count != 0)
                {
                    for (int j = 0; j < this._dictionary.Count; j++)
                    {
                        if ((this._output[current].Contains(j)))
                        {
                            int length = this._dictionary[j].Length;
                            int firstChar = i - length + 1;

                            if (!this._matchWholeWords
                                || Text.IsStartWordBoundary(firstChar)
                                && Text.IsEndWordBoundary(i))
                            {
                                var substring = new Substring
                                    {
                                        Position = firstChar,
                                        Length = length,
                                        Value = this._dictionary[j]
                                    };
                                if (this._overlappingMatches)
                                {
                                    yield return substring;
                                }
                                else
                                {
                                    // If we have a previous match with a different starting position
                                    // than the current one, we have to emit it.
                                    if (prevMatch != null && prevMatch.Position != substring.Position)
                                    {
                                        yield return prevMatch;
                                    }

                                    // In case we don't want overlapping matches,
                                    // cache the match and emit on the next failed match.
                                    // (Handles common prefixes).
                                    prevMatch = substring;

                                    // We get the longest match first from the dictionary, 
                                    // so to handle common postfixes, we break here.
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // If a match occurs at the end of the input, we have a tail
            // to emit.
            if (prevMatch != null)
            {
                yield return prevMatch;
                prevMatch = null;
            }
        }
        // ...................................................................
        #endregion
    }
}
