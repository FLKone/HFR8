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
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO;
using System.Threading;
using Microsoft.Phone.Shell;
using System.Text;
using Microsoft.Phone.Tasks;

namespace HFR7
{
    public partial class Mp : PhoneApplicationPage
    {
        string currentTheme;
        string userPseudo;
        string senderPseudo;
        string pageNumber;
        string numberOfPages;
        string mpId;
        string content;
        string backAction;
        bool invoked = false;
        string triangleUriEnvoye;
        string triangleUriRecu;
        string accentColor;
        string answer;
        string hash;
        string position;
        string smileyCode;
        double answerHeight = 100;
        string sujet;
        bool navigatedBack = false;
        int randomNumber;
        IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        XDocument docTopic;
        CookieContainer container;
        IsolatedStorageSettings store = IsolatedStorageSettings.ApplicationSettings;
        
        public Mp()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if ((string)store["disableLandscape"] == "true") MpPA.SupportedOrientations = SupportedPageOrientation.Portrait;
            // Affichage de la ProgressBar
            progressBar.Visibility = System.Windows.Visibility.Visible;

            // Thème
            if ((Visibility)Resources["PhoneDarkThemeVisibility"] == System.Windows.Visibility.Visible) currentTheme = "dark";
            else currentTheme = "light";

            // Accent Color
            accentColor = HFRClasses.ColorConvert.ConvertToHtml(((Color)Application.Current.Resources["PhoneAccentColor"]).ToString());

            // Récupération du cookie du store
            container = store["HFRcookies"] as CookieContainer;
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

            // Récupération du hash check
            hash = store["hash"] as String;

            // Récupération du pseudo
            userPseudo = store["userPseudo"] as string;

            // Récupération du titre du sujet
            NavigationContext.QueryString.TryGetValue("sujet", out sujet);

            // Récupération du numéro de la page
            NavigationContext.QueryString.TryGetValue("numberofpages", out numberOfPages);

            // Récupération du pseudo de l'expéditeur
            NavigationContext.QueryString.TryGetValue("senderpseudo", out senderPseudo);

            // Récupération de l'ID du MP
            NavigationContext.QueryString.TryGetValue("mpid", out mpId);

            // Récupération de la backaction
            NavigationContext.QueryString.TryGetValue("back", out backAction);

            // Récupération du smileycode
            NavigationContext.QueryString.TryGetValue("smileyCode", out smileyCode);

            // Récupération du smileycode
            NavigationContext.QueryString.TryGetValue("position", out position);

            if (backAction == "answer") NavigationService.RemoveBackEntry();
            if (backAction == "tool") NavigationService.RemoveBackEntry();
            if (backAction == "smiley")
            {
                NavigationService.RemoveBackEntry();
                NavigationService.RemoveBackEntry();
            }

            // Navigated back
            if (store.Contains("navigatedBack"))
            {
                navigatedBack = true;
                store.Remove("navigatedBack");
            }

            // Création d'un nombre random pour éviter la mise en cache du WebRequest (bug du contrôle)
            Random random = new Random();
            randomNumber = random.Next(1, 100000);

            // Page title
            PageTitle.Text = senderPseudo;

            // Smiley
            if (position != null && smileyCode != null)
            {
                string saveAnswer = (string)store["saveAnswer"];
                saveAnswer = saveAnswer.Insert(Convert.ToInt32(position), " " + smileyCode + " ");
                messageTextBox.Text = saveAnswer;
                messageTextBox.Select(Convert.ToInt32(position) + smileyCode.Length + 2, 0);
                store.Remove("saveanswer");
            }


            if (!isoStore.FileExists("mp-" + mpId + ".html") || (backAction != "tool" && backAction != "smiley"))
            {
                // Récupération MP
                DownloadMp();
            }
            else
            {
                NavigateMp();
            }
        }

