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
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

namespace HFR7
{
    public partial class NewMp : PhoneApplicationPage
    {
        CookieContainer container = new CookieContainer();
        System.IO.IsolatedStorage.IsolatedStorageSettings store = IsolatedStorageSettings.ApplicationSettings;
        string hash;
        string message;
        string pseudoDestiDirect;
        string pseudoDesti;
        string sujetMp;
        string urlPost = "";

        public NewMp()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Cookie
            container = store["HFRcookies"] as CookieContainer;
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

            // Hash check
            hash = store["hash"] as String;

            // GET
            NavigationContext.QueryString.TryGetValue("pseudodesti", out pseudoDestiDirect);
            NavigationContext.QueryString.TryGetValue("url", out urlPost);
            if (pseudoDestiDirect != null)
            {
                pseudoTextBox.Text = pseudoDestiDirect;
                pseudoTextBox.Hint = "";
            }
        }

        private void postMessageButton_Click(object sender, EventArgs e)
        {
            if (pseudoTextBox.Text == "" || messageTextBox.Text == "" || sujetTextBox.Text == "")
            {
                MessageBox.Show("Veuillez remplir tous les champs avant de poster.");
            }
            else
            {
                // Affichage de la progressbar
                progressBar.Visibility = System.Windows.Visibility.Visible;

                // On désactive les textbox
                pseudoTextBox.IsEnabled = false;
                messageTextBox.IsEnabled = false;
                sujetTextBox.IsEnabled = false;

                // On désactive l'appbar
                ApplicationBar.IsVisible = false;

                // Mise en forme du message
                message = Regex.Replace(messageTextBox.Text, "\r\n", "\r");
                message = Regex.Replace(message, "\r", Environment.NewLine);
                message = HttpUtility.UrlEncode(message);

                // Pseudo du destinataire
                pseudoDesti = pseudoTextBox.Text;

                // Sujet du MP
                sujetMp = HttpUtility.UrlEncode(sujetTextBox.Text);

                // Création de l'objet HttpWebRequest.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://forum.hardware.fr/bddpost.php");

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

        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // Arrêt de l'opération
            Stream postStream = request.EndGetRequestStream(asynchronousResult);

            // Requête
            string postData = "&MsgIcon=20&content_form=" + message + "&hash_check=" + hash + "&new=0&post=&cat=prive" + "&page=1&verifrequet=1100&p=1&sondage=0&owntopic=1&config=hfr.inc&emaill=0&pseudo=" + store["userPseudo"] + "&sujet=" + sujetMp + "&dest=" + pseudoDesti + "&signature=1";

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
                        if (readerToEnd.Contains("tentatives de flood")) errorMessage = "Afin d'éviter le flood, merci d'attendre 20 secondes avant d'envoyer un nouveau MP :o";
                        else if (readerToEnd.Contains("le pseudo suivant n'existe pas")) errorMessage = "Le pseudo \"" + pseudoDesti + "\" n'existe pas. Merci de vérifier l'orthographe.";
                        else errorMessage = "Erreur inconnue.";
                        MessageBox.Show("Erreur ! \n" + errorMessage);
                        ApplicationBar.IsVisible = true;
                        messageTextBox.IsEnabled = true;
                        sujetTextBox.IsEnabled = true;
                        pseudoTextBox.IsEnabled = true;
                    });
                }
                else
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Message envoyé avec succès !");
                        if (urlPost == null) NavigationService.Navigate(new Uri("/WelcomePage.xaml", UriKind.Relative));
                        else NavigationService.Navigate(new Uri(HttpUtility.UrlDecode(urlPost), UriKind.Relative));
                    });
                }
                // Cachage de la progressBar
            }
            catch
            {
                Dispatcher.BeginInvoke(() => MessageBox.Show("Erreur de connexion. Vérifiez l'état du serveur ou votre connectivité."));
            }
            Dispatcher.BeginInvoke(() => progressBar.Visibility = System.Windows.Visibility.Collapsed);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (urlPost != null)
            {
            //    
            //    NavigationService.Navigate(new Uri(HttpUtility.UrlDecode(urlPost), UriKind.Relative));
            }
            e.Cancel = true;
            store.Remove("navigatedback");
            store.Add("navigatedback", "true");
            NavigationService.GoBack();

        }

        private void hfrrehostButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/HFRrehostMP.xaml", UriKind.Relative));
        }
    }
}