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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using HtmlAgilityPack;

namespace HFR7
{
    public partial class AnswerTopic : PhoneApplicationPage
    {
        string pageNumber;
        string idTopic;
        string idCat;
        string topicName;
        string hash;
        string answer;
        string action;
        string backAction;
        string numberOfPages;
        double answerHeight = 130;
        int selectionStart;
        int selectionEnd;
        string numRep;
        string currentTheme;
        string currentOrientation;
        string smileyCode;
        string saveAnswer;
        string reponseId;
        bool posted = false;
        string position;
        string hfrRehost;
        string quotedText;
        bool navigatedBack = false;
        CookieContainer container = new CookieContainer();
        System.IO.IsolatedStorage.IsolatedStorageSettings store = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;

        public AnswerTopic()
        {
            InitializeComponent();
        }
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

            SystemTray.ProgressIndicator = new ProgressIndicator();

            if ((string)store["disableLandscape"] == "true") AnswerTopicPage.SupportedOrientations = SupportedPageOrientation.Portrait;
            // Background clair ou foncé
            if ((Visibility)Resources["PhoneDarkThemeVisibility"] == System.Windows.Visibility.Visible) currentTheme = "dark";
            else currentTheme = "light";
            //if (AnswerTopicPage.Orientation == PageOrientation.Landscape || AnswerTopicPage.Orientation == PageOrientation.LandscapeLeft || AnswerTopicPage.Orientation == PageOrientation.LandscapeRight) currentOrientation = "landscape";
            //else currentOrientation = "portrait";
            //backgroundImageBrush.ImageSource = new BitmapImage(new Uri("Images/" + currentTheme + "/Background/Background_" + currentOrientation + ".jpg", UriKind.Relative));

            // Cookie
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

            //GET
            NavigationContext.QueryString.TryGetValue("pagenumber", out pageNumber);
            NavigationContext.QueryString.TryGetValue("idtopic", out idTopic);
            NavigationContext.QueryString.TryGetValue("idcat", out idCat);
            NavigationContext.QueryString.TryGetValue("topicname", out topicName);
            topicName = HttpUtility.UrlDecode(topicName);
            NavigationContext.QueryString.TryGetValue("action", out action);
            NavigationContext.QueryString.TryGetValue("numberofpages", out numberOfPages);
            NavigationContext.QueryString.TryGetValue("position", out position);
            NavigationContext.QueryString.TryGetValue("smileyCode", out smileyCode);
            NavigationContext.QueryString.TryGetValue("reponseid", out reponseId);
            NavigationContext.QueryString.TryGetValue("hfrrehost", out hfrRehost);
            NavigationContext.QueryString.TryGetValue("back", out backAction);
            NavigationContext.QueryString.TryGetValue("numrep", out numRep);

            // DEBUG
            //pageNumber = "7994";
            //topicName = "loal.com";
            //numberOfPages = "8000";
            //numRep = "27926498";
            //idTopic = "55667";
            //idCat = "13";
            //action = "answer";

            // Nettoyage du backstack
            if (backAction == "tool")
            {
                NavigationService.RemoveBackEntry();
                NavigationService.RemoveBackEntry();
            }

            // Navigated back
            if (store.Contains("navigatedBack") && backAction != "tool")
            {
                navigatedBack = true;
                store.Remove("navigatedBack"); 
            }

