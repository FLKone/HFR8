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
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Linq;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Phone.Tasks;
using System.Threading;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Shell;
using System.Xml;
using Microsoft.Phone.Net.NetworkInformation;
using System.Windows.Navigation;

namespace HFR7
{
    public partial class SearchTopic : PhoneApplicationPage
    {
        # region Déclarations

        IsolatedStorageSettings store = IsolatedStorageSettings.ApplicationSettings;
        string idCat;
        string idTopic;
        string topicName;
        string pageNumber;
        string jump;
        string numberOfPages;
        string from;
        bool navigated;
        string accentColor;
        string editedColor;
        string quotedColor;
        string lastReponseId;
        string backAction;
        bool invoked = false;
        bool isWifi = false;
        bool fromCache = false;
        bool navQuote = false;
        XDocument docTopic = new XDocument();
        string content = "";
        bool navigationStop = true;
        IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        CookieContainer container = new CookieContainer();
        CookieContainer containerDummy = new CookieContainer();
        string userPseudo;
        string currentTheme;
        string foregroundColor;
        string backgroundColor;
        int fontSizeValue;
        int fontSizeValuePseudo;
        int fontSizeValueDateHeure;
        string subtleColor;
        string moresubtleColor;
        string linkColor;
        string currentOrientation;
        string pinchToZoomSetting;
        string searchPseudo;
        string searchWord;

        #endregion

        public SearchTopic()
        {
            InitializeComponent();
        }

        private void SearchTopicPA_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();

            if ((string)store["disableLandscape"] == "true") SearchTopicPA.SupportedOrientations = SupportedPageOrientation.Portrait;
            // Vérification de l'état de navigation du webbrowser
            try
            {
                if (readTopicWebBrowser.Source.ToString().Contains("topic"))
                {
                    navigated = true;
                }
            }
            catch
            {
                navigated = false;
            }
            // Vient-on du bouton back et le topic est-il bien chargé ?
            if (store.Contains("navigatedback") && navigated)
            {
                store.Remove("navigatedback");
            }
            else
            {
                // Affichage de la ProgressBar
                SystemTray.ProgressIndicator.IsVisible = true;
                SystemTray.ProgressIndicator.IsIndeterminate = true;
                SystemTray.ProgressIndicator.Text = "Chargement des résultats...";

                // Thème
                if ((Visibility)Resources["PhoneDarkThemeVisibility"] == Visibility.Visible) currentTheme = "dark";
                else currentTheme = "light";

                // Couleur d'accent
                accentColor = HFRClasses.ColorConvert.ConvertToHtml(((Color)Application.Current.Resources["PhoneAccentColor"]).ToString());

                // Couleur subtile
                if (currentTheme == "dark")
                {
                    foregroundColor = "#FFFFFF";
                    backgroundColor = "#000000";
                    editedColor = "#d3d3d3";
                    quotedColor = "#e5e5e5";
                    subtleColor = "#303030";
                    moresubtleColor = "#202020";
                    linkColor = "#cbd1ff";
                }
                else
                {
                    foregroundColor = "#000000";
                    backgroundColor = "#FFFFFF";
                    subtleColor = "#eaeaea";
                    editedColor = "#303030";
                    quotedColor = "#6d6d6d";
                    moresubtleColor = "#f2f2f2";
                    linkColor = "#034993";
                }

                // Orientation
                if (SearchTopicPA.Orientation == PageOrientation.Portrait) currentOrientation = "portrait";
                else currentOrientation = "landscape";

                // Font size
                fontSizeValue = (int)store["fontSizeValue"];
                fontSizeValuePseudo = fontSizeValue + 9;
                fontSizeValueDateHeure = fontSizeValue - 2;

                // Pinch To Zoom
                if ((string)store["pinchToZoomOption"] == "true") pinchToZoomSetting = "yes";
                else pinchToZoomSetting = "no";


                RecuperationInformations();
            }
        }

