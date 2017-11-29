using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using ImageTools.IO.Gif;
using HtmlAgilityPack;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.IO;

namespace HFR7
{
    public partial class SmileyPage : PhoneApplicationPage
    {
        List<HFRClasses.SmileyFixe> smileyFixeObject = new List<HFRClasses.SmileyFixe>();
        List<HFRClasses.SmileyAnimated> smileyAnimatedObject = new List<HFRClasses.SmileyAnimated>();
        System.IO.IsolatedStorage.IsolatedStorageSettings store = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
        string pageNumber;
        string idTopic;
        string idCat;
        string topicName;
        string hash;
        string answer;
        string action;
        string numberOfPages;
        string currentTheme;
        string currentOrientation;
        string position;
        string numRep;
        string mpId;
        string senderPseudo;
        IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        XDocument docSmiley;
        string keyword;

        public SmileyPage()
        {
            InitializeComponent();
            ImageTools.IO.Decoders.AddDecoder<GifDecoder>();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if ((string)store["disableLandscape"] == "true") WikiSmileyPage.SupportedOrientations = SupportedPageOrientation.Portrait;

            // Background clair ou foncé
            if ((Visibility)Resources["PhoneDarkThemeVisibility"] == System.Windows.Visibility.Visible) currentTheme = "dark";
            else currentTheme = "light";
            //if (WikiSmileyPage.Orientation == PageOrientation.Landscape || WikiSmileyPage.Orientation == PageOrientation.LandscapeLeft || WikiSmileyPage.Orientation == PageOrientation.LandscapeRight) currentOrientation = "landscape";
            //else currentOrientation = "portrait";
            //backgroundImageBrush.ImageSource = new BitmapImage(new Uri("Images/" + currentTheme + "/Background/Background_" + currentOrientation + ".jpg", UriKind.Relative));
            
            // GET
            NavigationContext.QueryString.TryGetValue("pagenumber", out pageNumber);
            NavigationContext.QueryString.TryGetValue("idtopic", out idTopic);
            NavigationContext.QueryString.TryGetValue("idcat", out idCat);
            NavigationContext.QueryString.TryGetValue("topicname", out topicName);
            NavigationContext.QueryString.TryGetValue("action", out action);
            NavigationContext.QueryString.TryGetValue("numberofpages", out numberOfPages);
            NavigationContext.QueryString.TryGetValue("position", out position);
            NavigationContext.QueryString.TryGetValue("numrep", out numRep);
            NavigationContext.QueryString.TryGetValue("mpid", out mpId);
            NavigationContext.QueryString.TryGetValue("senderpseudo", out senderPseudo);

            keywordTextBox.Focus();
        
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            // Cachage du WB
            smileyWB.Visibility = System.Windows.Visibility.Collapsed;
            // Keyword
            keyword = keywordTextBox.Text;
            if (keywordTextBox.Text == "") MessageBox.Show("Veuillez entrer un mot clé.");
            else if (keyword.Length < 3)
            {
                MessageBox.Show("Le mot clé doit faire au moins 3 caractères.");
            }
            else
            {
                // Création du pattern de la page
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("smiley-" + HttpUtility.UrlEncode(keyword) + ".html", FileMode.Create, isoStore))
                {
                    XDocument docModel = XDocument.Load("SmileyHTMLfile_" + currentTheme + ".html");
                    docModel.Save(isoStream);
                }
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("smiley-" + HttpUtility.UrlEncode(keyword) + ".html", FileMode.Open, isoStore))
                {
                    docSmiley = XDocument.Load(isoStream);
                }

