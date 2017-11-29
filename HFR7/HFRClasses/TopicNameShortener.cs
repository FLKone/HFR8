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
using System.Text.RegularExpressions;

namespace HFR7.HFRClasses
{
    public class TopicNameShortener
    {
        public static string Shorten(string input)
        {
            string output = Regex.Replace(input, "topic unique", "T.U.", RegexOptions.IgnoreCase);
            output = Regex.Replace(output, "topique unique", "T.U.", RegexOptions.IgnoreCase);
            output = Regex.Replace(output, "topic unik", "T.U.", RegexOptions.IgnoreCase);
            output = Regex.Replace(output, "topik unik", "T.U.", RegexOptions.IgnoreCase);
            output = Regex.Replace(output, "topik unique", "T.U.", RegexOptions.IgnoreCase);
            output = Regex.Replace(output, "topic officiel", "T.U.", RegexOptions.IgnoreCase);

            //if (output.Length > 29)
            //{
            //    output = output.Remove(29) + "...";
            //}

            return output;
        }
    }
}