            // Si sauvegarde
            if (store.Contains("saveAnswer") && (action == "answer" || action == "edit" || action=="quote" || store.Contains("activated")))
            {
                if (idTopic == (string)store["saveAnswerTopicNumber"])
                {
                    saveAnswer = (string)store["saveAnswer"];
                    // Si smiley
                    if (store.Contains("quotedText"))
                    {
                        quotedText = (string)store["quotedText"];
                        store.Remove("quotedText");
                        if (quotedText.Length > 100 && !store.Contains("listQuoteText"))
                        {
                            showQuoteButton.Visibility = System.Windows.Visibility.Visible;
                            answerTextBox.IsEnabled = true;
                            ApplicationBar.IsVisible = true;
                            progressBar.Visibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                    if (position != null && smileyCode != null)
                    {
                        saveAnswer = saveAnswer.Insert(Convert.ToInt32(position), " " + smileyCode + " ");
                        answerTextBox.Text = saveAnswer;
                        answerTextBox.Select(Convert.ToInt32(position) + smileyCode.Length + 2, 0);
                        store.Remove("smileyCode");
                    }
                    if (position != null && hfrRehost != null)
                    {
                        saveAnswer = saveAnswer.Insert(Convert.ToInt32(position), hfrRehost);
                        answerTextBox.Text = saveAnswer;
                        answerTextBox.Select(Convert.ToInt32(position) + hfrRehost.Length, 0);
                    }

                }
            }

            // Si liste de quote
            if (action == "answer")
            {
                PageTitle.Text = "votre réponse";
                if (!store.Contains("listQuoteText")) answerTextBox.Focus();
            }

            if (store.Contains("listQuoteText") && (action == "answer" || action == "quote"))
            {
                string listQuoteText = (string)store["listQuoteText"];
                answerTextBox.Text = answerTextBox.Text + listQuoteText;
                store.Remove("listQuoteText");
                if (store.Contains("quotedText")) store.Remove("quotedText");
                store.Add("quotedText", listQuoteText);
            }
            
            // Hash check
            hash = store["hash"] as String;

            if (action == "quote" && backAction != "tool" && navigatedBack == false)
            {
                PageTitle.Text = "votre réponse";

                // Verrouillage de l'answerBox
                answerTextBox.IsEnabled = false;
                ApplicationBar.IsVisible = false;

                // Progress Bar
                progressBar.Visibility = System.Windows.Visibility.Visible;

                // On cherche la quote
                string urlQuote = "https://forum.hardware.fr/message.php?config=hfr.inc&cat=" + idCat + "&post=" + idTopic + "&numrep=" + numRep;
                HtmlWeb.LoadAsync(urlQuote, container, (s, args) =>
                    {
                        Dispatcher.BeginInvoke(() => progressBar.Visibility = System.Windows.Visibility.Visible);
                        if (args.Error != null)
                        {
                            Dispatcher.BeginInvoke(() =>
                            {
                                MessageBox.Show("Erreur dans la lecture de la quote. Informations : " + args.Error.Message + ", " + args.Error.Data);
                                progressBar.Visibility = System.Windows.Visibility.Collapsed;
                            });
                        }
                        if (args.Document == null)
                        {
                            Dispatcher.BeginInvoke(() =>
                                {
                                    MessageBox.Show("Serveur en cours de maintenance. Veuillez réessayer plus tard.");
                                    answerTextBox.IsEnabled = true;
                                    ApplicationBar.IsVisible = true;
                                    progressBar.Visibility = System.Windows.Visibility.Collapsed;
                                });
                        }
                        else
                        {
                            string[] quoteText = args.Document.DocumentNode.Descendants("textarea").Where(x => (string)x.GetAttributeValue("name", "") == "content_form").
                            Select(y => y.InnerText).ToArray();
                            quotedText = HttpUtility.HtmlDecode(quoteText[0]).Substring(0, HttpUtility.HtmlDecode(quoteText[0]).Length - 1);
                            if (store.Contains("quotedText")) store.Remove("quotedText");
                            store.Add("quotedText", quotedText);
                            Dispatcher.BeginInvoke(() =>
                            {
                                if (quotedText.Length > 100 && !store.Contains("listQuoteText"))
                                {
                                    showQuoteButton.Visibility = System.Windows.Visibility.Visible;
                                    answerTextBox.IsEnabled = true;
                                    ApplicationBar.IsVisible = true;
                                    progressBar.Visibility = System.Windows.Visibility.Collapsed;
                                }
                                else
                                {
                                    showQuoteButton.Visibility = System.Windows.Visibility.Collapsed;
                                    answerTextBox.Text = answerTextBox.Text + quotedText;
                                    answerTextBox.IsEnabled = true;
                                    ApplicationBar.IsVisible = true;
                                    progressBar.Visibility = System.Windows.Visibility.Collapsed;
                                }

                            });
                        }
                    });
            }

            if (action == "delete")
            {
                answerTextBox.Visibility = System.Windows.Visibility.Collapsed;
                NavigationContext.QueryString.TryGetValue("numrep", out numRep);
                MessageBoxResult result = MessageBox.Show("Êtes-vous sûr de supprimer ce message ?", "Confirmation", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    PageTitle.Text = "suppression en cours...";
                    DeleteAnswer();
                }
                else
                {
                    NavigationService.GoBack();
                }
            }

            if (action == "edit" && backAction != "tool")
            {
                PageTitle.Text = "édition"; 
                NavigationContext.QueryString.TryGetValue("numrep", out numRep);

                // Verrouillage de l'answerBox
                answerTextBox.IsEnabled = false;
                ApplicationBar.IsVisible = false;


                // Progress Bar
                progressBar.Visibility = System.Windows.Visibility.Visible;

                // On cherche le post à éditer
                NavigationContext.QueryString.TryGetValue("numrep", out numRep);
                string urlEdit = "https://forum.hardware.fr/message.php?config=hfr.inc&cat=" + idCat + "&post=" + idTopic + "&numreponse=" + numRep;
                HtmlWeb.LoadAsync(urlEdit, container, (s, args) =>
                {
                    Dispatcher.BeginInvoke(() => progressBar.Visibility = System.Windows.Visibility.Visible);
                    if (args.Error != null)
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("Erreur dans la lecture du post. Informations : " + args.Error.Message + ", " + args.Error.Data);
                            progressBar.Visibility = System.Windows.Visibility.Collapsed;
                        });
                    }
                    if (args.Document == null)
                    {
                        Dispatcher.BeginInvoke(() =>
                            {
                                MessageBox.Show("Serveur en cours de maintenance. Veuillez réessayer plus tard.");
                                answerTextBox.IsEnabled = true;
                                ApplicationBar.IsVisible = true;
                                progressBar.Visibility = System.Windows.Visibility.Collapsed;
                            });
                    }
                    else
                    {
                        string[] editText = args.Document.DocumentNode.Descendants("textarea").Where(x => (string)x.GetAttributeValue("name", "") == "content_form").
                        Select(y => y.InnerText).ToArray();
                            
                        Dispatcher.BeginInvoke(() =>
                        {
                            answerTextBox.Text = HttpUtility.HtmlDecode(editText[0]) + Environment.NewLine;
                            answerTextBox.IsEnabled = true;
                            ApplicationBar.IsVisible = true;
                            progressBar.Visibility = System.Windows.Visibility.Collapsed;
                        });
                    }
                });

            }


        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (action == "answer" || action == "quote")
            {
                PostAnswer();
                if (store.Contains("saveAnswer")) store.Remove("saveAnswer");
            }
            if (action == "edit")
            {
                EditAnswer();
                if (store.Contains("saveAnswer")) store.Remove("saveAnswer");
            }
        }

        private void PostAnswer()
        {
            // Est-ce que la quote a été incluse ?
            if (showQuoteButton.Visibility == System.Windows.Visibility.Visible && action == "quote")
            {
                string answerSave = answerTextBox.Text;
                answerTextBox.Text = quotedText + answerSave;
            }
            // Désactivation des bouton de post et de la textbox
            ApplicationBar.IsVisible = false;
            answerTextBox.IsEnabled = false;

            // Affichage de la ProgressBar
                SystemTray.ProgressIndicator.IsVisible = true;
                SystemTray.ProgressIndicator.IsIndeterminate = true;
                SystemTray.ProgressIndicator.Text = "Envoi de votre réponse...";

            // Réponse
            answer = Regex.Replace(answerTextBox.Text, "\r\n", "\r");
            answer = Regex.Replace(answer, "\r", Environment.NewLine);
            answer = HttpUtility.UrlEncode(answer);
            //answer = Regex.Replace(HttpUtility.HtmlEncode(answer), "%250D%250A", "%0D%0A");


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

            try
            {
                // Démarrage de l'opération asynchrone
                request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
            }
            catch { MessageBox.Show("Erreur de connectivité. Veuillez réessayer"); }
        }


        private void EditAnswer()
        {
            // Désactivation des bouton de post et de la textbox
            ApplicationBar.IsVisible = false;
            answerTextBox.IsEnabled = false;

            // Affichage de la progress bar
            SystemTray.ProgressIndicator.IsVisible = true;
            SystemTray.ProgressIndicator.IsIndeterminate = true;
            SystemTray.ProgressIndicator.Text = "Envoi de votre réponse...";

            // Réponse
            answer = Regex.Replace(answerTextBox.Text, "\r\n", "\r");
            answer = Regex.Replace(answer, "\r", Environment.NewLine);
            answer = HttpUtility.UrlEncode(answer);

            // Création de l'objet HttpWebRequest.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://forum.hardware.fr/bdd.php");

            // ContentType
            request.ContentType = "application/x-www-form-urlencoded";

            //Cookie
            request.CookieContainer = container;

            // Méthode en POST
            request.Method = "POST";

            // Ligne spécifique à SFR : headers EXPECT 100 CONTINUE
            request.Headers["User-Agent"] = "Mozilla /4.0 (compatible; MSIE 7.0; Windows Phone OS 7.0; IEMobile/7.0) Vodafone/1.0/SFR_v1615/1.56.163.8.39";

            // Démarrage de l'opération asynchrone
            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallbackEdit), request);
        }

        
        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // Arrêt de l'opération
            Stream postStream = request.EndGetRequestStream(asynchronousResult);

            // Requête
            string postData = "&MsgIcon=20&content_form=" + answer + "&hash_check=" + hash + "&new=0&post=" + idTopic + "&cat=" + idCat + "&subcat=" + idCat + "&page=" + pageNumber + "&verifrequet=1100&p=1&sondage=0&owntopic=1&config=hfr.inc&emaill=0&pseudo=" + store["userPseudo"] + "&sujet=" + HttpUtility.UrlEncode(topicName) + "&signature=1";

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
            try
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
                            else if (readerToEnd.Contains("ce sujet a été fermé")) errorMessage = "Ce sujet est fermé. Désolé de pas l'avoir dit avant, c'était trop chiant à coder :o";
                            else errorMessage = "Le serveur ne répond pas. Vérifiez votre connexion ou l'état de HFR";
                            MessageBox.Show("Erreur ! \n" + errorMessage);
                            ApplicationBar.IsVisible = true;
                            answerTextBox.IsEnabled = true;
                        });
                }
                else
                {
                    Dispatcher.BeginInvoke(() =>
                        {
                            posted = true;
                            NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopic + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&pagenumber=" + numberOfPages + "&jump=bas" + "&numberofpages=" + numberOfPages + "&from=answer&back=answer", UriKind.Relative));
                        });
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Erreur de connexion. Vérifiez l'état du serveur ou votre connectivité.");
                        answerTextBox.IsEnabled = true;
                        ApplicationBar.IsVisible = true;
                        progressBar.Visibility = System.Windows.Visibility.Collapsed;
                    });
            }
            // Cachage de la progressBar
            Dispatcher.BeginInvoke(() => progressBar.Visibility = System.Windows.Visibility.Collapsed);
        }


        private void DeleteAnswer()
        {
            // Désactivation des bouton de post et de la textbox
            ApplicationBar.IsVisible = false;
            answerTextBox.IsEnabled = false;

            // Affichage de la d bar
            progressBar.Visibility = System.Windows.Visibility.Visible;

            // Création de l'objet HttpWebRequest.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://forum.hardware.fr/bdd.php");

            // ContentType
            request.ContentType = "application/x-www-form-urlencoded";

            //Cookie
            request.CookieContainer = container;

            // Méthode en POST
            request.Method = "POST";

            // Ligne spécifique à SFR : headers EXPECT 100 CONTINUE
            request.Headers["User-Agent"] = "Mozilla /4.0 (compatible; MSIE 7.0; Windows Phone OS 7.0; IEMobile/7.0) Vodafone/1.0/SFR_v1615/1.56.163.8.39";

            // Démarrage de l'opération asynchrone
            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallbackDelete), request);
        }

        private void GetRequestStreamCallbackDelete(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // Arrêt de l'opération
            Stream postStream = request.EndGetRequestStream(asynchronousResult);

            // Requête
            string postData = "&hash_check=" + hash + "&parents=&post=" + idTopic + "&stickold=0&new=0&cat=" + idCat + "&subcat=" + idCat + "&numreponse=" + numRep + "&page=" + pageNumber + "&verifrequet=1100&sond=0&cache=cache&p=1&config=hfr.inc&sondage=0&owntopic=1&config=hfr.inc&emaill=0&pseudo=" + store["userPseudo"] + "&password=" + store["userPassword"] + "&sujet=" + HttpUtility.UrlEncode(topicName) + "&signature=1&delete=1&submit=Valider+votre+message&content_form=content&sujet=sujet";
            // Conversion du string en byte
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Enregistrement de la requête vers le string
            postStream.Write(byteArray, 0, postData.Length);
            postStream.Close();

            // Démarrage de l'opération asynchrone pour avoir la réponse
            request.BeginGetResponse(new AsyncCallback(GetResponseCallbackDelete), request);
        }

        private void GetResponseCallbackDelete(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

                // Arrêt de l'opération
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                var stream = response.GetResponseStream();
                var reader = new StreamReader(stream);
                string reponseServ = reader.ReadToEnd();
                if (!reponseServ.Contains("avec succès"))
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Erreur ! \n" + reader.ReadToEnd());
                        ApplicationBar.IsVisible = true;
                        answerTextBox.IsEnabled = true;
                    });
                }
                else
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        posted = true;
                        NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopic + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&pagenumber=" + pageNumber + "&jump=bas" + "&numberofpages=" + numberOfPages + "&from=answer&back=answer", UriKind.Relative));
                    });
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show("Erreur de connexion. Vérifiez l'état du serveur ou votre connectivité."));
                answerTextBox.IsEnabled = true;
                ApplicationBar.IsVisible = true;
                progressBar.Visibility = System.Windows.Visibility.Collapsed;
            }

            // Cachage de la progressBar
            Dispatcher.BeginInvoke(() => progressBar.Visibility = System.Windows.Visibility.Collapsed);
        }


        private void GetRequestStreamCallbackEdit(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // Arrêt de l'opération
            Stream postStream = request.EndGetRequestStream(asynchronousResult);

            // Requête
            string postData = "&content_form=" + answer + "&hash_check=" + hash + "&new=0&post=" + idTopic + "&cat=" + idCat + "&numreponse=" + numRep + "&page=" + pageNumber + "&verifrequet=1100&p=1&sondage=0&owntopic=1&config=hfr.inc&emaill=0&pseudo=" + store["userPseudo"] + "&sujet=" + HttpUtility.UrlEncode(topicName) + "&signature=1";

            // Conversion du string en byte
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Enregistrement de la requête vers le string
            postStream.Write(byteArray, 0, postData.Length);
            postStream.Close();

            // Démarrage de l'opération asynchrone pour avoir la réponse
            request.BeginGetResponse(new AsyncCallback(GetResponseCallbackEdit), request);
        }

        private void GetResponseCallbackEdit(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

                // Arrêt de l'opération
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                var stream = response.GetResponseStream();
                var reader = new StreamReader(stream);
                if (!reader.ReadToEnd().Contains("avec succès"))
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Erreur ! \n" + reader.ReadToEnd());
                        ApplicationBar.IsVisible = true;
                        answerTextBox.IsEnabled = true;
                    });
                }
                else
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        posted = true;
                        NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopic + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&pagenumber=" + pageNumber + "&jump=bas&numberofpages=" + numberOfPages + "&from=answer&back=answer", UriKind.Relative));
                    });
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Erreur de connexion. Vérifiez l'état du serveur ou votre connectivité.");
                        answerTextBox.IsEnabled = true;
                        ApplicationBar.IsVisible = true;
                        progressBar.Visibility = System.Windows.Visibility.Collapsed;
                    });
            }

            // Cachage de la progressBar
            Dispatcher.BeginInvoke(() => progressBar.Visibility = System.Windows.Visibility.Collapsed);
        }


        private void boldButton_Click(object sender, RoutedEventArgs e)
        {
            int selectionStart = answerTextBox.SelectionStart;
            if (answerTextBox.SelectedText != "")
            {
                string tempText;
                int selectionEnd = answerTextBox.SelectionStart + answerTextBox.SelectionLength;
                tempText = answerTextBox.Text.Insert(selectionEnd, "[/b]").Insert(selectionStart, "[b]");
                answerTextBox.Text = tempText;
                answerTextBox.Focus();
                answerTextBox.Select(selectionEnd + 7, 0);
            }
            else
            {
                answerTextBox.Text = answerTextBox.Text.Insert(selectionStart, "[b][/b]");
                answerTextBox.Focus();
                answerTextBox.Select(selectionStart + 3, 0);
            }
            policeCanvas.Visibility = System.Windows.Visibility.Collapsed;
            ContentPanel.Opacity = 1;
        }

        private void italicsButton_Click(object sender, RoutedEventArgs e)
        {
            int selectionStart = answerTextBox.SelectionStart;
            if (answerTextBox.SelectedText != "")
            {
                string tempText;
                int selectionEnd = answerTextBox.SelectionStart + answerTextBox.SelectionLength;
                tempText = answerTextBox.Text.Insert(selectionEnd, "[/i]").Insert(selectionStart, "[i]");
                answerTextBox.Text = tempText;
                answerTextBox.Focus();
                answerTextBox.Select(selectionEnd + 7, 0);
            }
            else
            {
                answerTextBox.Text = answerTextBox.Text.Insert(selectionStart, "[i][/i]");
                answerTextBox.Focus();
                answerTextBox.Select(selectionStart + 3, 0);
            }
            policeCanvas.Visibility = System.Windows.Visibility.Collapsed;
            ContentPanel.Opacity = 1;
        }

        private void underlineButton_Click(object sender, RoutedEventArgs e)
        {
            int selectionStart = answerTextBox.SelectionStart;
            if (answerTextBox.SelectedText != "")
            {
                string tempText;
                int selectionEnd = answerTextBox.SelectionStart + answerTextBox.SelectionLength;
                tempText = answerTextBox.Text.Insert(selectionEnd, "[/u]").Insert(selectionStart, "[u]");
                answerTextBox.Text = tempText;
                answerTextBox.Focus();
                answerTextBox.Select(selectionEnd + 7, 0);
            }
            else
            {
                answerTextBox.Text = answerTextBox.Text.Insert(selectionStart, "[u][/u]");
                answerTextBox.Focus();
                answerTextBox.Select(selectionStart + 3, 0);
            }
            policeCanvas.Visibility = System.Windows.Visibility.Collapsed;
            ContentPanel.Opacity = 1;
        }

        private void spoilerButton_Click(object sender, RoutedEventArgs e)
        {
            int selectionStart = answerTextBox.SelectionStart;
            if (answerTextBox.SelectedText != "")
            {
                string tempText;
                int selectionEnd = answerTextBox.SelectionStart + answerTextBox.SelectionLength;
                tempText = answerTextBox.Text.Insert(selectionEnd, "[/spoiler]").Insert(selectionStart, "[spoiler]");
                answerTextBox.Text = tempText;
                answerTextBox.Focus();
                answerTextBox.Select(selectionEnd + 19, 0);
            }
            else
            {
                answerTextBox.Text = answerTextBox.Text.Insert(selectionStart, "[spoiler][/spoiler]");
                answerTextBox.Focus();
                answerTextBox.Select(selectionStart + 9, 0);
            }
            policeCanvas.Visibility = System.Windows.Visibility.Collapsed;
            ContentPanel.Opacity = 1;
        }

        private void urlButton_Click(object sender, RoutedEventArgs e)
        {
            int selectionStart = answerTextBox.SelectionStart;
            if (answerTextBox.SelectedText != "")
            {
                string tempText;
                int selectionEnd = answerTextBox.SelectionStart + answerTextBox.SelectionLength;
                tempText = answerTextBox.Text.Insert(selectionEnd, "[/url]").Insert(selectionStart, "[url]");
                answerTextBox.Text = tempText;
                answerTextBox.Focus();
                answerTextBox.Select(selectionEnd + 11, 0);
            }
            else
            {
                answerTextBox.Text = answerTextBox.Text.Insert(selectionStart, "[url][/url]");
                answerTextBox.Focus();
                answerTextBox.Select(selectionStart + 5, 0);
            }
            policeCanvas.Visibility = System.Windows.Visibility.Collapsed;
            ContentPanel.Opacity = 1;
        }

        private void imgButton_Click(object sender, RoutedEventArgs e)
        {
            int selectionStart = answerTextBox.SelectionStart;
            if (answerTextBox.SelectedText != "")
            {
                string tempText;
                int selectionEnd = answerTextBox.SelectionStart + answerTextBox.SelectionLength;
                tempText = answerTextBox.Text.Insert(selectionEnd, "[/img]").Insert(selectionStart, "[img]");
                answerTextBox.Text = tempText;
                answerTextBox.Focus();
                answerTextBox.Select(selectionEnd + 11, 0);
            }
            else
            {
                answerTextBox.Text = answerTextBox.Text.Insert(selectionStart, "[img][/img]");
                answerTextBox.Focus();
                answerTextBox.Select(selectionStart + 5, 0);
            }
            policeCanvas.Visibility = System.Windows.Visibility.Collapsed;
            ContentPanel.Opacity = 1;
        }

        private void citButton_Click(object sender, RoutedEventArgs e)
        {
            int selectionStart = answerTextBox.SelectionStart;
            if (answerTextBox.SelectedText != "")
            {
                string tempText;
                int selectionEnd = answerTextBox.SelectionStart + answerTextBox.SelectionLength;
                tempText = answerTextBox.Text.Insert(selectionEnd, "[/quote]").Insert(selectionStart, "[quote]");
                answerTextBox.Text = tempText;
                answerTextBox.Focus();
                answerTextBox.Select(selectionEnd + 15, 0);
            }
            else
            {
                answerTextBox.Text = answerTextBox.Text.Insert(selectionStart, "[quote][/quote]");
                answerTextBox.Focus();
                answerTextBox.Select(selectionStart + 7, 0);
            }
            policeCanvas.Visibility = System.Windows.Visibility.Collapsed;
            ContentPanel.Opacity = 1;

        }

        private void colorButton_Click(object sender, RoutedEventArgs e)
        {
            int selectionStart = answerTextBox.SelectionStart;
            Button colorButtonFound = (Button)sender;
            string colorFound = colorButtonFound.Tag.ToString();
            if (answerTextBox.SelectedText != "")
            {
                string tempText;
                int selectionEnd = answerTextBox.SelectionStart + answerTextBox.SelectionLength;
                tempText = answerTextBox.Text.Insert(selectionEnd, "[/" + colorFound + "]").Insert(selectionStart, "[" + colorFound + "]");
                answerTextBox.Text = tempText;
                answerTextBox.Focus();
                answerTextBox.Select(selectionEnd + 19, 0);
            }
            else
            {
                answerTextBox.Text = answerTextBox.Text.Insert(selectionStart, "[" + colorFound + "][/" + colorFound + "]");
                answerTextBox.Focus();
                answerTextBox.Select(selectionStart + 9, 0);
            }
            policeCanvas.Visibility = System.Windows.Visibility.Collapsed;
            ContentPanel.Opacity = 1;
        }

        private void answerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (answerTextBox.SelectionStart >= answerTextBox.Text.Length - 5)
            {
                answerScrollViewer.ScrollToVerticalOffset(double.MaxValue);
            }
        }

        private void policeButton_Click(object sender, EventArgs e)
        {
            if (policeCanvas.Visibility == System.Windows.Visibility.Collapsed)
            {
                this.Focus();
                policeCanvas.Visibility = System.Windows.Visibility.Visible;
                ContentPanel.Opacity = 0.3;
            }
            else
            {
                policeCanvas.Visibility = System.Windows.Visibility.Collapsed;
                ContentPanel.Opacity = 1;
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (policeCanvas.Visibility == System.Windows.Visibility.Visible)
            {
                e.Cancel = true;
                policeCanvas.Visibility = System.Windows.Visibility.Collapsed;
                ContentPanel.Opacity = 1;
            }
            else
            {
                if (store.Contains("saveAnswer")) store.Remove("saveAnswer");
                e.Cancel = true;
                store.Remove("navigatedback");
                store.Add("navigatedback", "true");
                NavigationService.GoBack();
                //NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopic + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&pagenumber=" + pageNumber + "&jump=" + reponseId + "&numberofpages=" + numberOfPages + "&fromUri=answer", UriKind.Relative));
            }
        }

        private void hfrrehostButton_Click(object sender, EventArgs e)
        {
            position = answerTextBox.SelectionStart.ToString();
            string actionHfrrehost;
            if (action == "edit") actionHfrrehost = "edit";
            else if (action == "quote") actionHfrrehost = "quote";
            else actionHfrrehost = "answer";
            NavigationService.Navigate(new Uri("/HFRrehost.xaml?idtopic=" + idTopic + "&idcat=" + idCat + "&pagenumber=" + pageNumber + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&action=" + actionHfrrehost + "&numberofpages=" + numberOfPages + "&position=" + position + "&numrep=" + numRep, UriKind.Relative));
        }

        private void smileyButton_Click(object sender, EventArgs e)
        {
            position = answerTextBox.SelectionStart.ToString();
            string actionSmiley;
            if (action == "edit") actionSmiley = "edit";
            else if (action == "quote") actionSmiley = "quote";
            else actionSmiley = "answer";
            NavigationService.Navigate(new Uri("/SmileyPage.xaml?idtopic=" + idTopic + "&idcat=" + idCat + "&pagenumber=" + pageNumber + "&topicname=" + HttpUtility.UrlEncode(topicName) + "&action=" + actionSmiley + "&numberofpages=" + numberOfPages + "&position=" + position + "&numrep=" + numRep, UriKind.Relative));
        }

        private void AnswerTopicPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // Sauvegarde de la réponse
            if (!posted)
            {
                store.Remove("saveAnswer");
                store.Remove("saveAnswerTopicNumber");
                store.Add("saveAnswer", answerTextBox.Text);
                store.Add("saveAnswerTopicNumber", idTopic); 
            }
        }

        private void AnswerTopicPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight)
            {
                answerScrollViewer.Height = 200;
            }
            else
            {
                answerScrollViewer.Height = 600;
            }
        }

        private void showQuoteButton_Click(object sender, RoutedEventArgs e)
        {
            string answerSave = answerTextBox.Text;
            answerTextBox.Text = quotedText + answerSave;
            showQuoteButton.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}