        private void RecuperationInformations()
        {
            // Récupération du cookie du store
            container = store["HFRcookies"] as CookieContainer;
            // Récupération du cookie dummy
            containerDummy = store["HFRcookiesDummy"] as CookieContainer;

            if (container.Count < 3)
            {
                List<Cookie> listCookies = store["listHFRcookies"] as List<Cookie>;
                foreach (Cookie c in listCookies)
                {
                    container.Add(new Uri("https://forum.hardware.fr", UriKind.Absolute), c);
                }
                store.Remove("HFRcookies");
                store.Add("HFRcookies", container);
            }

            if (containerDummy.Count < 3)
            {
                List<Cookie> listCookiesDummy = store["listHFRcookiesDummy"] as List<Cookie>;
                foreach (Cookie c in listCookiesDummy)
                {
                    containerDummy.Add(new Uri("https://forum.hardware.fr", UriKind.Absolute), c);
                }
                store.Remove("HFRcookiesDummy");
                store.Add("HFRcookiesDummy", containerDummy);
            }

            // Récupération du pseudo
            userPseudo = store["userPseudo"] as string;

            // Récupération de l'ID de la catégorie
            NavigationContext.QueryString.TryGetValue("idcat", out idCat);

            // Récupération de l'ID du topic
            NavigationContext.QueryString.TryGetValue("idtopic", out idTopic);

            // Récupération du pseudo de recherche
            NavigationContext.QueryString.TryGetValue("spseudo", out searchPseudo);

            // Récupération du mot de recherche
            NavigationContext.QueryString.TryGetValue("word", out searchWord);

            // Récupération du nom du topic
            NavigationContext.QueryString.TryGetValue("topicname", out topicName);
            topicName = HttpUtility.HtmlDecode(topicName);
            
            // Titres
            topicNameTextBlock.Text = topicName.ToUpper();
            if (searchPseudo != "" && searchWord != "") pagesTextBlock.Text = "recherche de \"" + searchWord + "\" par " + searchPseudo + "...";
            else if (searchPseudo != "") pagesTextBlock.Text = "recherche des posts de " + searchPseudo + "...";
            else if (searchWord != "") pagesTextBlock.Text = "recherche de \"" + searchWord + "\"...";
            enterLoadingTextBlock.Begin();

            // Récupération de l'état du wifi
            if (DeviceNetworkInformation.IsWiFiEnabled) isWifi = true;

            // Création du WebBrowser
            NavigateToTopic();
        }

        private void NavigateToTopic()
        {
            DownloadTopic(idCat, idTopic, pageNumber, container, true, true);
        }

