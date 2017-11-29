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
    public class TopicFav
    {
        public string TopicNameFav { get; set; }
        public string TopicCatIdFav { get; set; }
        public string TopicCatNameFav { get; set; }
        public string TopicIdFav { get; set; }
        public string TopicUriFav { get; set; }
        public double TopicLastPostDateDouble { get; set; }
        public string TopicLastPost { get; set; }
        public string TopicGroup { get; set; }
        public string TopicIsHot { get; set; }
        public string TopicNumberOfPages { get; set; }
        public string TopicPageNumber { get; set; }
        public string TopicIsUnread { get; set; }
    }
}