        private void DownloadMp()
        {
            if (isoStore.FileExists("mp-" + mpId + ".html")) isoStore.DeleteFile("mp-" + mpId + ".html");
            // Création du pattern de la page
            docTopic = XDocument.Load("MP_HTMLfile_" + currentTheme + ".html");

            // Construction de URL du MP
            //https://forum.hardware.fr/forum2.php?cat=prive&post=1705945&page=1
            string urlMp = "https://forum.hardware.fr/forum2.php?config=hfr.inc&cat=prive&post=" + mpId + "&page=" + numberOfPages + "&random=" + randomNumber.ToString();
            //string urlMp = "https://forum.hardware.fr/forum2.php?config=hfr.inc&cat=prive&post=1633240&page=1&random=" + randomNumber.ToString();

            // Récupération et parsage de l'HTML du MP sur HFR
            HtmlWeb.LoadAsync(urlMp, container, (s, args) =>
            {

                    if (args.Error != null)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("Erreur dans la lecture du MP. Informations : " + args.Error.Message + ", " + args.Error.Data);
                            progressBar.Visibility = System.Windows.Visibility.Collapsed;
                        });
                    }
                    if (args.Document == null)
                    {
                        Dispatcher.BeginInvoke(() => MessageBox.Show("Serveur en cours de maintenance. Veuillez réessayer plus tard.")); ;
                    }

                    int i = 0;
                    args.Document.OptionWriteEmptyNodes = true;

                    string[] mpText = args.Document.DocumentNode.Descendants("div").Where(x => (bool)x.GetAttributeValue("id", "").Contains("para") == true).
                                                Select(y => y.InnerHtml).ToArray();

                    string[] messCase1 = args.Document.DocumentNode.Descendants("td")
                        .Where(x => (bool)x.GetAttributeValue("class", "").Contains("messCase1") == true && (bool)x.InnerHtml.Contains("<div><b class=\"s2\">Publicité</b></div>") == false && (bool)x.InnerHtml.Contains("Auteur") == false)
                        .Select(x => x.InnerHtml).ToArray();

                    string[] messCase2 = args.Document.DocumentNode.Descendants("td")
                        .Where(x => (bool)x.GetAttributeValue("class", "").Contains("messCase2") == true && (bool)x.InnerHtml.Contains("<div><b class=\"s2\">Publicité</b></div>") == false)
                        .Select(x => x.InnerText).ToArray();

                    string[] toolbar = args.Document.DocumentNode.Descendants("div")
                        .Where(x => (bool)x.GetAttributeValue("class", "").Contains("toolbar") == true)
                        .Select(x => x.InnerHtml).ToArray();

