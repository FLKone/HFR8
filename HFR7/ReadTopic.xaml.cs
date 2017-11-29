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
using HFR7.HFRClasses;
using System.Xml.Serialization;

namespace HFR7
{
    public partial class ReadTopic : PhoneApplicationPage
    {
        # region Déclarations

        IsolatedStorageSettings store = IsolatedStorageSettings.ApplicationSettings;
        List<String> navQuoteBackStack;
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
        bool loadingTextBlockDisplayed = false;
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
        List<HFRClasses.NotifTopics> notifTopicList = new List<NotifTopics>();
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
        bool searchIsOn = false;

        #endregion

        public ReadTopic()
        {
            InitializeComponent();
        }

        private void ReadTopicPA_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationBar.Mode = ApplicationBarMode.Default;
            SystemTray.ProgressIndicator = new ProgressIndicator();
            
            if ((string)store["disableLandscape"] == "true") ReadTopicPA.SupportedOrientations = SupportedPageOrientation.Portrait;
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
            if (store.Contains("quotedText"))
            {
                store.Remove("quotedText");
            }
            if (store.Contains("navigatedback") && navigated)
            {
                store.Remove("navigatedback");
            }
            else
            {

                // Affichage de la ProgressBar
                SystemTray.ProgressIndicator.IsVisible = true;
                SystemTray.ProgressIndicator.IsIndeterminate = true;
                SystemTray.ProgressIndicator.Text = "Chargement du sujet...";

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

                // Font size
                fontSizeValue = (int)store["fontSizeValue"];
                fontSizeValuePseudo = fontSizeValue + 9;
                fontSizeValueDateHeure = fontSizeValue - 2;

                // Orientation
                if (ReadTopicPA.Orientation.ToString().Contains("Portrait")) currentOrientation = "portrait";
                else currentOrientation = "landscape";

                // Pinch To Zoom
                if ((string)store["pinchToZoomOption"] == "true") pinchToZoomSetting = "yes";
                else pinchToZoomSetting = "no";

                // Nettoyage backstack
                NavigationContext.QueryString.TryGetValue("back", out backAction);
                if (backAction == "changepage" || backAction == "fftt")
                {
                    NavigationService.RemoveBackEntry();
                }
                if (backAction == "answer" || backAction == "tool")
                {
                    NavigationService.RemoveBackEntry();
                    NavigationService.RemoveBackEntry();
                }
                store.Remove("navigatedback");
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
                    container.Add(new Uri("http://forum.hardware.fr", UriKind.Absolute), c);
                }
                store.Remove("HFRcookies");
                store.Add("HFRcookies", container);
            }

            if (containerDummy.Count < 3)
            {
                List<Cookie> listCookiesDummy = store["listHFRcookiesDummy"] as List<Cookie>;
                foreach (Cookie c in listCookiesDummy)
                {
                    containerDummy.Add(new Uri("http://forum.hardware.fr", UriKind.Absolute), c);
                }
                store.Remove("HFRcookiesDummy");
                store.Add("HFRcookiesDummy", containerDummy);
            }

            // Récupération du pseudo
            userPseudo = store["userPseudo"] as string;

            // Récupération du numéro de la page
            NavigationContext.QueryString.TryGetValue("pagenumber", out pageNumber);

            // Récupération de l'anchor
            NavigationContext.QueryString.TryGetValue("jump", out jump);
            if (jump == null) jump = "";

            // Récupération du nombre de pages
            NavigationContext.QueryString.TryGetValue("numberofpages", out numberOfPages);

            // Récupération de l'ID de la catégorie
            NavigationContext.QueryString.TryGetValue("idcat", out idCat);

            // Récupération de l'ID du topic
            NavigationContext.QueryString.TryGetValue("idtopic", out idTopic);

            // Récupération du nom du topic
            NavigationContext.QueryString.TryGetValue("topicname", out topicName);
            topicName = HttpUtility.UrlDecode(topicName);

            //// Vérification si abonné au topic
            //XmlSerializer serializer = new XmlSerializer(notifTopicList.GetType());
            //if (isoStore.FileExists("notifications.xml"))
            //{
            //    using (var file = isoStore.OpenFile("notifications.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            //    {
            //        using (var reader = new StreamReader(file))
            //        {
            //            try
            //            {
            //                object deserialized = serializer.Deserialize(reader.BaseStream);
            //                notifTopicList = (List<HFRClasses.NotifTopics>)deserialized;
            //            }
            //            catch
            //            {
            //                MessageBox.Show("Erreur dans la déserialization de la liste des notifications. Merci de poster ce bug sur le topic HFR7 en précisant nom et page du topic :o");
            //            }
            //        }
            //    }
            //}
            //int i = 0;
            //int removeNotif = -1;
            //foreach (NotifTopics notifTopic in notifTopicList)
            //{
            //    if (notifTopic.TopicId == idTopic)
            //    {

