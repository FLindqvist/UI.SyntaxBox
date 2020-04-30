using System.Windows;

namespace UI.SyntaxBox.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool value = (bool)this.textBox.GetValue(SyntaxBox.EnabledProperty);

            this.textBox.SetValue(SyntaxBox.EnabledProperty, !value);
        }
    }
}