                    foreach (string line in mpText)
                    {
                        string dateHeure;
                        string posterPseudo;
                        string reponseId;
                        string tableGlobalType;

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
                        int lastReponseId = messCase1[i].LastIndexOf("\" alt=\"n°");
                        reponseId = messCase1[i].Substring(firstReponseId, lastReponseId - firstReponseId);
                        // Mise en forme du texte
                        int lastPostText = mpText[i].IndexOf("<span class=\"signature\">");
                        if (lastPostText == -1)
                        {
                            lastPostText = mpText[i].Length;
                        }
                        string postText = mpText[i].Substring(0, lastPostText);
                        postText = Regex.Replace(HttpUtility.HtmlDecode(postText), " target=\"_blank\"", "");

                        // Affichage des images ?
                        //if ((string)store["displayImages"] == "always") 
                        //postText = Regex.Replace(postText, "md_verif_size(this,'Cliquez pour agrandir','2','250')", "resize(this)");
                        //else
                        //{
                        //    // Très sale mais fonctionne : vérification si l'image est un smiley ou non
                        //    postText = Regex.Replace(postText, "<img src=\"http://forum-images.hardware.fr/", "<save=\"http://forum-images.hardware.fr/");
                        //    postText = Regex.Replace(postText, "<img src=\"", "<a class=\"cLink\" href=\"§");
                        //    postText = Regex.Replace(postText, "<save=\"http://forum-images.hardware.fr/", "<img src=\"http://forum-images.hardware.fr/");
                        //    postText = Regex.Replace(postText, "style=\"margin: 5px\" />", ">Image</a>");
                        //}

                        // Close icon uri
                        string closeIconUri;
                        if (currentTheme == "light") closeIconUri = "http://reho.st/thumb/http://self/pic/7e245621270799c85a73129c8987a861fc1b00a2.png";
                        else closeIconUri = "http://reho.st/http://self/pic/fd15923f5210a70161468f6019643695ae57ef98.png";


                        // Vérification si posteur = utilisateur de l'appli
                        triangleUriEnvoye = "http://reho.st/http://self/pic/eba141848517d633c15fa519ecaf49d8c99758fc.png";
                        triangleUriRecu = "http://reho.st/http://self/pic/eba141848517d633c15fa519ecaf49d8c99758fc.png";
                        if (posterPseudo.ToLower() == userPseudo.ToLower())
                        {
                            tableGlobalType = "User";
                            //if (currentTheme == "dark")
                            //{
                            //    triangleUriEnvoye = "http://reho.st/http://self/pic/afbdb13a508cde183cca7efcc3cee7a2234681fb.png";
                            //}
                            //else
                            //{
                            //    triangleUriEnvoye = "http://reho.st/http://self/pic/320a43454faca6e539d1f3a3de06a91842e36c66.png";
                            //}
                        }
                        else
                        {
                            tableGlobalType = "Sender";
                            //if (currentTheme == "dark")
                            //{
                            //    triangleUriRecu = "http://reho.st/http://self/pic/2fb379ab9953769ab90506a1f0698f1af4c2ed8c.png";
                            //}
                            //else
                            //{
                            //    triangleUriRecu = "http://reho.st/http://self/pic/d9bbf2b3b409f8259eb1b567e5a8e22e8635df2b.png";
                            //}
                        }

                        // Parsage du MP
                        XElement post;
                        // Avatars activés
                        post = new XElement("table", new XAttribute("class", "tableGlobal" + tableGlobalType),
                                            new XElement("tr", new XAttribute("class", "trPost"),
                                                new XElement("td", new XAttribute("colspan", "3"),
                                                    new XElement("div", new XAttribute("class", "divPost"), postText))),
                                            new XElement("tr", new XAttribute("class", "trDateHeure"),
                                                new XElement("td", new XAttribute("colspan", "3"),
                                                        dateHeure))
                                            );
                        docTopic.Root.Element("body").Add(post);

                        if (i == mpText.Length - 2)
                        {
                            // Bas de page
                            XElement last = new XElement("a", new XAttribute("name", "bas"));
                            docTopic.Root.Element("body").Add(last);
                        }
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
                        content = Regex.Replace(content, "#PINCHTOZOOMOPTION", (string)store["pinchToZoomOption"]);
                        content = Regex.Replace(content, "#ACHANGER", accentColor);
                    }
                    catch
                    {
                        MessageBox.Show("Erreur dans la mise en forme du HTML");
                    }

                    // Ecriture du HTML dans le fichier
                    using (var file = isoStore.OpenFile("mp-" + mpId + ".html", System.IO.FileMode.Create, System.IO.FileAccess.Write))
                    {
                        using (var writer = new StreamWriter(file))
                        {
                            writer.Write(content);
                        }
                    }

