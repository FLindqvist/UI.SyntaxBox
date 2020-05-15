using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Globalization;

/// <summary>
/// This file contains all classes related to KeySequence*.
/// KeySequence implements multi-key shortcuts, like Ctrl+K, Ctrl+C
/// int VS. 
/// </summary>
namespace UI.SyntaxBox
{
    // ###################################################################
    /// <summary>
    /// Entry point to create a multi-key binding sequence. Simply 
    /// introduces a new type converter for the Gesture property, to  parse
    /// sequences on the form Modifier+Key1, Key2, ..., KeyN.
    /// Usage:
    /// <c>
    /// <Window.InputBindings>
    ///     <syntax:KeySequenceBinding Gesture="Ctrl+A, B" Command="..." />
    ///     <syntax:KeySequenceBinding Gesture="Ctrl+A, C" Command="..." />
    /// </Window.InputBindings>
    /// </c>
    /// </summary>
    public class KeySequenceBinding : InputBinding
    {
        [TypeConverter(typeof(KeySequenceConverter))]
        public override InputGesture Gesture
        {
            get => base.Gesture;
            set => base.Gesture = value;
        }
    }
    // ###################################################################
    /// <summary>
    /// Gesture implementation. Keeps a sequence of keys and advances the 
    /// pointer in the sequence if a keystroke matches.
    /// </summary>
    public class KeySequenceGesture : KeyGesture
    {
        ModifierKeys _modifiers;
        IList<Key> _keys;
        public int _pointer = 0;

        #region Constructors
        // ...................................................................
        /// <summary>
        /// Ensures that the default constructor cannot be used externally.
        /// </summary>
        /// <param name="DisplayString"></param>
        private KeySequenceGesture(string DisplayString)
            : base(Key.None, ModifierKeys.None, DisplayString)
        { /* ... */ }
        // ...................................................................
        /// <summary>
        /// Initializing constructor.
        /// </summary>
        /// <param name="Modifiers">Modifier keys</param>
        /// <param name="Keys">List of keys in the sequence</param>
        /// <param name="DisplayString">Display string for the sequence</param>
        public KeySequenceGesture(
            ModifierKeys Modifiers, 
            IList<Key> Keys, 
            string DisplayString)
            : this(DisplayString)
        {
            this._modifiers = Modifiers;
            this._keys = Keys;
        }
        // ...................................................................
        #endregion

        #region Overrides
        // ...................................................................
        /// <summary>
        /// Matches an input event to the current state of the instance.
        /// If a match is found, the _pointer is advanced forward in the sequence.
        /// Returns true only if the match is made on the LAST element in the 
        /// sequence.
        /// </summary>
        /// <param name="targetElement"></param>
        /// <param name="inputEventArgs"></param>
        /// <returns></returns>
        public override bool Matches(
            object targetElement, 
            InputEventArgs inputEventArgs)
        {
            KeyEventArgs keyArgs = (inputEventArgs as KeyEventArgs);
            if (keyArgs == null)
                return (false);

            if (keyArgs.IsRepeat)
            {
                return (false);
            }
            // Wrong input => fail and reset.
            if (Keyboard.Modifiers != this._modifiers || keyArgs.Key != this._keys[this._pointer])
            {
                this._pointer = 0;
                return (false);
            }
            // Matches current element in sequence => set to handled and advance
            else
            {
                keyArgs.Handled = true;
                this._pointer++;
            }

            // If we now passed the tail of the sequence, return true
            if (this._pointer >= this._keys.Count)
            {
                this._pointer = 0;
                return (true);
            }
            return (false);
        }
        // ...................................................................
        #endregion
    }
    // ###################################################################
    /// <summary>
    /// Converts a string into either a KeyGEsture or a KeySequenceGesture 
    /// if multiple keys are specified.
    /// </summary>
    public class KeySequenceConverter : TypeConverter
    {
        #region Overrides
        // ...................................................................
        /// <summary>
        /// Determines whether this instance can convert from the specified sourceType.
        /// Can convert from string and nothing else.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can convert from] the specified context; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }
        // ...................................................................
        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">value</exception>
        /// <exception cref="ArgumentException">Argument must be of string type - value</exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            string sInput = value as string;
            if (sInput == null)
                throw new ArgumentException("Argument must be of string type", nameof(value));

            ModifierKeys modifiers = ModifierKeys.None;

            int lastSplit = sInput.LastIndexOf('+');
            if (lastSplit > 1)
            {
                string sMod = sInput.Substring(0, lastSplit + 1);
                modifiers = (ModifierKeys)new ModifierKeysConverter().ConvertFromString(context, culture, sMod);
            }

            string sKeys = sInput.Substring(lastSplit + 1, sInput.Length - lastSplit - 1);

            KeyConverter keyConv = new KeyConverter();
            var keys = (sKeys)
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select((x) => (Key)keyConv.ConvertFromString(context, culture, x.Trim()))
                .ToList();

            // Fall back to standard key gesture if this is not a sequence.
            return (keys.Count == 1)
                ? new KeyGesture(keys[0], modifiers, sInput)
                : new KeySequenceGesture(modifiers, keys, sInput)
                ;
        }
        // ...................................................................
        #endregion
    }
    // ###################################################################
}