            //        ((ApplicationBarMenuItem)ApplicationBar.MenuItems[3]).Text = "se désabonner des notifications";
            //        if (pageNumber == numberOfPages)
            //        {
            //            removeNotif = i;
            //        }
            //    }
            //    i++;
            //}
            //if (removeNotif != -1)
            //{
            //    notifTopicList.RemoveAt(removeNotif);
            //    notifTopicList.Add(new NotifTopics()
            //    {
            //        TopicCatId = idCat.ToString(),
            //        TopicId = idTopic.ToString(),
            //        TopicIsNotif = false,
            //        TopicJump = jump.ToString(),
            //        TopicName = topicName.ToString(),
            //        TopicNumberOfPages = numberOfPages.ToString(),
            //        TopicPageNumber = pageNumber.ToString()
            //    });
            //    using (var file = isoStore.OpenFile("notifications.xml", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
            //    {
            //        using (var writer = new StreamWriter(file))
            //        {
            //            serializer.Serialize(writer.BaseStream, notifTopicList);
            //        }
            //    }

            //}


            // Récupération de la provenance
            NavigationContext.QueryString.TryGetValue("from", out from);
            if (from == "answer")
            {
                SystemTray.ProgressIndicator.Text = "Message posté avec succès !";
                if (isoStore.DirectoryExists("topics"))
                {
                    foreach (string fileName in isoStore.GetFileNames("topics/*"))
                    {
                        try { isoStore.DeleteFile("topics/" + fileName); }
                        catch { }
                    }
                }
            }

            if (from == "edit")
            {
                SystemTray.ProgressIndicator.Text = "Message édité avec succès !";
                if (isoStore.DirectoryExists("topics"))
                {
                    foreach (string fileName in isoStore.GetFileNames("topics/*"))
                    {
                        isoStore.DeleteFile("topics/" + fileName);
                    }
                }
            }

            // Titres
            topicNameTextBlock.Text = HttpUtility.HtmlDecode(topicName).ToUpper();
            if (from == "navquote")
            {
                topicNameTextBlock.Text = "...";
                pagesTextBlock.Text = "page " + pageNumber + "/...";
            }
            if (from == "answer") pagesTextBlock.Text = "page " + pageNumber + "/...";

            else pagesTextBlock.Text = "page " + pageNumber + "/" + numberOfPages;
            enterLoadingTextBlock.Begin();
            loadingTextBlockDisplayed = true;

            // Récupération de l'état du wifi
            Dispatcher.BeginInvoke(() =>
            {
                //if (DeviceNetworkInformation.IsWiFiEnabled && NetworkInterface.NetworkInterfaceType.ToString().ToLower().Contains("wireless")) 
                isWifi = true;
            });

            // Affichage des boutons
            AffichageBoutons();

            // Update de la tile
            UpdateTile();

