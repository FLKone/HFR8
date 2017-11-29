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

namespace HFR7.HFRClasses
{
    public class Mp
    {
        //&cat=prive&post=1705763&page=1&p=1&sondage=0&owntopic=0&trash=0&trash_post=0&print=0&numreponse=0&quote_only=0&new=0&nojs=0
        public string MpTitle { get; set; }
        public string MpId { get; set; }
        public string MpSender { get; set; }
        public string MpNumberOfPages { get; set; }
        public string MpUri { get; set; }
        public string MpLastPostText { get; set; }
        public string MpIsUnRead { get; set; }
        public double MpPage { get; set; }
        public double MpLastPostDateDouble { get; set; }
    }
}
