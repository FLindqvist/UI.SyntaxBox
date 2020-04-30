using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.SyntaxBox
{
    /// <summary>
    /// Collection used to wit attached property SyntaxDrivers.
    /// Allows multiple syntax drivers to be declared in XAML.
    /// </summary>
    public class SyntaxDriverCollection : List<ISyntaxDriver>
    {
    }
}