            // Création du WebBrowser
            NavigateToTopic();
        }

        private void UpdateTile()
        {
            if (pageNumber == numberOfPages)
            {
                foreach (ShellTile tile in ShellTile.ActiveTiles)
                {
                    if (tile.NavigationUri.ToString() != "/")
                    {
                        string tilePostId;
                        int firstTilePostId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString()).IndexOf("&idtopic=") + "&idtopic=".Length;
                        int lastTilePostId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString()).IndexOf("&topicname=", firstTilePostId);
                        tilePostId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString().Substring(firstTilePostId, lastTilePostId - firstTilePostId));
                        if (tilePostId == idTopic)
                        {
                            StandardTileData NewTileDataRead = new StandardTileData
                            {
                                BackContent = "Pas de nouveaux messages",
                                BackgroundImage = new Uri("Images/tiles/" + HFR7.HFRClasses.GetCatName.ShortNameFromId(idCat) + ".png", UriKind.Relative),
                                BackBackgroundImage = new Uri("Images/tiles/read.png", UriKind.Relative)

                            };
                            // Update the Application Tile
                            tile.Update(NewTileDataRead);
                        }
                    }
                }
            }
        }

        private void AffichageBoutons()
        {
            // Affichage du bouton page suivante
            if (Convert.ToInt32(pageNumber) == Convert.ToInt32(numberOfPages))
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[3]).IsEnabled = false;
            }
            else
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[3]).IsEnabled = true;
            }
            // Affichage du bouton page précédente
            if (Convert.ToInt32(pageNumber) == 1)
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[2]).IsEnabled = false;
            }
        }

        private void NavigateToTopic()
        {
            if (from != "refresh")
            {
                if (isoStore.FileExists("topics/topic-" + idTopic + "-" + pageNumber + ".html") && (string)store["activateCache"] == "true")
                {
                    fromCache = true;
                    navigationStop = false;
                    NaviguerWebBrowserTopic();
                }
                else
                {
                    DownloadTopic(idCat, idTopic, pageNumber, container, true, true);
                }
            }
            else
            {
                DownloadTopic(idCat, idTopic, pageNumber, container, true, true);
            }

        }

        private void DownloadTopic(string idCatParse, string idTopicParse, string pageNumberParse, CookieContainer containerParse, bool navigateWhenFinished, bool saveTopic)
        {
            // Création du pattern de la page
            XDocument docTopic = XDocument.Load("HTMLfile.html");

            // Création d'un nombre random pour éviter la mise en cache du WebRequest (bug du contrôle)
            int randomNumber = new Random().Next(1, 100000);

            // Construction de URL du topic
            string urlTopic = "http://forum.hardware.fr/forum2.php?config=hfr.inc&cat=" + idCatParse + "&post=" + idTopicParse + "&page=" + pageNumberParse + "&sondage=1&owntopic=1&random=" + randomNumber.ToString();
            //string urlTopic = "https://dl.dropboxusercontent.com/u/68034087/bug.htm";
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
                    //if (args.Error.ToString().Contains("WebException"))
                    //{
                    //    //DownloadTopic(idCat, idTopic, pageNumber, container, true, true);
                    //}
                    else
                    {
                        Dispatcher.BeginInvoke(() => MessageBox.Show("Erreur inconnue."));
                    }
                }
                else if (saveTopic)
                {
                    int i = 0;
                    args.Document.OptionWriteEmptyNodes = true;
                    string[] meta = args.Document.DocumentNode.Descendants("meta").Where(x => (string)x.GetAttributeValue("name", "") == "Description").
                                                Select(y => y.GetAttributeValue("content", "")).ToArray();

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

                    string[] topicNameArray = args.Document.DocumentNode.Descendants("input")
                        .Where(x => (string)x.GetAttributeValue("name", "") == "sujet")
                        .Select(x => x.GetAttributeValue("value", "")).ToArray();

                    // Vérification nombre de pages
                    double numberOfPagesCheck;
                    try
                    {
                        int firstNumberOfPagesCheck = meta[0].IndexOf("Pages : ") + "Pages : ".Length;
                        int lastNumberOfPagesCheck = meta[0].LastIndexOf(" - Dernier message : ");
                        numberOfPagesCheck = Convert.ToDouble(meta[0].Substring(firstNumberOfPagesCheck, lastNumberOfPagesCheck - firstNumberOfPagesCheck));
                    }

                    catch
                    {
                        numberOfPagesCheck = Convert.ToDouble(numberOfPages);
                    }

                    if (topicName != HttpUtility.HtmlDecode(topicNameArray[0]))
                    {
                        topicName = HttpUtility.HtmlDecode(topicNameArray[0]);
                        Dispatcher.BeginInvoke(() =>topicNameTextBlock.Text = topicName.ToUpper());
                    }
                    if (numberOfPagesCheck != Convert.ToDouble(numberOfPages))
                    {
                        numberOfPages = numberOfPagesCheck.ToString();
                        Dispatcher.BeginInvoke(() =>
                        {
                            AffichageBoutons();
                        });
                    }
                    Dispatcher.BeginInvoke(() =>
                    {
                        AffichageBoutons();
                        pagesTextBlock.Text = "page " + pageNumber + "/" + numberOfPages;
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
                            avatarUri = "http://i.imgur.com/HQJ4Z.png";
                        }

                        // Date et heure
                        int firstDate = toolbar[i].IndexOf("Posté le ") + "Posté le ".Length; ;
                        int lastDate = 31;
                        dateHeure = Regex.Replace(toolbar[i].Substring(firstDate, lastDate), "&nbsp;", " ");

                        // Pseudo
                        int firstPseudo = messCase1[i].IndexOf("<b class=\"s2\">") + "<b class=\"s2\">".Length;
                        int lastPseudo = messCase1[i].IndexOf("</b>");
                        posterPseudo = messCase1[i].Substring(firstPseudo, lastPseudo - firstPseudo);
                        if (posterPseudo.Contains("Supprimer tous les messages de ce pseudo dans le sujet"))
                        {
                            firstPseudo = posterPseudo.IndexOf("return false;\">") + "return false;\">".Length;
                            lastPseudo = posterPseudo.IndexOf("</a>");
                            posterPseudo = posterPseudo.Substring(firstPseudo, lastPseudo - firstPseudo);
                        }
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
                        postText = postText.Replace("", ""); // Bug avec un caractère étrange...
                        // Affichage des images ?
                        if ((string)store["displayImages"] == "never" || (string)store["displayImages"] == "wifi" && !isWifi)
                        {
                            // Très sale mais fonctionne : vérification si l'image est un smiley ou non
                            postText = Regex.Replace(postText, "<img src=\"http://forum-images.hardware.fr/", "<save=\"http://forum-images.hardware.fr/");
                            postText = Regex.Replace(postText, "<img src=\"", "<a class=\"cLink\" href=\"§");
                            postText = Regex.Replace(postText, "<save=\"http://forum-images.hardware.fr/", "<img src=\"http://forum-images.hardware.fr/");
                            postText = Regex.Replace(postText, "style=\"margin: 5px\" />", ">Image</a>");
                        }

                        // Close icon uri
                        string closeIconUri;
                        if (currentTheme == "light") closeIconUri = "http://i.imgur.com/tbdle.png";
                        else closeIconUri = "http://i.imgur.com/BUjb9.png";

                        // Action button uri
                        string actionButtonUri;
                        //if (currentTheme == "light") actionButtonUri = "http://reho.st/http://self/pic/048a76e56afd2b0b3238a228683cffc65d9fa825.png";
                        //else actionButtonUri = "http://reho.st/http://self/pic/332d5e8737f3030cb4ce4ca93b3eebb1673a4aa5.png";
                        actionButtonUri = "http://i.imgur.com/eEws3.png";
                    

                        // Vérification si posteur = utilisateur de l'appli
                        XElement menu = new XElement("menu", "menu");
                        if (HttpUtility.UrlDecode(posterPseudo).ToLower() == HttpUtility.UrlDecode(userPseudo).ToLower())
                        {
                            // Menu de post
                            menu = new XElement("div", new XAttribute("class", "menu_post"), new XAttribute("name", "menu_post"), new XAttribute("id", "rep" + reponseId), new XAttribute("style", "visibility:hidden"),
                                new XElement("img", new XAttribute("onClick", "HideMenu('rep" + reponseId + "');"), new XAttribute("class", "menu_post_close"), new XAttribute("src", closeIconUri), new XAttribute("width", "32"), new XAttribute("height", "32")),
                                new XElement("ul", new XAttribute("class", "menu_post_list"),
                                    new XElement("li",
                                        new XElement("a", new XAttribute("href", "/AnswerTopic.xaml?action=edit&idtopic=" + idTopicParse + "&idcat=" + idCatParse + "&pagenumber=" + pageNumberParse + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&numrep=" + reponseId + "&numberofpages=" + numberOfPages), new XAttribute("class", "menu_post_link"), "éditer"), new XAttribute("class", "menu_post_li")),
                                     new XElement("li",
                                        new XElement("a", new XAttribute("href", "/AnswerTopic.xaml?action=delete&idtopic=" + idTopicParse + "&idcat=" + idCatParse + "&pagenumber=" + pageNumberParse + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&numrep=" + reponseId + "&numberofpages=" + numberOfPages), new XAttribute("class", "menu_post_link"), "supprimer"), new XAttribute("class", "menu_post_li")),
                                    new XElement("li",
                                        new XElement("a", new XAttribute("href", "/AnswerTopic.xaml?action=quote&idtopic=" + idTopicParse + "&idcat=" + idCatParse + "&pagenumber=" + pageNumberParse + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&numrep=" + reponseId + "&numberofpages=" + numberOfPages), new XAttribute("class", "menu_post_link"), "citer"), new XAttribute("class", "menu_post_li")),
                                    new XElement("li",
                                        new XElement("a", new XAttribute("href", "addquote?numrep=" + reponseId), new XAttribute("class", "menu_post_link"), "ajouter aux citations"), new XAttribute("class", "menu_post_li")),
                                    new XElement("li",
                                        new XElement("a", new XAttribute("href", "addfav?numrep=" + reponseId), new XAttribute("class", "menu_post_link"), "poser un favori"), new XAttribute("class", "menu_post_li"))));
                        }
                        else
                        {
                            // Menu de post
                            menu = new XElement("div", new XAttribute("class", "menu_post"), new XAttribute("name", "menu_post"), new XAttribute("id", "rep" + reponseId), new XAttribute("style", "visibility:hidden"),
                                new XElement("img", new XAttribute("onClick", "HideMenu('rep" + reponseId + "');"), new XAttribute("class", "menu_post_close"), new XAttribute("src", closeIconUri), new XAttribute("width", "32"), new XAttribute("height", "32")),
                                new XElement("ul", new XAttribute("class", "menu_post_list"),
                                new XElement("li",
                                    new XElement("a", new XAttribute("href", "/AnswerTopic.xaml?action=quote&idtopic=" + idTopicParse + "&idcat=" + idCatParse + "&pagenumber=" + pageNumberParse + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&numrep=" + reponseId + "&numberofpages=" + numberOfPages), new XAttribute("class", "menu_post_link"), "citer"), new XAttribute("class", "menu_post_li")),
                                new XElement("li",
                                    new XElement("a", new XAttribute("href", "addquote?numrep=" + reponseId), new XAttribute("class", "menu_post_link"), "ajouter aux citations"), new XAttribute("class", "menu_post_li")),
                                new XElement("li",
                                    new XElement("a", new XAttribute("href", "addfav?numrep=" + reponseId), new XAttribute("class", "menu_post_link"), "poser un favori"), new XAttribute("class", "menu_post_li")),
                                new XElement("li",
                                    new XElement("a", new XAttribute("href", "sendmp?pseudo=" + posterPseudo), new XAttribute("class", "menu_post_link"), "envoyer un MP"), new XAttribute("class", "menu_post_li"))
                                        ));
                        }

                        // Parsage du post
                        XElement post;
                        post = new XElement("table", new XAttribute("class", "tableGlobal"),
                                            menu,
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
                                new XElement("img", new XAttribute("class", "imageActionButton"), new XAttribute("src", actionButtonUri), new XAttribute("width", "32"), new XAttribute("height", "32"),
                                new XAttribute("onClick", "ShowMenu('rep" + reponseId + "');"))))), 
                                new XElement("tr",
                                                new XElement("td", new XAttribute("colspan", "3"),
                            // Post
                                                    new XElement("div", new XAttribute("class", "divPost"), postText)
                                                    )
                                                )
                                            );
                        try
                        {
                            string error = post.LastNode.ToString();
                            docTopic.Root.Element("body").Add(post);
                        }
                        catch { }

                        i++;
                    }

                    // Bas de page
                    XElement last = new XElement("div", new XAttribute("class", "divPageBas"), "page " + pageNumberParse + "/" + numberOfPages, new XElement("br", new XAttribute("style", "line-height:180px;")), new XElement("a", new XAttribute("name", "bas")), new XElement("br", new XAttribute("style", "line-height20px;")));
                    //XElement last = new XElement("div", new XAttribute("class", "divPageBas"), new XElement("a", new XAttribute("name", "bas")));
                    docTopic.Root.Element("body").Add(last);

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
                        content = Regex.Replace(content, "#REPLACEPAGES", "page " + pageNumberParse + "/" + numberOfPages);
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
                        enterWB.Begin();
                        quitLoadingTextBlock.Begin();
                        loadingTextBlockDisplayed = false;
                        try { SystemTray.ProgressIndicator.IsVisible = false;
                        if (currentOrientation == "landscape") SystemTray.IsVisible = false;
                        }
                        catch { }
                    });
                }
            }
        }

        void GoReponse()
        {


            if (jump != "" && jump != null)
            {
                // Le try sert à gérer les cas où le post a été supprimé
                try { readTopicWebBrowser.InvokeScript("JumpTo", "#" + jump); }
                catch 
                {
                    MessageBox.Show("Une erreur s'est produite dans l'accès au dernier post non lu (post supprimé), vous allez être redirigé en haut de page");
                }
                new Thread((ThreadStart)delegate
                {
                    Thread.Sleep(4000);
                    SaveTopic();
                }).Start();
                
            }
            else
            {
                enterWB.Begin();
                quitLoadingTextBlock.Begin();
                loadingTextBlockDisplayed = false;
                try { SystemTray.ProgressIndicator.IsVisible = false; } catch { }
                new Thread((ThreadStart)delegate
                {
                    Thread.Sleep(4000);
                    SaveTopic();
                }).Start();
            }
            // Vibrer quand c'est chargé
            if ((string)store["vibrateLoad"] == "true")
            {
                Microsoft.Devices.VibrateController vibrate = Microsoft.Devices.VibrateController.Default;
                vibrate.Start(TimeSpan.FromMilliseconds(50));
                Thread.Sleep(200);
                vibrate.Start(TimeSpan.FromMilliseconds(50));
            }
        }

        private void SaveTopic()
        {
            if ((string)store["activateCache"] == "true" && from != "navquote")
            {
                if (fromCache == true)
                {
                    // On valide le drapeau
                    DownloadTopic(idCat, idTopic, pageNumber, container, false, false);
                }
                if (pageNumber != numberOfPages)
                {
                    // Et on précharge la page d'après avec le dummy
                    DownloadTopic(idCat, idTopic, (Convert.ToInt32(pageNumber) + 1).ToString(), containerDummy, false, true);
                }
            }
        }

        void AddFav(string favNum)
        {
            SystemTray.ProgressIndicator.Text = "Ajout du favori...";
            SystemTray.ProgressIndicator.IsVisible = true;
            string addFavUri = "http://forum.hardware.fr/user/addflag.php?config=hfr.inc&cat=" + idCat + "&post=" + idTopic + "&numreponse=" + favNum;
            HtmlWeb.LoadAsync(addFavUri, container, (s, args) =>
            {
                if (args.Document == null)
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show("Serveur en cours de maintenance. Veuillez réessayer plus tard.")); ;
                }
                else
                {
                    int i = 0;
                    args.Document.OptionWriteEmptyNodes = true;
                    string[] reponse = args.Document.DocumentNode.Descendants("div").Where(x => (string)x.GetAttributeValue("class", "") == "hop").
                                                Select(y => y.InnerText).ToArray();
                    if (reponse[0].Contains("succès")) Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Favori positionné !");
                        try { SystemTray.ProgressIndicator.IsVisible = false; } catch { }
                    });
                    else Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Erreur !");
                        try { SystemTray.ProgressIndicator.IsVisible = false; } catch { }
                    });
                }
            });
        }

        void AddQuote(string quoteId)
        {
            ApplicationBar.IsVisible = false;
            Dispatcher.BeginInvoke(() => readTopicWebBrowser.InvokeScript("HideMenu", "rep" + quoteId));
            SystemTray.ProgressIndicator.IsVisible = true;
            SystemTray.ProgressIndicator.Text = "Ajout de la citation...";
            string urlQuote = "http://forum.hardware.fr/message.php?config=hfr.inc&cat=" + idCat + "&post=" + idTopic + "&numrep=" + quoteId;
            HtmlWeb.LoadAsync(urlQuote, container, (s, args) =>
            {
                if (args.Error != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Erreur dans la lecture de la quote. Informations : " + args.Error.Message + ", " + args.Error.Data);
                        ApplicationBar.IsVisible = true;
                        try { SystemTray.ProgressIndicator.IsVisible = false; } catch { }
                    });
                }
                if (args.Document == null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Serveur en cours de maintenance. Veuillez réessayer plus tard.");
                        ApplicationBar.IsVisible = true;
                        try { SystemTray.ProgressIndicator.IsVisible = false; } catch { }
                    });
                }
                else
                {
                    string[] quoteText = args.Document.DocumentNode.Descendants("textarea").Where(x => (string)x.GetAttributeValue("name", "") == "content_form").
                    Select(y => y.InnerText).ToArray();
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (store.Contains("listQuoteText"))
                        {
                            store["listQuoteText"] = store["listQuoteText"] + HttpUtility.HtmlDecode(quoteText[0]);
                        }
                        else
                        {
                            store.Add("listQuoteText", HttpUtility.HtmlDecode(quoteText[0]));
                        }
                        ApplicationBar.IsVisible = true;
                        try { SystemTray.ProgressIndicator.IsVisible = false; } catch { }
                    });
                }
            });
        }

        // Interception navigation hors application
        void readTopicWebBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            string navUri = e.Uri.ToString();
            if (navUri.Contains("ReadTopic.xaml"))
            {
                e.Cancel = true;
                NavigationService.Navigate(new Uri(e.Uri.ToString() + "&back=changepage", UriKind.Relative));
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

            else if (navUri.Contains("addfav"))
            {
                e.Cancel = true;
                AddFav(navUri.Split('=')[1]);
            }

            else if (navUri.Contains("addquote"))
            {
                e.Cancel = true;
                AddQuote(navUri.Split('=')[1]);
            }

            else if (navUri.Contains("sendmp"))
            {
                e.Cancel = true;
                NavigationService.Navigate(new Uri("/NewMp.xaml?pseudodesti=" + navUri.Split('=')[1] + "&url=" + HttpUtility.UrlEncode(NavigationService.CurrentSource.ToString()), UriKind.Relative));
            }

            else if (navUri.Contains("/forum2.php?config=hfr.inc"))
            {
                e.Cancel = true;
                navQuote = true;
                if (pageNumber == GetUri("page", navUri))
                {
                    Dispatcher.BeginInvoke(() =>
                        {
                            //navQuoteBackStack.Add("#rep" + navUri.Split('#')[1].Substring(1, navUri.Split('#')[1].Length - 1));
                            readTopicWebBrowser.InvokeScript("JumpTo", "#rep" + navUri.Split('#')[1].Substring(1, navUri.Split('#')[1].Length - 1));
                        });
                }
                else
                {
                    try { jump = navUri.Split('#')[1].Substring(1, navUri.Split('#')[1].Length - 1); }
                    catch { jump = ""; }

                    NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + GetUri("cat", navUri) + "&idtopic=" + GetUri("post", navUri) + "&topicname=...&pagenumber=" + GetUri("page", navUri) + "&jump=rep" + jump + "&numberofpages=0&from=navquote", UriKind.Relative));
                }
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
            if (internWebBrowserCanvas.Visibility == Visibility.Visible)
            {
                e.Cancel = true;
                try { SystemTray.ProgressIndicator.IsVisible = false; } catch { }
                internWebBrowserCanvas.Visibility = Visibility.Collapsed;
                internWebBrowser.Visibility = Visibility.Collapsed;
                TopicPanel.Opacity = 1;
            }
            else if (choosePageCanvas.Visibility == Visibility.Visible)
            {
                e.Cancel = true;
                choosePageCanvas.Visibility = Visibility.Collapsed;
                TopicPanel.Opacity = 1;
            }
            else if (searchIsOn)
            {
                e.Cancel = true;
                searchIsOn = false;
                quitSearch.Begin();
            }
            else
            {
                e.Cancel = true;
                if (backAction == "fftt")
                {
                    //BackwardOutTransition();
                    //Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri("/WelcomePage.xaml?pivot=1", UriKind.Relative)));

                    NavigationService.GoBack();
                }
                else
                {
                    store.Remove("navigatedback");
                    store.Add("navigatedback", "true");
                    BackwardOutTransition();
                    NavigationService.GoBack();
                }
            }
        }

        private void previousPageAppbarButton_Click(object sender, EventArgs e)
        {
            navigationStop = true;
            NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopic + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&pagenumber=" + (Convert.ToInt32(pageNumber) - 1).ToString() + "&numberofpages=" + numberOfPages + "&back=changepage", UriKind.Relative));
        }

        private void nextPageAppbarButton_Click(object sender, EventArgs e)
        {

            navigationStop = true;
            NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopic + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&pagenumber=" + (Convert.ToInt32(pageNumber) + 1).ToString() + "&numberofpages=" + numberOfPages + "&back=changepage", UriKind.Relative));
        }

        private void first_page_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopic + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&pagenumber=1&numberofpages=" + numberOfPages + "&back=changepage", UriKind.Relative));
        }

        private void last_page_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopic + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&pagenumber=" + numberOfPages + "&numberofpages=" + numberOfPages + "&back=changepage", UriKind.Relative));
        }

        private void answerButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AnswerTopic.xaml?idtopic=" + idTopic + "&idcat=" + idCat + "&pagenumber=" + pageNumber + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&action=answer&numberofpages=" + numberOfPages + "&jump=" + jump, UriKind.Relative));
        }

        private void choose_page_Click(object sender, EventArgs e)
        {
            choosePageCanvas.Visibility = Visibility.Visible;
            TopicPanel.Opacity = 0.3;
            pageNumberChooseTextBox.Focus();
        }

        private void pageNumberChooseButton_Click(object sender, RoutedEventArgs e)
        {
            string pageChosenString = pageNumberChooseTextBox.Text;
            double pageChosenDouble;
            if (pageChosenString == "")
            {
                MessageBox.Show("Veuillez entrer un numéro de page.");
            }
            else
            {
                bool isNum = double.TryParse(pageChosenString, out pageChosenDouble);
                if (!isNum)
                {
                    MessageBox.Show("Veuillez entrer un numéro de page correct.");
                }
                else
                {
                    if (pageChosenDouble < 0 || pageChosenDouble > Convert.ToDouble(numberOfPages))
                    {
                        MessageBox.Show("Veuillez entrer un numéro de page compris entre 0 et " + Convert.ToDouble(numberOfPages) + ".");
                    }
                    else
                    {
                        NavigationService.Navigate(new Uri("/ReadTopic.xaml?idtopic=" + idTopic + "&idcat=" + idCat + "&pagenumber=" + pageChosenDouble + "&topicname=" + topicName + "&numerofpages=" + numberOfPages + "&back=changepage", UriKind.Relative));
                    }
                }
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            int randomNum = new Random().Next(1, 100000);
            NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopic + "&topicname=" + topicName + "&pagenumber=" + pageNumber + "&jump=rep" + lastReponseId + "&numberofpages=" + numberOfPages + "&from=refresh&random=" + randomNum + "&back=changepage", UriKind.Relative));
        }

        private void internWebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            internWebBrowser.Visibility = Visibility.Visible;
            try { SystemTray.ProgressIndicator.IsVisible = false; } catch { }
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

        private void ReadTopicPA_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation.ToString().Contains("Landscape"))
            {
                readTopicWebBrowser.Height = 480;
                readTopicWebBrowser.Width = 730;
                try
                {
                    if (SystemTray.ProgressIndicator.IsVisible == false) SystemTray.IsVisible = false;
                }
                catch { }
                currentOrientation = "landscape";
            }
            else if (e.Orientation.ToString().Contains("Portrait"))
            {
                readTopicWebBrowser.Height = 720;
                readTopicWebBrowser.Width = 480;
                currentOrientation = "portrait";
                SystemTray.IsVisible = true;
            }
        }

        private void ApplicationBar_StateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            if (e.IsMenuVisible == true && SystemTray.ProgressIndicator.IsVisible == false)
            {
                if (ReadTopicPA.Orientation.ToString().Contains("Portrait"))
                {
                    enterLoadingTextBlock.Begin();
                    loadingTextBlockDisplayed = true;
                }
                else
                {
                    SystemTray.IsVisible = true;
                }
            }
            if (e.IsMenuVisible == false && SystemTray.ProgressIndicator.IsVisible == false)
            {
                if (ReadTopicPA.Orientation.ToString().Contains("Portrait"))
                {
                    quitLoadingTextBlock.Begin();
                    loadingTextBlockDisplayed = false;
                }
                else
                {
                    SystemTray.IsVisible = false;
                }
            }
        }

        private void search_Click(object sender, EventArgs e)
        {
            if (!searchIsOn)
            {
                searchCanvas.Visibility = System.Windows.Visibility.Visible;
                enterSearch.Begin();
                searchIsOn = true;
            }
            else
            {
                searchIsOn = false;
                quitSearch.Begin();
            }
        }

        private void searchPseudoTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchPseudoTextBox.Text == "entrez un pseudo") searchPseudoTextBox.Text = "";
        }

        private void searchWordTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (searchWordTextBox.Text == "entrez un mot") searchWordTextBox.Text = "";
        }

        private void searchPseudoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchPseudoTextBox.Text != "" || (searchPseudoTextBox.Text == "" && searchWordTextBox.Text != ""))
            {
                searchStartButton.IsEnabled = true;
            }
            else
            {
                searchStartButton.IsEnabled = false;
            }
        }

        private void searchWordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchWordTextBox.Text != "" || (searchWordTextBox.Text == "" && searchPseudoTextBox.Text != ""))
            {
                searchStartButton.IsEnabled = true;
            }
            else
            {
                searchStartButton.IsEnabled = false;
            }
        }


        private void searchStartButton_Click(object sender, RoutedEventArgs e)
        {
            string wordSearch;
            string pseudoSearch;
            if (searchWordTextBox.Text == "" || searchWordTextBox.Text == "entrez un mot") wordSearch = "";
            else wordSearch = searchWordTextBox.Text;
            if (searchPseudoTextBox.Text == "" || searchPseudoTextBox.Text == "entrez un pseudo") pseudoSearch = "";
            else pseudoSearch = searchPseudoTextBox.Text;
            MessageBoxResult result = MessageBox.Show("Attention, la recherche peut être une opération très longue si les résultats sont nombreux. Voulez-vous continuer ?", "Attention", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                NavigationService.Navigate(new Uri("/SearchTopic.xaml?idtopic=" + idTopic + "&idcat=" + idCat + "&word=" + wordSearch + "&spseudo=" + pseudoSearch + "&topicname=" + HttpUtility.HtmlEncode(topicName), UriKind.Relative));
            }
        }

        private void PinTopic_Click(object sender, EventArgs e)
        {
            string backgroundImageTile;
            // Create the Tile object and set some initial properties for the Tile.
            ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopic + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&pagenumber=" + pageNumber + "&jump=" + jump + "&numberofpages=" + numberOfPages));
            if (TileToFind == null)
            {
                backgroundImageTile = HFRClasses.GetCatName.ShortNameFromId(idCat);

                // The Count value of 12 shows the number 12 on the front of the Tile. Valid values are 1-99.
                // A Count value of 0 indicates that the Count should not be displayed.
                StandardTileData NewTileData = new StandardTileData
                {
                    BackgroundImage = new Uri("images/tiles/" + backgroundImageTile + "_new.png", UriKind.Relative),
                    BackBackgroundImage = new Uri("Images/tiles/unread.png", UriKind.Relative),
                    Title = topicName,
                    BackContent = "Nouveaux messages !",
                    BackTitle = topicName
                };

                // Create the Tile and pin it to Start. This will cause a navigation to Start and a deactivation of our application.
                // TopicUriFav = "/ReadTopic.xaml?idcat=" + topicCatId + "&idtopic=" + topicId + "&topicname=" + HttpUtility.UrlEncode(line) + "&pagenumber=" + pageNumber + "&reponseid=" + reponseId + "&numberofpages=" + numberOfPagesTopicLine,
                ShellTile.Create(new Uri("/FromTileToTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopic + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&pagenumber=" + pageNumber + "&jump=" + jump + "&numberofpages=" + numberOfPages, UriKind.Relative), NewTileData);

            }
            else
            {
                MessageBox.Show("Ce favori est déjà épinglé sur la page d'accueil.");
            }
        }

        //private void SuscribeNotif_Click(object sender, EventArgs e)
        //{
        //    XmlSerializer serializer = new XmlSerializer(notifTopicList.GetType());
        //    using (var file = isoStore.OpenFile("notifications.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read))
        //    {
        //        using (var reader = new StreamReader(file))
        //        {

        //            object deserialized = serializer.Deserialize(reader.BaseStream);
        //            notifTopicList = (List<HFRClasses.NotifTopics>)deserialized;
        //        }
        //    }
        //    int i = 0;
        //    int removeNotif = -1;
        //    foreach (NotifTopics notifTopic in notifTopicList)
        //    {
        //        if (notifTopic.TopicId == idTopic)
        //        {
        //            removeNotif = i;
        //            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[3]).Text = "s'abonner aux notifications";
        //            MessageBox.Show("Vous êtes maintenant désabonné des notifications de nouveaux messages de ce sujet !");
        //        }
        //        i++;
        //    }
        //    if (removeNotif != -1) notifTopicList.RemoveAt(removeNotif);
        //    else
        //    {
        //        notifTopicList.Add(new NotifTopics()
        //        {
        //            TopicCatId = idCat.ToString(),
        //            TopicId = idTopic.ToString(),
        //            TopicIsNotif = false,
        //            TopicJump = jump.ToString(),
        //            TopicName = topicName.ToString(),
        //            TopicNumberOfPages = numberOfPages.ToString(),
        //            TopicPageNumber = pageNumber.ToString()
        //        });
        //        MessageBox.Show("Vous êtes maintenant abonné aux notifications de nouveaux messages de ce sujet !");
        //        ((ApplicationBarMenuItem)ApplicationBar.MenuItems[3]).Text = "se désabonner des notifications";
        //    }

        //    if (isoStore.FileExists("notifications.xml")) isoStore.DeleteFile("notifications.xml");
        //    using (var file = isoStore.OpenFile("notifications.xml", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
        //    {
        //        using (var writer = new StreamWriter(file))
        //        {
        //            serializer.Serialize(writer.BaseStream, notifTopicList);
        //        }
        //    }
        //}

        private void GestureListener_Flick(object sender, FlickGestureEventArgs e)
        {
            //if ((e.VerticalVelocity > 50 || e.VerticalVelocity < -50) && e.Direction == System.Windows.Controls.Orientation.Vertical)
            //{
            //    readTopicWebBrowser.InvokeScript("getVerticalOffset");
            //}

            // Page suivante
            if (e.HorizontalVelocity < -2000 && e.Direction == System.Windows.Controls.Orientation.Horizontal && Convert.ToInt32(pageNumber) < Convert.ToInt32(numberOfPages))
            {
                quitToLeftWB.Begin();
                navigationStop = true;
                NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopic + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&pagenumber=" + (Convert.ToInt32(pageNumber) + 1).ToString() + "&numberofpages=" + numberOfPages + "&back=changepage", UriKind.Relative));
            }

            // Page précédente
            if (e.HorizontalVelocity > 2000 && e.Direction == System.Windows.Controls.Orientation.Horizontal && Convert.ToInt32(pageNumber) > 1)
            {
                // Load the previous image
                navigationStop = true;
                quitToRightWB.Begin();
                NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopic + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&pagenumber=" + (Convert.ToInt32(pageNumber) - 1).ToString() + "&numberofpages=" + numberOfPages + "&back=changepage", UriKind.Relative));
            }
        }

        //private void readTopicWebBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        //{
        //    if (e.Value == "scrollcompleted" && loadingTextBlockDisplayed == false)
        //    {
        //        enterLoadingTextBlock.Begin();
        //        loadingTextBlockDisplayed = true;
        //    }
        //    if (e.Value == "scrollnotcompleted" && loadingTextBlockDisplayed == true)
        //    {
        //        quitLoadingTextBlock.Begin();
        //        loadingTextBlockDisplayed = false;
        //    }
        //}
    }
}