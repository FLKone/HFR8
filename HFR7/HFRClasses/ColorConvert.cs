using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;

namespace HFR7.HFRClasses
{
    public class ColorConvert 
    {
        public static string ConvertToHtml(string input)
        {
            return "#" + input.Substring(3, 6);
        }
    }
}