                globalProgressBar.Visibility = System.Windows.Visibility.Visible;
                // Lancement de la recherche
                HtmlWeb.LoadAsync("https://forum.hardware.fr/wikismilies.php?config=hfr.inc&threecol=0", null, new string[] { "findcode=", "findkeyword=" + keyword }, (s, args) =>
                    {
                        string[] smileyUrl = args.Document.DocumentNode.Descendants("img").Where(x => (bool)x.GetAttributeValue("title", "").Contains("[:") == true).
                                                    Select(y => y.GetAttributeValue("src", "")).ToArray();
                        string[] smileyCode = args.Document.DocumentNode.Descendants("img").Where(x => (bool)x.GetAttributeValue("title", "").Contains("[:") == true).
                                                    Select(y => y.GetAttributeValue("title", "")).ToArray();
                        int i = 0;
                        if (smileyUrl.Length == 0)
                        {
                            XElement result = new XElement("font", new XAttribute("class", "fontClass"), "Aucun smiley n'a été trouvé.");
                            docSmiley.Root.Element("body").Add(result);
                        }
                        foreach (var smile in smileyUrl)
                        {
                            if (i < 200)
                            {
                                XElement smileyImg = new XElement("a", new XAttribute("class", "link"), new XAttribute("href", smileyCode[i]), new XElement("img", new XAttribute("src", smile)));
                                docSmiley.Root.Element("body").Add(smileyImg);
                                i++;
                            }
                        }

                        using (IsolatedStorageFileStream isoStreamSave = new IsolatedStorageFileStream("smiley-" + HttpUtility.UrlEncode(keyword) + ".html", FileMode.Create, isoStore))
                        {
                            docSmiley.Save(isoStreamSave);
                        }
                        Dispatcher.BeginInvoke(() =>
                    {
                        smileyWB.Navigate(new Uri("smiley-" + HttpUtility.UrlEncode(keyword) + ".html", UriKind.Relative));
                    });
                    });
            }
        }

        private void smileyWB_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            smileyWB.Visibility = System.Windows.Visibility.Visible;
            globalProgressBar.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void smileyWB_Navigating(object sender, NavigatingEventArgs e)
        {
            if (!e.Uri.ToString().Contains("html") && idTopic != null)
            {
                e.Cancel = true;
                NavigationService.Navigate(new Uri("/AnswerTopic.xaml?idtopic=" + idTopic + "&idcat=" + idCat + "&pagenumber=" + pageNumber + "&topicname=" + topicName + "&action=" + action + "&numberofpages=" + numberOfPages + "&smileyCode=" + e.Uri.ToString() + "&position=" + position + "&numrep=" + numRep + "&back=tool", UriKind.Relative));
            }
            else if (!e.Uri.ToString().Contains("html") && mpId != null)
            {
                e.Cancel = true;
                NavigationService.Navigate(new Uri("/Mp.xaml?senderpseudo=" + senderPseudo + "&mpid=" + mpId + "&numberofpages=" + numberOfPages + "&sujet=" + topicName + "&smileyCode=" + e.Uri.ToString().Split('#')[1] + "&position=" + position + "&back=smiley", UriKind.Relative));
            }
        }

        private void WikiSmileyPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight)
            {
                smileyWB.Height = 260;
                smileyWB.Width = 740;
            }
            else
            {
                smileyWB.Height = 530;
                smileyWB.Width = 460;
            }
        }

        private void smileyButton_Click(object sender, RoutedEventArgs e)
        {
            Button clicked = (Button)sender;
            //NavigationService.Navigate(new Uri("/AnswerTopic.xaml?idtopic=" + idTopic + "&idcat=" + idCat + "&pagenumber=" + pageNumber + "&topicname=" + topicName + "&action=" + action + "&numberofpages=" + numberOfPages + "&smileyCode=" + clicked.Content + "&position=" + position + "&numrep=" + numRep + "&back=tool", UriKind.Relative));
            if (idTopic != null)
            {
                NavigationService.Navigate(new Uri("/AnswerTopic.xaml?idtopic=" + idTopic + "&idcat=" + idCat + "&pagenumber=" + pageNumber + "&topicname=" + topicName + "&action=" + action + "&numberofpages=" + numberOfPages + "&smileyCode=" + clicked.Content + "&position=" + position + "&numrep=" + numRep + "&back=tool", UriKind.Relative));
            }
            else if (mpId != null)
            {
                NavigationService.Navigate(new Uri("/Mp.xaml?senderpseudo=" + senderPseudo + "&mpid=" + mpId + "&numberofpages=" + numberOfPages + "&sujet=" + topicName + "&smileyCode=" + clicked.Content + "&position=" + position + "&back=smiley", UriKind.Relative));
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            store.Add("navigatedBack", "true");
        }
    }
}