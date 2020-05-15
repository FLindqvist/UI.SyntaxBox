using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace UI.SyntaxBox.Demo
{
    public static class Commands
    {
        public static ICommand CommentCommand = new RoutedCommand();
        public static ICommand UncommentCommand = new RoutedCommand();
    }
}
