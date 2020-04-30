using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace UI.SyntaxBox
{
    /// <summary>
    /// Class syntax highlighting behavior that can be attached to any WPF TextBox.
    /// </summary>
    public class SyntaxBox : DependencyObject
    {
        static ControlTemplate 
            _syntaxTemplate, 
            _defaultTemplate;

        #region Constructors
        // ...................................................................
        /// <summary>
        /// Static initializer for the <see cref="SyntaxBox"/> class.
        /// Reads the resource dictionary and extracts the control templates.
        /// </summary>
        static SyntaxBox()
        {
            var asmName = System.Reflection.Assembly.GetExecutingAssembly().FullName;
            var myDictionary = new ResourceDictionary();
            myDictionary.Source = new Uri($"/{asmName};component/Resources.xaml", UriKind.RelativeOrAbsolute);
            _syntaxTemplate = (ControlTemplate)myDictionary["SyntaxTextBoxTemplate"];
            _defaultTemplate = (ControlTemplate)myDictionary["DefaultTextBoxTemplate"];
        }
        // ...................................................................
        #endregion

        #region Attached properties

        #region Enabled
        // ...................................................................
        /// <summary>
        /// Enables/disables syntax highlighting on any textbox
        /// </summary>
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enable",
            typeof(bool),
            typeof(SyntaxBox),
            new FrameworkPropertyMetadata(
                false, 
                FrameworkPropertyMetadataOptions.AffectsRender, 
                new PropertyChangedCallback(OnEnableChanged)));
        /// <summary>
        /// Enable property set accessor
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetEnable(TextBox target, bool value)
        {
            target.SetValue(EnabledProperty, value);
        }
        /// <summary>
        /// Enable property get accessor.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool GetEnable(TextBox target)
        {
            return (bool)target.GetValue(EnabledProperty);
        }
        /// <summary>
        /// Called when Enabled changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        public static void OnEnableChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox target = (TextBox)sender;
            if (((bool)e.OldValue) == false && ((bool)e.NewValue) == true)
            {
                var brushConverter = new System.Windows.Media.BrushConverter();
                target.SetValue(OriginalForegroundProperty, target.Foreground);
                target.Template = _syntaxTemplate;
                target.Foreground = System.Windows.Media.Brushes.Transparent;
                
                TextBlock.SetLineStackingStrategy(target, LineStackingStrategy.MaxHeight);
            }
            else if (((bool)e.OldValue) == true && ((bool)e.NewValue) == false)
            {
                target.Template = _defaultTemplate;
                target.Foreground = (System.Windows.Media.Brush)target.GetValue(OriginalForegroundProperty);
            }
        }
        // ...................................................................
        #endregion

        #region ShowLineNumbers
        // ...................................................................
        /// <summary>
        /// Shows/hides the line numbers part.
        /// </summary>
        public static readonly DependencyProperty ShowLineNumbersProperty = DependencyProperty.RegisterAttached(
            "ShowLineNumbers",
            typeof(bool),
            typeof(SyntaxBox),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.AffectsRender));
        /// <summary>
        /// ShowLineNumbers property set accessor
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetShowLineNumbers(TextBox target, bool value)
        {
            target.SetValue(ShowLineNumbersProperty, value);
        }
        /// <summary>
        /// ShowLineNumbers property get accessor.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool GetShowLineNumbers(TextBox target)
        {
            return (bool)target.GetValue(ShowLineNumbersProperty);
        }
        // ...................................................................
        #endregion

        #region LineNumbersBackground
        // ...................................................................
        /// <summary>
        /// The background brush of the line numbers part.
        /// </summary>
        public static readonly DependencyProperty LineNumbersBackgroundProperty = DependencyProperty.RegisterAttached(
            "LineNumbersBackground",
            typeof(System.Windows.Media.Brush),
            typeof(SyntaxBox),
            new FrameworkPropertyMetadata(
                System.Windows.Media.Brushes.WhiteSmoke,
                FrameworkPropertyMetadataOptions.AffectsRender));
        public static void SetLineNumbersBackground(TextBox target, System.Windows.Media.Brush value)
        {
            target.SetValue(LineNumbersBackgroundProperty, value);
        }
        public static System.Windows.Media.Brush GetLineNumbersBackground(TextBox target)
        {
            return (System.Windows.Media.Brush)target.GetValue(LineNumbersBackgroundProperty);
        }
        // ...................................................................
        #endregion

        #region LineNumbersForeground
        // ...................................................................
        /// <summary>
        /// The foreground brush of the line numbers part.
        /// </summary>
        public static readonly DependencyProperty LineNumbersForegroundProperty = DependencyProperty.RegisterAttached(
            "LineNumbersForeground",
            typeof(System.Windows.Media.Brush),
            typeof(SyntaxBox),
            new FrameworkPropertyMetadata(
                System.Windows.Media.Brushes.SlateGray,
                FrameworkPropertyMetadataOptions.AffectsRender));
        public static void SetLineNumbersForeground(TextBox target, System.Windows.Media.Brush value)
        {
            target.SetValue(LineNumbersForegroundProperty, value);
        }
        public static System.Windows.Media.Brush GetLineNumbersForeground(TextBox target)
        {
            return (System.Windows.Media.Brush)target.GetValue(LineNumbersForegroundProperty);
        }
        // ...................................................................
        #endregion

        #region OriginalForeground
        // ...................................................................
        public static readonly DependencyProperty OriginalForegroundProperty = DependencyProperty.RegisterAttached(
            "OriginalForeground",
            typeof(System.Windows.Media.Brush),
            typeof(SyntaxBox),
            new FrameworkPropertyMetadata(System.Windows.Media.Brushes.Black));
        public static void OriginalForeground(TextBox target, System.Windows.Media.Brush value)
        {
            target.SetValue(OriginalForegroundProperty, value);
        }
        public static System.Windows.Media.Brush OriginalForeground(TextBox target)
        {
            return (System.Windows.Media.Brush)target.GetValue(OriginalForegroundProperty);
        }
        // ...................................................................
        #endregion

        #region SyntaxDriver
        // ...................................................................
        public static readonly DependencyProperty SyntaxDriverProperty = DependencyProperty.RegisterAttached(
            "SyntaxDriver",
            typeof(ISyntaxDriver),
            typeof(SyntaxBox),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));
        public static void SetSyntaxDriver(TextBox target, ISyntaxDriver value)
        {
            target.SetValue(SyntaxDriverProperty, value);
        }
        public static ISyntaxDriver GetSyntaxDriver(TextBox target)
        {
            return (ISyntaxDriver)target.GetValue(SyntaxDriverProperty);
        }
        // ...................................................................
        static readonly DependencyProperty SyntaxDriversProperty = DependencyProperty.RegisterAttached(
            // Name not matching the getter forces the actual getter top be used
            // and not optimized away.
            "SyntaxDrivers_",
            typeof(SyntaxDriverCollection),
            typeof(SyntaxBox));
        public static SyntaxDriverCollection GetSyntaxDrivers(TextBox Target)
        {
            var collection = Target.GetValue(SyntaxDriversProperty) as SyntaxDriverCollection;
            if (collection == null)
            {
                collection = new SyntaxDriverCollection();
                Target.SetValue(SyntaxDriversProperty, collection);
            }
            return (collection);
        }
        // ...................................................................
        #endregion

        #region ExpandTabs
        // ...................................................................
        /// <summary>
        /// Enables/disables syntax highlighting tab to space expansion
        /// </summary>
        public static readonly DependencyProperty ExpandTabsProperty = DependencyProperty.RegisterAttached(
            "ExpandTabs",
            typeof(bool),
            typeof(SyntaxBox),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));
        /// <summary>
        /// ExpandTabs property set accessor
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetExpandTabs(TextBox target, bool value)
        {
            target.SetValue(ExpandTabsProperty, value);
        }
        /// <summary>
        /// ExpandTabs property get accessor.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool GetExpandTabs(TextBox target)
        {
            return (bool)target.GetValue(ExpandTabsProperty);
        }
        // ...................................................................
        #endregion

        #region AutoIndent
        // ...................................................................
        /// <summary>
        /// Enables/disables auto-indentation.
        /// </summary>
        public static readonly DependencyProperty AutoIndentProperty = DependencyProperty.RegisterAttached(
            "AutoIndent",
            typeof(bool),
            typeof(SyntaxBox),
            new FrameworkPropertyMetadata(
                true,
                FrameworkPropertyMetadataOptions.None));
        /// <summary>
        /// AutoIndent property set accessor
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetAutoIndent(TextBox target, bool value)
        {
            target.SetValue(AutoIndentProperty, value);
        }
        /// <summary>
        /// AutoIndent property get accessor.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool GetAutoIndent(TextBox target)
        {
            return (bool)target.GetValue(AutoIndentProperty);
        }
        // ...................................................................
        #endregion

        #region IndentCount
        // ...................................................................
        /// <summary>
        /// Gets the number of space characters to indent on Tab expansion.
        /// </summary>
        public static readonly DependencyProperty IndentCountProperty = DependencyProperty.RegisterAttached(
            "IndentCount",
            typeof(int),
            typeof(SyntaxBox),
            new FrameworkPropertyMetadata(
                4,
                FrameworkPropertyMetadataOptions.None));
        /// <summary>
        /// IndentCount property set accessor
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public static void SetIndentCount(TextBox target, bool value)
        {
            target.SetValue(IndentCountProperty, value);
        }
        /// <summary>
        /// IndentCount property get accessor.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static int GetIndentCount(TextBox target)
        {
            return (int)target.GetValue(IndentCountProperty);
        }
        // ...................................................................
        #endregion

        #endregion

    }
}