        private void DownloadTopic(string idCatParse, string idTopicParse, string pageNumberParse, CookieContainer containerParse, bool navigateWhenFinished, bool saveTopic)
        {
            // Création du pattern de la page
            XDocument docTopic = XDocument.Load("HTMLfile.html");

            // Création d'un nombre random pour éviter la mise en cache du WebRequest (bug du contrôle)
            int randomNumber = new Random().Next(1, 100000);

            //forum.hardware.fr/forum2.php?post=1197&cat=25&spseudo=Poogz&filter=1

            // Construction de URL du topic
            string urlTopic = "https://forum.hardware.fr/forum2.php?config=hfr.inc&cat=" + idCatParse + "&post=" + idTopicParse + "&spseudo=" + searchPseudo + "&word=" + searchWord + "&filter=1&random=" + randomNumber.ToString();
            //string urlTopic = "http://www.scrubs-fr.net/perso/modopb.html";
            // Récupération et parsage de l'HTML du topic sur HFR
            HtmlWeb.LoadAsync(urlTopic, containerParse, (s, args) =>
            {
                if (args.Error != null)
                {
                    if (args.Error.ToString().Contains("NotFound"))
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("Le serveur distant ne répond pas. Vérifiez votre connectivité ou l'état du serveur distant.");
                            try { SystemTray.ProgressIndicator.IsVisible = false; }
                            catch { }
                        });
                    }
                }
                else if (saveTopic)
                {
                    int i = 0;
                    args.Document.OptionWriteEmptyNodes = true;
                    string[] meta = args.Document.DocumentNode.Descendants("meta").Where(x => (string)x.GetAttributeValue("name", "") == "Description").
                                                Select(y => y.GetAttributeValue("content", "")).ToArray();

                    string[] messageLink = args.Document.DocumentNode.Descendants("a").Where(x => (bool)x.InnerHtml.Contains("<b>Voir ce message dans le sujet non filtré</b>") == true).
                             Select(y => y.GetAttributeValue("href", "")).ToArray();

                    string[] topicText = args.Document.DocumentNode.Descendants("div").Where(x => (bool)x.GetAttributeValue("id", "").Contains("para") == true).
                                                Select(y => y.InnerHtml).ToArray();

                    string[] messCase1 = args.Document.DocumentNode.Descendants("td")
                        .Where(x => (bool)x.GetAttributeValue("class", "").Contains("messCase1") == true && (bool)x.InnerHtml.Contains("<div><b class=\"s2\">Publicité</b></div>") == false && (bool)x.InnerHtml.Contains("Auteur") == false)
                        .Select(x => x.InnerHtml).ToArray();

                    string[] messCase2 = args.Document.DocumentNode.Descendants("td")
                        .Where(x => (bool)x.GetAttributeValue("class", "").Contains("messCase2") == true && (bool)x.InnerHtml.Contains("<div><b class=\"s2\">Publicité</b></div>") == false)
                        .Select(x => x.InnerText).ToArray();

                    string[] toolbar = args.Document.DocumentNode.Descendants("div")
                        .Where(x => (string)x.GetAttributeValue("class", "") == "toolbar")
                        .Select(x => x.InnerHtml).ToArray();

                    // Si pas de résultats...
                    if (topicText.Length == 0) Dispatcher.BeginInvoke(() => 
                        {
                            MessageBox.Show("La recherche n'a donné aucun résultat.");
                            NavigationService.GoBack();
                        });

                    // Construction du HTML pour l'affichage dans le WebBrowser
                    foreach (string line in topicText)
                    {
                        string avatarUri = "";
                        string posterPseudo;
                        string reponseId;
                        string dateHeure;
                        // Affichage des avatars
                        if (messCase1[i].Contains("avatar_center") && ((string)store["displayAvatars"] == "always" || ((string)store["displayAvatars"] == "wifi" && isWifi)))
                        {
                            int firstAvatar = messCase1[i].IndexOf("<div class=\"avatar_center\" style=\"clear:both\"><img src=\"") + "<div class=\"avatar_center\" style=\"clear:both\"><img src=\"".Length;
                            int lastAvatar = messCase1[i].LastIndexOf("\" alt=\"");
                            avatarUri = messCase1[i].Substring(firstAvatar, lastAvatar - firstAvatar);
                        }
                        else if ((string)store["displayAvatars"] == "always" || ((string)store["displayAvatars"] == "wifi" && isWifi))
                        {
                            avatarUri = "http://reho.st/http://self/pic/f41fdbf4dc93b9fc7cc290349fa3b17707671544.png";
                        }

                        // Date et heure
                        int firstDate = toolbar[i].IndexOf("Posté le ") + "Posté le ".Length; ;
                        int lastDate = 31;
                        dateHeure = Regex.Replace(toolbar[i].Substring(firstDate, lastDate), "&nbsp;", " ");

                        // Pseudo
                        int firstPseudo = messCase1[i].IndexOf("<b class=\"s2\">") + "<b class=\"s2\">".Length;
                        int lastPseudo = messCase1[i].LastIndexOf("</b>");
                        posterPseudo = messCase1[i].Substring(firstPseudo, lastPseudo - firstPseudo);
                        posterPseudo = posterPseudo.Replace(((char)8203).ToString(), ""); // char seperator (jocebug)

                        // Id de la réponse
                        int firstReponseId = messCase1[i].IndexOf("title=\"n°") + "title=\"n°".Length;
                        int lastlastReponseId = messCase1[i].LastIndexOf("\" alt=\"n°");
                        reponseId = messCase1[i].Substring(firstReponseId, lastlastReponseId - firstReponseId);

                        // Dernier reponseId
                        if (i == topicText.Length - 1)
                        {
                            lastReponseId = reponseId;
                        }

                        // Mise en forme du texte
                        int lastPostText = topicText[i].IndexOf("<span class=\"signature\">");
                        if (lastPostText == -1)
                        {
                            lastPostText = topicText[i].Length;
                        }
                        string postText = topicText[i].Substring(0, lastPostText);
                        postText = Regex.Replace(HttpUtility.HtmlDecode(postText), " target=\"_blank\"", "");

                        // Affichage des images ?
                        if ((string)store["displayImages"] == "never" || (string)store["displayImages"] == "wifi" && !isWifi)
                        {
                            // Très sale mais fonctionne : vérification si l'image est un smiley ou non
                            postText = Regex.Replace(postText, "<img src=\"http://forum-images.hardware.fr/", "<save=\"http://forum-images.hardware.fr/");
                            postText = Regex.Replace(postText, "<img src=\"", "<a class=\"cLink\" href=\"§");
                            postText = Regex.Replace(postText, "<save=\"http://forum-images.hardware.fr/", "<img src=\"http://forum-images.hardware.fr/");
                            postText = Regex.Replace(postText, "style=\"margin: 5px\" />", ">Image</a>");
                        }

                        
                        // Parsage du post
                        XElement post;
                        post = new XElement("table", new XAttribute("class", "tableGlobal"),
                                            new XElement("tr", new XAttribute("class", "enteteTr"),
                                                new XElement("td", new XAttribute("class", "enteteTd"),
                                                    new XElement("span", new XAttribute("class", "spanImageAvatar"),
                            //Avatar
                                                    new XElement("img", new XAttribute("src", avatarUri), new XAttribute("class", "imageAvatar"))
                                                    ),
                                                    new XElement("span", new XAttribute("class", "spanPseudoDateHeure"),
                                                        new XElement("div", new XAttribute("class", "divPseudo"), posterPseudo), new XElement("div", new XAttribute("class", "divDateHeure"), dateHeure)),


                            new XElement("span", new XAttribute("class", "spanActionButton"),
                            // Jump Url
                                                                new XElement("a", new XAttribute("name", "rep" + reponseId)),
                                new XAttribute("onClick", "ShowMenu('rep" + reponseId + "');")))),
                                new XElement("tr",
                                                new XElement("td", new XAttribute("colspan", "3"),
                            // Post
                                                    new XElement("div", new XAttribute("class", "divPost"), postText)
                                                    )
                                                )
                                            );
                        docTopic.Root.Element("body").Add(post);
                        i++;
                    }

                    // Ouverture du flux

                    // Modification du HTML
                    try
                    {
                        content = docTopic.ToString();
                        content = Regex.Replace(content, "&lt;", "<");
                        content = Regex.Replace(content, "&gt;", ">");
                        content = Regex.Replace(content, "</b><br /><br /><p>", "</b><p>");
                        content = Regex.Replace(content, "<br /> <br /> <br />", "<br />");
                        content = Regex.Replace(content, "#REPLACETOPICNAME", topicName.ToUpper());
                        content = Regex.Replace(content, "#REPLACEPAGES", "résultats de la recherche");
                        content = Regex.Replace(content, "#FOREGROUNDCOLOR", foregroundColor);
                        content = Regex.Replace(content, "#BACKGROUNDCOLOR", backgroundColor);
                        content = Regex.Replace(content, "#FONTSIZECONTENT", fontSizeValue + "px");
                        content = Regex.Replace(content, "#FONTSIZEPSEUDO", fontSizeValuePseudo + "px");
                        content = Regex.Replace(content, "#FONTSIZEDATEHEURE", fontSizeValueDateHeure + "px");
                        content = Regex.Replace(content, "#ACCENTCOLOR", accentColor);
                        content = Regex.Replace(content, "#SUBTLECOLOR", subtleColor);
                        content = Regex.Replace(content, "#EDITEDCOLOR", editedColor);
                        content = Regex.Replace(content, "#QUOTEDCOLOR", quotedColor);
                        content = Regex.Replace(content, "#MORESUBTLECOLOR", moresubtleColor);
                        content = Regex.Replace(content, "#LINKCOLOR", linkColor);
                        content = Regex.Replace(content, "#REPLACETOPICNAME", topicName.ToUpper());
                        content = Regex.Replace(content, "#REPLACEPAGES", "page " + pageNumberParse + "/" + numberOfPages);
                        content = Regex.Replace(content, "#PINCHTOZOOMOPTION", pinchToZoomSetting);

                        content = Regex.Replace(content, "<div class=\"edited\"><a href=", "<div class=\"edited\"><div href=");
                        content = Regex.Replace(content, "fois</a></div>", "fois</div></div>");

                        content = Regex.Replace(content, "a écrit :</a></b>", "a écrit (voir) :</a></b>");

                        content = Regex.Replace(content, "Spoiler :</b><br /><br />", "Spoiler :</b><br />");
                        content = Regex.Replace(content, "Spoiler :</b><br /><br />", "Spoiler :</b><br />");

                        content = Regex.Replace(content, "<div class=\"edited\"><br />Message édité par", "<div class=\"edited\"><br />Édité par");
                        content = Regex.Replace(content, "fois</a><br />Message édité", "fois</a><br />Édité par");


                        content = Regex.Replace(content, "<br /> <br />", "<p class=\"doubleP\" />");
                        content = Regex.Replace(content, "<br /><br />", "<p class=\"doubleP\" />");
                        //content = Regex.Replace(content, "class=\"spoiler\" onclick=\"javascript:swap_spoiler_states(this)\"", "");
                        content = Regex.Replace(content, "<b class=\"s1Topic\">Spoiler :</b>", "<b class=\"s1Topic\">Spoiler (touchez ici)</b>");
                        content = Regex.Replace(content, "<p class=\"doubleP\" /></p><div class=\"container\">", "<div class=\"container\">");
                        content = Regex.Replace(content, "class=\"Topic masque\"", "");



                    }
                    catch
                    {
                        MessageBox.Show("Erreur dans la mise en forme du HTML");
                    }

                    // Ecriture du HTML dans le fichier
                    using (var file = isoStore.OpenFile("topics/topic-" + idTopicParse + "-" + pageNumberParse + ".html", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                    {
                        using (var writer = new StreamWriter(file))
                        {
                            writer.Write(content);
                        }
                    }

                    // Navigation vers ce fichier
                    if (navigateWhenFinished)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            navigationStop = false;
                            NaviguerWebBrowserTopic();
                        });
                    }
                }
            });
        }

        private void NaviguerWebBrowserTopic()
        {
            if (isoStore.FileExists("topics/topic-" + idTopic + "-" + pageNumber + ".html") && !navigationStop)
            {
                try
                {
                    readTopicWebBrowser.Navigate(new Uri("topics/topic-" + idTopic + "-" + pageNumber + ".html", UriKind.Relative));
                    //invoked = false;
                    //readTopicWebBrowser.Navigated += new EventHandler<System.Windows.Navigation.NavigationEventArgs>(readTopicWebBrowser_Navigated);
                }
                catch
                {
                    MessageBox.Show("Erreur dans le cache.");
                }
            }
        }

        void readTopicWebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.ToString().Contains("topic"))
            {
                if (!e.Uri.ToString().Contains("rep") && !e.Uri.ToString().Contains("bas"))
                {
                    GoReponse();
                }

                // Si l'URI contient rep c'est qu'on a sauté dans le topic
                else if (navQuote != true)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        Thread.Sleep(2000);
                        enterSB.Begin();
                        quitLoadingTextBlock.Begin();
                        try { SystemTray.ProgressIndicator.IsVisible = false; }
                        catch { }
                    });
                }
            }
        }

        void GoReponse()
        {
            enterSB.Begin();
            quitLoadingTextBlock.Begin();
            try { SystemTray.ProgressIndicator.IsVisible = false; }
            catch { }
        }


        // Interception navigation hors application
        void readTopicWebBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            string navUri = e.Uri.ToString();
            if (navUri.Contains("ReadTopic.xaml"))
            {
                e.Cancel = true;
                string lol = e.Uri.ToString();
                NavigationService.Navigate(new Uri(e.Uri.ToString(), UriKind.Relative));
            }

            else if (navUri.Contains("AnswerTopic.xaml"))
            {
                e.Cancel = true;
                NavigationService.Navigate(new Uri(e.Uri.ToString(), UriKind.Relative));
            }

            else if (navUri.Contains("§"))
            {
                e.Cancel = true;
                internWebBrowser.Source = new Uri(e.Uri.ToString().Split('§')[1].ToString());
                TopicPanel.Opacity = 0.2;
                internWebBrowserCanvas.Visibility = Visibility.Visible;
                SystemTray.ProgressIndicator.IsVisible = true;
                SystemTray.ProgressIndicator.Text = "Chargement...";
            }
            else if (navUri.Contains("forum2.php?config=hfr.inc"))
            {
                e.Cancel = true;
            }
            else if (!navUri.Contains("topics/topic-" + idTopic + "-" + pageNumber + ".html"))
            {
                e.Cancel = true;
                WebBrowserTask task = new WebBrowserTask();
                task.URL = HttpUtility.UrlEncode(e.Uri.ToString());
                task.Show();
            }
        }

        private string GetUri(string get, string uri)
        {
            string toSplit = uri.Substring(uri.IndexOf(get + "=") + (get + "=").Length, uri.Length - uri.IndexOf(get + "=") - (get + "=").Length);
            return toSplit.Split('&')[0];
        }

        // Interception bouton back
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (internWebBrowserCanvas.Visibility == Visibility.Collapsed)
            {
                //if (souscatUri == null || souscatUri == "") Dispatcher.BeginInvoke(() => { 
                //    NavigationService.Navigate(new Uri("/WelcomePage.xaml?pivot=1", UriKind.Relative)); 
                //    
                //});
                //else if (from == "listdrapcat")
                //{
                //    NavigationService.Navigate(new Uri("/ListTopics.xaml?souscaturi=" + souscatUri + "&idcat=" + idCat + "&souscatname=" + souscatName + "&pivot=drap&from=readtopic", UriKind.Relative));
                //}
                //else if (from == "listtopicscat")
                //{
                //    NavigationService.Navigate(new Uri("/ListTopics.xaml?souscaturi=" + souscatUri + "&idcat=" + idCat + "&souscatname=" + souscatName + "&pivot=topics&from=readtopic", UriKind.Relative));
                //}
                //else Dispatcher.BeginInvoke(() =>
                //{
                //    NavigationService.Navigate(new Uri("/ListTopics.xaml?souscaturi=" + souscatUri + "&idcat=" + idCat + "&souscatname=" + souscatName + "&from=readtopic", UriKind.Relative));
                //});
                //quitSB.Begin();
                e.Cancel = true;
                if (backAction == "fftt")
                {
                    BackwardOutTransition();
                    Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/WelcomePage.xaml?pivot=1", UriKind.Relative)));
                }
                else
                {
                    store.Remove("navigatedback");
                    store.Add("navigatedback", "true");
                    BackwardOutTransition();
                    NavigationService.GoBack();
                }
            }
            if (internWebBrowserCanvas.Visibility == Visibility.Visible)
            {
                e.Cancel = true;
                try { SystemTray.ProgressIndicator.IsVisible = false; }
                catch { }
                internWebBrowserCanvas.Visibility = Visibility.Collapsed;
                internWebBrowser.Visibility = Visibility.Collapsed;
                TopicPanel.Opacity = 1;
            }
        }


        private void internWebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            internWebBrowser.Visibility = Visibility.Visible;
            try { SystemTray.ProgressIndicator.IsVisible = false; }
            catch { }
        }

        private void BackwardOutTransition()
        {
            TransitionElement transitionElement = new TurnstileTransition { Mode = TurnstileTransitionMode.BackwardOut };
            ITransition transition = transitionElement.GetTransition((PhoneApplicationPage)(((PhoneApplicationFrame)Application.Current.RootVisual)).Content);
            transition.Completed += delegate
            {
                transition.Stop();
            };
            transition.Begin();
        }

        private void SearchTopicPA_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight)
            {
                readTopicWebBrowser.Height = 480;
                readTopicWebBrowser.Width = 730;
                SystemTray.IsVisible = false;
                currentOrientation = "landscape";
            }
            else
            {
                readTopicWebBrowser.Height = 730;
                readTopicWebBrowser.Width = 480;
                currentOrientation = "portrait";
                SystemTray.IsVisible = true;
            }
        }

        private void ApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            if (e.IsMenuVisible == true)
            {
                if (SearchTopicPA.Orientation.ToString().Contains("Portrait"))
                {
                    enterLoadingTextBlock.Begin();
                }
                else
                {
                    SystemTray.IsVisible = true;
                }
            }
            if (e.IsMenuVisible == false)
            {
                if (SearchTopicPA.Orientation.ToString().Contains("Portrait"))
                {
                    quitLoadingTextBlock.Begin();
                }
                else
                {
                    SystemTray.IsVisible = false;
                }
            }
        }

        private void TextBlock_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {

        }
    }
}