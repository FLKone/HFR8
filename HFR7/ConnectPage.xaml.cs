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
using HtmlAgilityPack;
using System.Text;
using System.Windows.Media.Imaging;
using ImageTools.Helpers;
using ImageTools;
using HFR7.HFRClasses;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;

namespace HFR7
{
    public partial class ConnectPage : PhoneApplicationPage
    {
        string currentVersion = "3.2";
        CookieContainer container = new CookieContainer();
        CookieContainer containerDummy = new CookieContainer();
        IsolatedStorageSettings store = IsolatedStorageSettings.ApplicationSettings;
        IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();


        WebClient _webClient;

        public ConnectPage()
        {
            InitializeComponent();

            // Sauvegarde de l'avatar dans la mémoire du téléphone
            _webClient = new WebClient();
            _webClient.OpenReadCompleted += (s1, e1) =>
            {
                if (e1.Error == null)
                {
                    try
                    {
                        string fileName = store["userAvatarFile"] as string;
                        bool isSpaceAvailable = IsSpaceIsAvailable(e1.Result.Length);
                        if (isSpaceAvailable)
                        {
                            if (isoStore.FileExists(fileName))
                            {
                                isoStore.DeleteFile(fileName);
                            }
                            using (var isfs = new IsolatedStorageFileStream(fileName, FileMode.CreateNew, isoStore))
                            {
                                long fileLen = e1.Result.Length;
                                byte[] b = new byte[fileLen];
                                e1.Result.Read(b, 0, b.Length);
                                if (b[0] == 71 && b[1] == 73 && b[2] == 70)
                                {
                                    // File is a GIF, we need to convert it!
                                    var image = new ExtendedImage();
                                    var gifDecoder = new ImageTools.IO.Gif.GifDecoder();
                                    gifDecoder.Decode(image, new MemoryStream(b));
                                    image.WriteToStream(isfs, "avatar.png");

                                    Dispatcher.BeginInvoke(() =>
                                    {
                                        store.Remove("isConnected");
                                        store.Add("isConnected", "");
                                        NavigationService.Navigate(new Uri("/WelcomePage.xaml?from=connect", UriKind.Relative));
                                    });
                                }
                                else
                                {
                                    isfs.Write(b, 0, b.Length);
                                    isfs.Flush();
                                    Dispatcher.BeginInvoke(() =>
                                    {
                                        store.Remove("isConnected");
                                        store.Add("isConnected", "");
                                        NavigationService.Navigate(new Uri("/WelcomePage.xaml?from=connect", UriKind.Relative));
                                    });
                                }
                            }
                        }
                        else
                        {
                            Dispatcher.BeginInvoke(() => { MessageBox.Show("Erreur lors de la première connexion. La mémoire de votre terminal semble pleine."); });
                        }
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.BeginInvoke(() => { MessageBox.Show("Erreur n°1 dans la sauvegarde de l'avatar : " + ex.Message); });
                    }
                }
                else
                {
                    Dispatcher.BeginInvoke(() => { MessageBox.Show("Erreur n°2 dans la sauvegarde de l'avatar : " + e1.Error.Message); });
                }
            };
        }
        
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            SystemTray.ProgressIndicator.IsIndeterminate = true;
            SystemTray.ProgressIndicator.IsVisible = false;
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            // Pseudo HTML
            if (store.Contains("userPseudoHTML")) store.Remove("userPseudoHTML");
            store.Add("userPseudoHTML", HttpUtility.HtmlDecode(HttpUtility.UrlEncode(pseudoTextBox.Text)));

            // Pseudo
            if (store.Contains("userPseudo")) store.Remove("userPseudo");
            store.Add("userPseudo", HttpUtility.UrlEncode(pseudoTextBox.Text));

            // Mot de passe
            if (store.Contains("userPassword")) store.Remove("userPassword");
            store.Add("userPassword", passwordPasswordBox.Password);

            // Sauvegarde du store (bug WP8 ?)
            store.Save();

            // Vérification du remplissage correct des TextBox
            if (pseudoTextBox.Text == "") MessageBox.Show("Veuillez entrer un pseudo.");
            if (passwordPasswordBox.Password == "") MessageBox.Show("Veuillez entrer un mot de passe.");

            // Vérification de l'identité et retrieving du cookie
            if (pseudoTextBox.Text != "" && passwordPasswordBox.Password != "")
            {
                StartCookie();
            }
        }