                // Navigation vers ce fichier
                Dispatcher.BeginInvoke(() =>
                {
                    NavigateMp();
                });
            });
        }

        private void NavigateMp()
        {
            mpWebBrowser.Navigate(new Uri("mp-" + mpId + ".html", UriKind.Relative));
            invoked = false;
            //mpWebBrowser.Navigated += new EventHandler<System.Windows.Navigation.NavigationEventArgs>(mpWebBrowser_Navigated);
        }

        void mpWebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            string lol = e.Uri.ToString();
            if (e.Uri.ToString().Contains("mp") && !e.Uri.ToString().Contains("#bas"))
            {
                mpWebBrowser.InvokeScript("JumpTo", "#bas");
                mpWebBrowser.Opacity = 1;
                ArchiveMp();
                progressBar.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private void SendAnswer()
        {
            // Désactivation des bouton de post et de la textbox
            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
            //((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
            messageTextBox.IsEnabled = false;

            // Affichage de la progress bar
            progressBar.Visibility = System.Windows.Visibility.Visible;

            // Réponse
            answer = Regex.Replace(messageTextBox.Text, "\r\n", "\r");
            answer = Regex.Replace(answer, "\r", Environment.NewLine);
            answer = HttpUtility.UrlEncode(answer);

            // Création de l'objet HttpWebRequest.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://forum.hardware.fr/bddpost.php");

            // ContentType
            request.ContentType = "application/x-www-form-urlencoded";

            //Cookie
            request.CookieContainer = container;

            // Méthode en POST
            request.Method = "POST";

            // Ligne spécifique à SFR : headers EXPECT 100 CONTINUE
            request.Headers["User-Agent"] = "Mozilla /4.0 (compatible; MSIE 7.0; Windows Phone OS 7.0; IEMobile/7.0) Vodafone/1.0/SFR_v1615/1.56.163.8.39";

            // Démarrage de l'opération asynchrone
            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // Arrêt de l'opération
            Stream postStream = request.EndGetRequestStream(asynchronousResult);

            // Requête
            string postData = "&MsgIcon=20&content_form=" + answer + "&hash_check=" + hash + "&new=0&post=" + mpId + "&cat=prive" + "&sujet=" + sujet +"&subcat=0&verifrequet=1100&p=1&sondage=0&owntopic=1&config=hfr.inc&emaill=0&pseudo=" + store["userPseudo"] + "&signature=1";

            // Conversion du string en byte
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Enregistrement de la requête vers le string
            postStream.Write(byteArray, 0, postData.Length);
            postStream.Close();

            // Démarrage de l'opération asynchrone pour avoir la réponse
            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // Arrêt de l'opération
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            var stream = response.GetResponseStream();
            var reader = new StreamReader(stream);
            string errorMessage = "";
            string readerToEnd = reader.ReadToEnd();
            if (!readerToEnd.Contains("avec succès"))
            {
                Dispatcher.BeginInvoke(() =>
                {
                    if (readerToEnd.Contains("tentatives de flood")) errorMessage = "Afin d'éviter le flood, merci de ne pas poster plus de 3 messages en 10 minutes :o";
                    else if (readerToEnd.Contains("ce sujet a été fermé")) errorMessage = "Ce MP est fermé. Désolé de pas l'avoir dit avant, c'était trop chiant à coder :o";
                    else errorMessage = "Le serveur ne répond pas. Vérifiez votre connexion ou l'état de HFR";

                    MessageBox.Show("Erreur ! \n" + errorMessage);
                    ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
                    //((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true;
                    messageTextBox.IsEnabled = true;
                });
            }
            else
            {
                Dispatcher.BeginInvoke(() =>
                {
                    int randomNum = new Random().Next(1, 100000);
                    NavigationService.Navigate(new Uri("/Mp.xaml?senderpseudo=" + senderPseudo + "&mpid=" + mpId + "&numberofpages=" + numberOfPages + "&sujet=" + sujet + "&random=" + randomNum + "&back=answer", UriKind.Relative));
                });
            }
            // Cachage de la progressBar
            Dispatcher.BeginInvoke(() => progressBar.Visibility = System.Windows.Visibility.Collapsed);
        }

        private void ArchiveMp()
        {
            //Thread.Sleep(4000);
            try
            {
                using (var readStream = new IsolatedStorageFileStream("mp-" + mpId + ".html", FileMode.Open, isoStore))
                {
                    using (var writeStream = new IsolatedStorageFileStream("mp-" + mpId + "-" + randomNumber + ".html", FileMode.Create, isoStore))
                    {
                        using (var reader = new StreamReader(readStream))
                        {
                            using (var writer = new StreamWriter(writeStream))
                            {
                                writer.Write(reader.ReadToEnd());
                            }
                        }
                    }
                }
            }
            catch { }

        }

        private void messageTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (messageTextBox.Text == "tapez votre message") messageTextBox.Text = "";

            mpMiniRectangle.Fill = (Brush)Application.Current.Resources["PhoneForegroundBrush"];
        }

        private void messageTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            mpMiniRectangle.Fill = (Brush)Application.Current.Resources["PhoneTextBoxBrush"];
        }

        private void answerButton_Click(object sender, EventArgs e)
        {
            //NavigationService.Navigate(new Uri("/AnswerMp.xaml?mpid=" + mpId, UriKind.Relative));
            SendAnswer();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            //e.Cancel = true;
            //NavigationService.Navigate(new Uri("/WelcomePage.xaml?pivot=3", UriKind.Relative));
            e.Cancel = true;
            store.Remove("navigatedback");
            store.Add("navigatedback", "true");
            NavigationService.GoBack();
        }

        private void moreButton_Click(object sender, EventArgs e)
        {

        }

        private void messageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (messageTextBox.Text != "" && messageTextBox.Text != "tapez votre message") ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
            else ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;

            if (messageTextBox.SelectionStart >= messageTextBox.Text.Length - 5)
            {
                if (messageTextBox.ActualHeight > answerHeight)
                {
                    if (messageTextBox.MinHeight != 175)
                    {
                        mpWebBrowser.Height = mpWebBrowser.Height - 75;
                        messageTextBox.MinHeight = 175;
                    }
                    messageScrollViewer.ScrollToVerticalOffset(double.MaxValue);
                }
            }
        }

        private void hfrrehostButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/HFRrehostMP.xaml", UriKind.Relative));
        }

        private void mpWebBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            string navUri = e.Uri.ToString();
            if (!navUri.Contains("mp-" + mpId + ".html"))
            {
                e.Cancel = true;
                WebBrowserTask task = new WebBrowserTask();
                task.URL = e.Uri.ToString();
                task.Show();
            }
        }

        private void smileyButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var readStream = new IsolatedStorageFileStream("mp-" + mpId + "-" + randomNumber + ".html", FileMode.Open, isoStore))
                {
                    using (var writeStream = new IsolatedStorageFileStream("mp-" + mpId + ".html", FileMode.Create, isoStore))
                    {
                        using (var reader = new StreamReader(readStream))
                        {
                            using (var writer = new StreamWriter(writeStream))
                            {
                                writer.Write(reader.ReadToEnd());
                            }
                        }
                    }
                }
                isoStore.DeleteFile("mp-" + mpId + "-" + randomNumber + ".html");
            }
            catch { }

            if (store.Contains("saveAnswer")) store.Remove("saveAnswer");
            if (messageTextBox.Text != "tapez votre message") store.Add("saveAnswer", messageTextBox.Text);
            else store.Add("saveAnswer", "");
            string position = messageTextBox.SelectionStart.ToString();
            NavigationService.Navigate(new Uri("/SmileyPage.xaml?senderpseudo=" + senderPseudo + "&mpid=" + mpId + "&topicname=" + sujet + "&numberofpages=" + numberOfPages + "&position=" + position + "&back=answer", UriKind.Relative));
        }

        private void Mp_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation.ToString().Contains("Landscape"))
            {
                mpWebBrowser.Height = 270;
                mpWebBrowser.Margin = new Thickness(170, 0, 0, 0);
            }
            else
            {
                mpWebBrowser.Height = 495;
                mpWebBrowser.Margin = new Thickness(0, 0, 0, 0);
            }
        }
    }
}