        private void StartCookie()
        {
            connectButton.IsEnabled = false;

            // Création de l'objet HttpWebRequest.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://forum.hardware.fr/login_validation.php?config=hfr.inc");

            request.ContentType = "application/x-www-form-urlencoded";

            // Méthode en POST
            request.Method = "POST";

            // Ligne spécifique à SFR : headers EXPECT 100 CONTINUE
            request.Headers["User-Agent"] = "Mozilla /4.0 (compatible; MSIE 6.0; Windows CE; IEMobile 7.6) Vodafone/1.0/SFR_v1615/1.56.163.8.39";

            // Formattage des cookies
            request.Headers["Set-Cookie"] = "name=value";


            container.Add(new Uri("http://forum.hardware.fr/"), new Cookie("name", "value"));
            request.CookieContainer = container;

            // Démarrage de l'opération asynchrone
            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), request);
        }


        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            Dispatcher.BeginInvoke(() =>
            {
                SystemTray.ProgressIndicator.Text = "Connexion";
                SystemTray.ProgressIndicator.IsVisible = true;
            });
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // Arrêt de l'opération
            Stream postStream = request.EndGetRequestStream(asynchronousResult);

            // Requête
            string postData = "&pseudo=" + store["userPseudoHTML"] + "&password=" + store["userPassword"];

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
            Dispatcher.BeginInvoke(() =>
            {
                SystemTray.ProgressIndicator.Text = "Identification";
                SystemTray.ProgressIndicator.IsVisible = true;
            });

            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // Arrêt de l'opération
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);

                if (container.Count == 1)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Erreur dans l'identifiant ou le mot de passe.");
                        connectButton.IsEnabled = true;
                        SystemTray.ProgressIndicator.IsVisible = false;
                        response.Close();
                    });
                }

                if (container.Count == 4)
                {
                    // Extractions des cookies
                    List<Cookie> listCookies = new List<Cookie>();
                    foreach (Cookie cookie in response.Cookies)
                    {
                        listCookies.Add(cookie);
                    }
                    store.Remove("listHFRcookies");
                    store.Add("listHFRcookies", listCookies);

                    foreach (Cookie c in listCookies)
                    {
                        c.Expires = new DateTime(2999, 12, 31);
                        container.Add(new Uri("http://forum.hardware.fr", UriKind.Absolute), c);
                    }
                    store.Remove("HFRcookies");
                    store.Add("HFRcookies", container);

                    response.Close();

                    // Récupération du cookie dummy
                    StartDummyCookie();
                }
            }
            catch
            {
                Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Le serveur distant ne répond pas. Vérifiez votre connectivité ou l'état du serveur distant.");
                    connectButton.IsEnabled = true;
                    SystemTray.ProgressIndicator.IsVisible = false;
                });
            }
        }

        private void StartDummyCookie()
        {
            // Création de l'objet HttpWebRequest.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://forum.hardware.fr/login_validation.php?config=hfr.inc");

            request.ContentType = "application/x-www-form-urlencoded";

            // Méthode en POST
            request.Method = "POST";

            // Ligne spécifique à SFR : headers EXPECT 100 CONTINUE
            request.Headers["User-Agent"] = "Mozilla /4.0 (compatible; MSIE 6.0; Windows CE; IEMobile 7.6) Vodafone/1.0/SFR_v1615/1.56.163.8.39";

            // Formattage des cookies
            request.Headers["Set-Cookie"] = "name=value";


            containerDummy.Add(new Uri("http://forum.hardware.fr/"), new Cookie("name", "value"));
            request.CookieContainer = containerDummy;

            // Démarrage de l'opération asynchrone
            request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallbackDummy), request);
        }


        private void GetRequestStreamCallbackDummy(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // Arrêt de l'opération
            Stream postStream = request.EndGetRequestStream(asynchronousResult);

            // Requête
            string postData = "&pseudo=HFR7Agent&password=4eabc9eb8c";

            // Conversion du string en byte
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Enregistrement de la requête vers le string
            postStream.Write(byteArray, 0, postData.Length);
            postStream.Close();

            // Démarrage de l'opération asynchrone pour avoir la réponse
            request.BeginGetResponse(new AsyncCallback(GetResponseCallbackDummy), request);
        }


        private void GetResponseCallbackDummy(IAsyncResult asynchronousResult)
        {

            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            // Arrêt de l'opération
            try
            {

                Dispatcher.BeginInvoke(() =>
                {
                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);

                    if (containerDummy.Count == 1)
                    {
                        MessageBox.Show("Erreur dans l'identifiant ou le mot de passe du dummy. Contactez le développeur");
                        connectButton.IsEnabled = true;
                        SystemTray.ProgressIndicator.IsVisible = false;
                        response.Close();
                    }

                    if (containerDummy.Count == 4)
                    {
                        // Extractions des cookies
                        List<Cookie> listCookiesDummy = new List<Cookie>();
                        foreach (Cookie cookie in response.Cookies)
                        {
                            cookie.Expires = new DateTime(2999, 12, 31);
                            listCookiesDummy.Add(cookie);
                        }
                        store.Remove("listHFRcookiesDummy");
                        store.Add("listHFRcookiesDummy", listCookiesDummy);

                        foreach (Cookie c in listCookiesDummy)
                        {
                            c.Expires = new DateTime(2999, 12, 31);
                            containerDummy.Add(new Uri("http://forum.hardware.fr", UriKind.Absolute), c);
                        }
                        store.Remove("HFRcookiesDummy");
                        store.Add("HFRcookiesDummy", containerDummy);
                        response.Close();

                        // Lancement de la fonction de première connexion
                        PremiereConnexion();
                    }
                });
            }
            catch
            {
                Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Le serveur distant ne répond pas. Vérifiez votre connectivité ou l'état du serveur distant.");
                    connectButton.IsEnabled = true;
                    SystemTray.ProgressIndicator.IsVisible = false;
                });
            }
        }

        public void PremiereConnexion()
        {
            CookieContainer container = store["HFRcookies"] as CookieContainer;

            // Ajout des préférences
            AppSettings.CheckAndAdd(currentVersion);

            // Id utilisateur + Url avatar
            HtmlWeb.LoadAsync("http://forum.hardware.fr/", container, (s, args) =>
            {
                if (args.Document == null)
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show("Serveur en cours de maintenance. Veuillez réessayer plus tard.")); ;
                }
                else
                {
                    string[] userIdArray = args.Document.DocumentNode.Descendants("a").Where(x => (bool)x.GetAttributeValue("href", "").Contains("/user/allread.php?id_user=") == true).
                                                Select(y => y.GetAttributeValue("href", "")).ToArray();
                    store.Add("userId", Convert.ToInt32(userIdArray[0].Split('=')[1].Split('&')[0]));
                }

                HtmlWeb.LoadAsync("http://forum.hardware.fr/user/editprofil.php?config=hfr.inc&page=2", container, (t, argt) =>
                {
                    if (argt.Document == null)
                    {
                        Dispatcher.BeginInvoke(() => MessageBox.Show("Serveur en cours de maintenance. Veuillez réessayer plus tard.")); ;
                    }
                    else
                    {
                        string[] userCitation = argt.Document.DocumentNode.Descendants("input").Where(x => (string)x.GetAttributeValue("name", "") == "citation").
                                                    Select(y => y.GetAttributeValue("value", "")).ToArray();

                        store.Add("citation", userCitation[0]);
                    }


                    HtmlWeb.LoadAsync("http://forum.hardware.fr/user/editprofil.php?config=hfr.inc&page=5", container, (u, argu) =>
                    {
                        if (argu.Document == null)
                        {
                            Dispatcher.BeginInvoke(() => MessageBox.Show("Serveur en cours de maintenance. Veuillez réessayer plus tard.")); ;
                        }
                        else
                        {
                            string[] userAvatarFileArray = argu.Document.DocumentNode.Descendants("img").Where(x => (bool)x.GetAttributeValue("src", "").Contains("http://forum-images.hardware.fr/images/mesdiscussions-") == true).
                                                        Select(y => y.GetAttributeValue("src", "")).ToArray();
                            string[] userHash = argu.Document.DocumentNode.Descendants("input").Where(x => (bool)x.GetAttributeValue("name", "").Contains("hash_check") == true).
                                                        Select(y => y.GetAttributeValue("value", "")).ToArray();

                            store.Add("hash", userHash[0]);

                            if (userAvatarFileArray.Length != 0)
                            {
                                foreach (string line in userAvatarFileArray)
                                {
                                    store.Add("userAvatarFile", line.Split('/')[4]);
                                }
                                _webClient.OpenReadAsync(new Uri("http://forum-images.hardware.fr/images/" + store["userAvatarFile"] as string));
                            }
                            else
                            {
                                Dispatcher.BeginInvoke(() =>
                                {
                                    store.Remove("isConnected");
                                    store.Add("isConnected", "");
                                    NavigationService.Navigate(new Uri("/WelcomePage.xaml?from=connect", UriKind.Relative));
                                });
                            }
                        }
                    });
                });

            });
        }

        private bool IsSpaceIsAvailable(long spaceReq)
        {
            using (var ISFstore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                long spaceAvail = ISFstore.AvailableFreeSpace;
                if (spaceReq > spaceAvail)
                {
                    return false;
                }
                return true;
            }
        }
        public static WriteableBitmap ToBitmap(ImageBase image)
        {
            Guard.NotNull(image, "image");
            var bitmap = new WriteableBitmap(image.PixelWidth, image.PixelHeight);
            ImageBase temp = image;
            byte[] pixels = temp.Pixels;
            int[] raster = bitmap.Pixels;
            Buffer.BlockCopy(pixels, 0, raster, 0, pixels.Length);
            for (int i = 0; i < raster.Length; i++)
            {
                int abgr = raster[i];
                int a = (abgr >> 24) & 0xff;
                float m = a / 255f;
                int argb = a << 24 |
                           (int)((abgr & 0xff) * m) << 16 |
                           (int)(((abgr >> 8) & 0xff) * m) << 8 |
                           (int)(((abgr >> 16) & 0xff) * m);
                raster[i] = argb;
            }
            bitmap.Invalidate();
            return bitmap;
        }

    }
}