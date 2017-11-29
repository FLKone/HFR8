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
using System.IO.IsolatedStorage;
using HtmlAgilityPack;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Phone.Shell;
using ImageTools;
using ImageTools.Helpers;
using System.Globalization;
using HFR7.HFRClasses;
using Microsoft.Phone.Scheduler;
using System.Windows.Navigation;
using System.Xml.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Controls.Primitives;


namespace HFR7
{

    public partial class WelcomePage : PhoneApplicationPage
    {
        private static Version TargetedVersion = new Version(7, 10, 8858);
        public static bool isTargetedVersion { get { return Environment.OSVersion.Version >= TargetedVersion; } }

        // METTRE A JOUR AUSSI LA CONNECT PAGE !!!
        string currentVersion = "3.2";
        PeriodicTask periodicTask;
        // Variables locales
        CookieContainer container = new CookieContainer();
        CookieContainer containerDummy = new CookieContainer();
        IsolatedStorageSettings store = IsolatedStorageSettings.ApplicationSettings;
        List<HFRClasses.TopicFav> favObject = new List<HFRClasses.TopicFav>();
        List<HFRClasses.TopicFav> favObjectCache = new List<HFRClasses.TopicFav>();
        List<HFRClasses.Categories> catObject = new List<HFRClasses.Categories>();
        //List<HFRClasses.NotifTopics> notifTopicObject = new List<NotifTopics>();
        List<HFRClasses.Mp> mpObject = new List<HFRClasses.Mp>();
        List<HFRClasses.Mp> mpObjectCache = new List<HFRClasses.Mp>();
        IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        string currentOrientation;
        string pivotNumber;
        string fromUri;
        int nbFav;
        IEnumerable<GroupFav<HFRClasses.TopicFav>> favParCat;
        bool buttonFavPressed = false;
        bool favTopicsGroupViewOpened = false;
        bool catGroupViewOpened = false;
        bool buttonCatPressed = false;
        bool mpLoaded = false;
        List<string> listFileStore = new List<string>();

        public WelcomePage()
        {
            InitializeComponent();
            ShellTile appTile = ShellTile.ActiveTiles.First();
            if (appTile != null)
            {
                if (isTargetedVersion)
                {
                    // Get the new FlipTileData type.
                    Type flipTileDataType = Type.GetType("Microsoft.Phone.Shell.FlipTileData, Microsoft.Phone");

                    // Get the ShellTile type so we can call the new version of "Update" that takes the new Tile templates.
                    Type shellTileType = Type.GetType("Microsoft.Phone.Shell.ShellTile, Microsoft.Phone");

                    // Loop through any existing Tiles that are pinned to Start.
                    var tileToUpdate = ShellTile.ActiveTiles.First();


                    // Get the constructor for the new FlipTileData class and assign it to our variable to hold the Tile properties.
                    //var UpdateTileData = flipTileDataType.GetConstructor(new Type[] { }).Invoke(null);

                    // Set the properties. 
                    //SetProperty(UpdateTileData, "Title", "HFR7");
                    //SetProperty(UpdateTileData, "SmallBackgroundImage", new Uri("Images/SmallBackground.png", UriKind.Relative));
                    ////SetProperty(UpdateTileData, "BackgroundImage", new Uri("wp8_medium.png", UriKind.Relative));
                    //SetProperty(UpdateTileData, "BackBackgroundImage", new Uri("wp8_back_medium.png", UriKind.Relative));
                    //SetProperty(UpdateTileData, "WideBackgroundImage", new Uri("wp8_large.png", UriKind.Relative));
                    //SetProperty(UpdateTileData, "WideBackBackgroundImage", new Uri("wp8_back_large.png", UriKind.Relative));
                    //SetProperty(UpdateTileData, "WideBackContent", "Content for Wide Back Tile. Lots more text here.");

                    // Invoke the new version of ShellTile.Update.
                    //shellTileType.GetMethod("Update").Invoke(tileToUpdate, new Object[] { UpdateTileData });

                }
            }
        }

        private static void SetProperty(object instance, string name, object value)
        {
            var setMethod = instance.GetType().GetProperty(name).GetSetMethod();
            setMethod.Invoke(instance, new object[] { value });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            while (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }
            if (!store.Contains("currentVersion"))
            {
                NavigationService.Navigate(new Uri("/ConnectPage.xaml", UriKind.Relative));
            }
            else if ((string)store["currentVersion"] != currentVersion)
            {
                Update();
            }
            if (!store.Contains("isConnected"))
            {
                NavigationService.Navigate(new Uri("/ConnectPage.xaml", UriKind.Relative));
            }
            else
            {
                // Affichage de l'avatar depuis l'ISF
                using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.Contains("userAvatarFile"))
                    {
                        string fileName = store["userAvatarFile"] as string;
                        using (var isoStream = isoStore.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                        {
                            BitmapImage image = new BitmapImage();
                            image.SetSource(isoStream);
                            avatarImage.Source = image;
                        }
                    }
                }

                // Affichage du pseudo
                string userPseudo = store["userPseudo"] as string;
                userPseudo = HttpUtility.UrlDecode(userPseudo);
                pseudoTextBlock.Text = userPseudo;

                // Affichage de la citation perso
                string citationStore = store["citation"] as string;
                if (citationStore != "\"\"\"") citationTextBlock.Text = citationStore;
                string favorisType = store["favorisType"] as string;
                string displayImages = store["displayImages"] as string;
                string displayAvatars = store["displayAvatars"] as string;
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (store.Contains("stopRequest")) store.Remove("stopRequest");
                if ((string)store["disableLandscape"] == "true") WelcomePagePA.SupportedOrientations = SupportedPageOrientation.Portrait;
                else WelcomePagePA.SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
                if (store.Contains("listQuoteText")) store.Remove("listQuoteText");
                if (store.Contains("navigatedback")) store.Remove("navigatedback");

                //if (!isoStore.FileExists("notifications.xml"))
                //{
                //    XmlSerializer serializer = new XmlSerializer(notifTopicObject.GetType());
                //    using (var file = isoStore.OpenFile("notifications.xml", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                //    {
                //        using (var writer = new StreamWriter(file))
                //        {
                //            serializer.Serialize(writer.BaseStream, notifTopicObject);
                //        }
                //    }
                //}
                else isoStore.DeleteFile("notifications.xml");
                if (!(bool)store["launch"])
                {
                    SystemTray.ProgressIndicator = new ProgressIndicator();
                    SystemTray.ProgressIndicator.IsIndeterminate = true;
                    //store.Remove("navigatedback");

                    new Thread((ThreadStart)delegate
                    {
                        if (isoStore.DirectoryExists("topics"))
                        {
                            foreach (string fileName in isoStore.GetFileNames("topics/*"))
                            {
                                try
                                {
                                    isoStore.DeleteFile("topics/" + fileName);
                                }
                                catch { }
                            }
                        }
                    }).Start();

                    if ((string)store["refreshFavWP"] == "true")
                    {
                        AffichageFavoris();
                        AffichageCategories();
                    }
                }
                else
                {
                    SystemTray.ProgressIndicator = new ProgressIndicator();
                    SystemTray.ProgressIndicator.IsIndeterminate = true;
                    store.Remove("launch");
                    store.Add("launch", (bool)false);
                    NavigationContext.QueryString.TryGetValue("from", out fromUri);
                    if (fromUri == "connect")
                    {
                        NavigationService.RemoveBackEntry();
                    }

                    NavigationContext.QueryString.TryGetValue("pivot", out pivotNumber);
                    if (pivotNumber == "1") welcomePivot.SelectedIndex = 1;
                    if (pivotNumber == "2") welcomePivot.SelectedIndex = 2;
                    if (pivotNumber == "3") welcomePivot.SelectedIndex = 3;

                    // Suppression du cache
                    if (store.Contains("saveAnswer"))
                    {
                        store.Remove("saveAnswer");
                        store.Remove("saveAnswerTopicNumber");
                    }
                    if (store.Contains("activated")) store.Remove("activated");
                    if (isoStore.DirectoryExists("topics"))
                    {
                        foreach (string fileName in isoStore.GetFileNames("topics/*"))
                        {
                            // Cache
                            isoStore.DeleteFile("topics/" + fileName);
                        }
                    }

                    // Binding null
                    favTopics.ItemsSource = null;
                    mpListBox.ItemsSource = null;
                    categoriesGroup.ItemsSource = null;

                    // Tile principale (MP)
                    ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault();
                    StandardTileData NewAppTileData = new StandardTileData
                    {
                        Title = "hfr8",
                        BackTitle = "",
                        BackBackgroundImage = new Uri("", UriKind.Relative),
                        BackContent = ""
                    };
                    TileToFind.Update(NewAppTileData);

                    // Cookie
                    if (store.Contains("listHFRcookies") && store.Contains("listHFRcookiesDummy"))
                    {
                        // Création du cookie de l'user
                        List<Cookie> listCookies = store["listHFRcookies"] as List<Cookie>;
                        foreach (Cookie c in listCookies)
                        {
                            container.Add(new Uri("http://forum.hardware.fr", UriKind.Absolute), c);
                        }
                        store.Remove("HFRcookies");
                        store.Add("HFRcookies", container);

                        // Création du cookie dummy
                        List<Cookie> listCookiesDummy = store["listHFRcookiesDummy"] as List<Cookie>;
                        foreach (Cookie c in listCookiesDummy)
                        {
                            containerDummy.Add(new Uri("http://forum.hardware.fr", UriKind.Absolute), c);
                        }
                        store.Remove("HFRcookiesDummy");
                        store.Add("HFRcookiesDummy", containerDummy);

                        // Redirection vers le cache ou chargement des favoris
                        AffichageFavoris();
                        AffichageCategories();
                        StartPeriodicAgent();
                    }
                    else
                    {
                        MessageBox.Show("Erreur dans la récupération du cookie. Veuillez vous déconnecter puis vous reconnecter, ou bien réinstaller l'application");
                    }
                }
            });
        }

        private void StartPeriodicAgent()
        {
            if ((string)store["activateFavAgent"] == "true")
            {
                periodicTask = ScheduledActionService.Find("FavSchedulerAgent") as PeriodicTask;
                //if (periodicTask.LastExitReason.ToString() != "Completed" || periodicTask.LastExitReason.ToString() != "None") MessageBox.Show("Last exit reason : " + periodicTask.LastExitReason.ToString() + "; Last scheduled time : " + periodicTask.LastScheduledTime + "; Expiration Time : " + periodicTask.ExpirationTime + "; is scheduled : " + periodicTask.IsScheduled);

                // If the task already exists and the IsEnabled property is false, background
                // agents have been disabled by the user
                if (periodicTask != null && !periodicTask.IsEnabled)
                {
                    return;
                }

                // If the task already exists and background agents are enabled for the
                // application, you must remove the task and then add it again to update 
                // the schedule
                if (periodicTask != null && periodicTask.IsEnabled)
                {
                    RemoveAgent("FavSchedulerAgent");
                }

                periodicTask = new PeriodicTask("FavSchedulerAgent");

                // The description is required for periodic agents. This is the string that the user
                // will see in the background services Settings page on the device.
                periodicTask.Description = "Ce service permet de vérifier toutes les 30 minutes si un des sujets épinglés en page d'accueil a reçu une réponse, et actualise le nombre de favoris non lus sur la tuile hfr8. Il consomme environ 9 ko par requête.";
                periodicTask.ExpirationTime = DateTime.Now.AddDays(7);
                try
                {
                    ScheduledActionService.Add(periodicTask);
                }
                catch { }
                // L'agent va se charger 10 secondes après 
                // l'exécution de cette ligne de code
                // The agent will be launched 10 seconds
                // after this line of code is executed
                //ScheduledActionService.LaunchForTest("FavSchedulerAgent", TimeSpan.FromSeconds(10));
            }
        }
        private void RemoveAgent(string name)
        {
            try
            {
                ScheduledActionService.Remove(name);
            }
            catch (Exception)
            {
            }
        }

        public void AffichageFavoris()
        {
            // Affichage favoris
            IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication();
            if (store.Contains("mpnum")) store.Remove("mpnum");
            Dispatcher.BeginInvoke(() =>
            {
                SystemTray.ProgressIndicator.IsVisible = true;
                SystemTray.ProgressIndicator.Text = "Réception des favoris...";
                //globalTextblock.Visibility = System.Windows.Visibility.Visible;
                //globalTextblock.Text = "Réception des favoris...";
                ApplicationBar.IsVisible = true;
            });
            string urlFav = "http://forum.hardware.fr/forum1f.php?owntopic=" + (string)store["favorisType"];
            //string urlFav = "http://www.scrubs-fr.net/perso/hfr7/cat0/fav.htm";
            HtmlWeb.LoadAsync(urlFav, store["HFRcookies"] as CookieContainer, (s, args) =>
            {
                if (args.Error != null)
                {
                    if (args.Error.ToString().Contains("NotFound"))
                    {
                        Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("Impossible d'accéder au serveur. Vérifiez votre connectivité ou l'état de HFR.");
                            try
                            {
                                SystemTray.ProgressIndicator.IsVisible = false;
                            }
                            catch { }
                            //globalTextblock.Text = "erreur !";
                        });
                    }
                    else if (args.Error.ToString().Contains("NotFound"))
                    {
                        AffichageFavoris();
                    }
                }
                else
                {
                    int i = 0;
                    string[] favorisTopicNames = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "cCatTopic" && (bool)x.GetAttributeValue("title", "").Contains("Sujet") == true).
                                                Select(y => y.InnerText).ToArray();

                    string[] favorisTopicNumberOfPages = args.Document.DocumentNode.Descendants("td").Where(x => (string)x.GetAttributeValue("class", "") == "sujetCase4").
                                                Select(y => y.InnerText).ToArray();

                    string[] favorisTopicUri = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "cCatTopic" && (bool)x.GetAttributeValue("title", "").Contains("Sujet") == true).
                                                Select(y => y.GetAttributeValue("href", "")).ToArray();

                    string[] favorisLastPost = args.Document.DocumentNode.Descendants("td").Where(x => (bool)x.GetAttributeValue("class", "").Contains("sujetCase9") == true).
                                                Select(y => y.InnerText).ToArray();

                    string[] favorisIsHot = args.Document.DocumentNode.Descendants("img").Where(x => (string)x.GetAttributeValue("alt", "") == "Off" || (string)x.GetAttributeValue("alt", "") == "On").
                            Select(y => y.GetAttributeValue("alt", "")).ToArray();

                    string[] favorisBalise = args.Document.DocumentNode.Descendants("a").Where(x => (bool)x.GetAttributeValue("href", "").Contains("#t")).
                            Select(y => y.GetAttributeValue("href", "")).ToArray();

                    string[] mpArray = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "red" && (bool)x.GetAttributeValue("href", "").Contains("forum2")).
                                                Select(y => y.InnerText).ToArray();



                    string numberOfPagesTopicLine = "";
                    int j = 0;
                    favObject.Clear();
                    foreach (string line in favorisTopicNames)
                    {
                        if (favorisIsHot[i] == "On")
                        {
                            // Nombre de pages
                            if (favorisTopicNumberOfPages[i] != "&nbsp;") numberOfPagesTopicLine = favorisTopicNumberOfPages[i];
                            else numberOfPagesTopicLine = "1";

                            string topicCatId;
                            int firstTopicCatId = HttpUtility.HtmlDecode(favorisBalise[j]).IndexOf("&cat=") + "&cat=".Length;
                            int lastTopicCatId = HttpUtility.HtmlDecode(favorisBalise[j]).IndexOf("&", firstTopicCatId);
                            topicCatId = HttpUtility.HtmlDecode(favorisBalise[j]).Substring(firstTopicCatId, lastTopicCatId - firstTopicCatId);

                            string topicId;
                            int firstTopicId = HttpUtility.HtmlDecode(favorisBalise[j]).IndexOf("&post=") + "&post=".Length;
                            int lastTopicId = HttpUtility.HtmlDecode(favorisBalise[j]).LastIndexOf("&page");
                            topicId = HttpUtility.HtmlDecode(favorisBalise[j]).Substring(firstTopicId, lastTopicId - firstTopicId);

                            string reponseId;
                            int firstReponseId = HttpUtility.HtmlDecode(favorisBalise[j]).IndexOf("#t") + "#t".Length;
                            int lastReponseId = HttpUtility.HtmlDecode(favorisBalise[j]).Length;
                            reponseId = "rep" + HttpUtility.HtmlDecode(favorisBalise[j]).Substring(firstReponseId, lastReponseId - firstReponseId);

                            string pageNumber;
                            int firstPageNumber = HttpUtility.HtmlDecode(favorisBalise[j]).IndexOf("&page=") + "&page=".Length;
                            int lastPageNumber = HttpUtility.HtmlDecode(favorisBalise[j]).LastIndexOf("&p=");
                            pageNumber = HttpUtility.HtmlDecode(favorisBalise[j]).Substring(firstPageNumber, lastPageNumber - firstPageNumber);

                            // Formatage topic name
                            string topicNameFav;
                            topicNameFav = TopicNameShortener.Shorten(HttpUtility.HtmlDecode(line));

                            // Conversion date
                            string favorisSingleLastPostTimeString;
                            favorisSingleLastPostTimeString = Regex.Replace(Regex.Replace(HttpUtility.HtmlDecode(favorisLastPost[i].Substring(0, 28)), "à", ""), "-", "/");
                            DateTime favorisSingleLastPostDT;
                            favorisSingleLastPostDT = DateTime.Parse(favorisSingleLastPostTimeString, new CultureInfo("fr-FR"));
                            double favorisSingleLastPostTime;
                            favorisSingleLastPostTime = Convert.ToDouble(favorisSingleLastPostDT.ToFileTime());

                            // Nom du dernier posteur
                            string favorisLastPostUser;
                            favorisLastPostUser = HttpUtility.HtmlDecode(favorisLastPost[i].Substring(28, favorisLastPost[i].Length - 28));

                            // Temps depuis dernier post
                            string favorisLastPostText;
                            TimeSpan timeSpent;
                            timeSpent = DateTime.Now.Subtract(favorisSingleLastPostDT);
                            favorisLastPostText = HFRClasses.TimeSpentTopic.Run(timeSpent, favorisLastPostUser);
                            string testt = HFRClasses.GetCatName.PlainNameFromId(topicCatId);
                            favObject.Add(new HFRClasses.TopicFav()
                            {
                                TopicNameFav = topicNameFav,
                                TopicCatIdFav = topicCatId,
                                TopicIdFav = topicId,
                                TopicCatNameFav = HFRClasses.GetCatName.PlainNameFromId(topicCatId),
                                TopicUriFav = "/ReadTopic.xaml?idcat=" + topicCatId + "&idtopic=" + topicId + "&topicname=" + HttpUtility.UrlEncode(line) + "&pagenumber=" + pageNumber + "&jump=" + reponseId + "&numberofpages=" + numberOfPagesTopicLine,
                                TopicLastPostDateDouble = favorisSingleLastPostTime,
                                TopicLastPost = favorisLastPostText + " (p. " + pageNumber + "/" + numberOfPagesTopicLine + ")",
                                TopicNumberOfPages = numberOfPagesTopicLine,
                                TopicPageNumber = pageNumber,
                            });
                            j++;
                        }
                        i++;

                    }

                    // Affichage et binding
                    Dispatcher.BeginInvoke(() =>
                    {

                        //XmlSerializer serializer = new XmlSerializer(favObject.GetType());
                        //using (var file = appStorage.OpenFile("favoris.xml", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                        //{
                        //    using (var writer = new StreamWriter(file))
                        //    {
                        //        serializer.Serialize(writer.BaseStream, favObject);
                        //    }
                        //}
                        //drapalTileImage.Source = new BitmapImage(new Uri("Images/tilewelcomepage/drapal_num.png", UriKind.Relative));
                        nbFav = favObject.Count;
                        favorisTileTextBlock.Text = nbFav.ToString();
                        //if (favObject.Count < 10 && favObject.Count > 0)
                        //{
                        //    favorisTileTextBlock.Margin = new Thickness(50, 0, 0, 0);
                        //}

                        if (mpArray.Length != 0)
                        {
                            if (Convert.ToInt32(mpArray[0].Split(' ')[2]) > 0)
                            {
                                store.Add("mpnum", mpArray[0].Split(' ')[2]);
                                mpTileTextBlock.Text = mpArray[0].Split(' ')[2];
                                if (isoStore.FileExists("isMpNotified.txt")) isoStore.DeleteFile("isMpNotified.txt");
                                using (var file = isoStore.OpenFile("isMpNotified.txt", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                                {
                                    using (var writer = new StreamWriter(file))
                                    {
                                        writer.Write("true");
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (isoStore.FileExists("isMpNotified.txt")) isoStore.DeleteFile("isMpNotified.txt");
                            using (var file = isoStore.OpenFile("isMpNotified.txt", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                            {
                                using (var writer = new StreamWriter(file))
                                {
                                    writer.Write("false");
                                }
                            }
                            mpTileTextBlock.Text = "";
                        }

                        // Tri et binding

                        //favParCat = null;
                        //favTopics.ItemsSource = from topic in favObject
                        //            group topic by topic.TopicCatNameFav into c
                        //            select new GroupFav<HFRClasses.TopicFav>(c.Key, c);

                        //var bonsoir = from topic in favObject
                        //                        orderby topic.TopicCatNameFav
                        //                        group topic by topic.TopicCatNameFav into c
                        //                        select new GroupFav<HFRClasses.TopicFav>(c.Key, c);

                        List<GroupFav<TopicFav>> DataSource = GroupFav<TopicFav>.CreateGroups(favObject, System.Threading.Thread.CurrentThread.CurrentUICulture, (TopicFav tf) => { return tf.TopicCatNameFav; }, true);
                        favTopics.ItemsSource = DataSource;
                        ShowHideCatFav();
                        ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
                        try { SystemTray.ProgressIndicator.IsVisible = false; }
                        catch { }

                        //// Fav last
                        //var favLast = favObject.OrderBy(x => 1 / x.TopicLastPostDateDouble).ToList().Take(2);
                        //lastFavListBox.ItemsSource = favLast;

                        //// Départ des animations
                        //lastFavSB.Begin();
                    });


                    // Mises à jour des tiles topics
                    foreach (ShellTile tile in ShellTile.ActiveTiles)
                    {
                        if (tile.NavigationUri.ToString() != "/")
                        {
                            foreach (TopicFav fav in favObject)
                            {
                                string tilePostId;
                                int firstTilePostId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString()).IndexOf("&idtopic=") + "&idtopic=".Length;
                                int lastTilePostId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString()).IndexOf("&topicname=", firstTilePostId);
                                tilePostId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString().Substring(firstTilePostId, lastTilePostId - firstTilePostId));

                                string tileCatId;
                                int firstTileCatId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString()).IndexOf("?idcat=") + "?idcat=".Length;
                                int lastTileCatId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString()).IndexOf("&idtopic=", firstTileCatId);
                                tileCatId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString().Substring(firstTileCatId, lastTileCatId - firstTileCatId));
                                // Set the properties to update for the Application Tile.
                                // Empty strings for the text values and URIs will result in the property being cleared.
                                if (tilePostId == fav.TopicIdFav)
                                {
                                    StandardTileData NewTileData = new StandardTileData
                                    {
                                        BackContent = "Nouveaux messages !",
                                        BackgroundImage = new Uri("Images/tiles/" + HFRClasses.GetCatName.ShortNameFromId(tileCatId) + "_new.png", UriKind.Relative),
                                        BackBackgroundImage = new Uri("Images/tiles/unread.png", UriKind.Relative)

                                    };
                                    // Update the Application Tile
                                    tile.Update(NewTileData);
                                    break;
                                }
                                StandardTileData NewTileDataRead = new StandardTileData
                                {
                                    BackContent = "Pas de nouveaux messages.",
                                    BackgroundImage = new Uri("Images/tiles/" + HFRClasses.GetCatName.ShortNameFromId(tileCatId) + ".png", UriKind.Relative),
                                    BackBackgroundImage = new Uri("Images/tiles/read.png", UriKind.Relative)

                                };
                                // Update the Application Tile
                                tile.Update(NewTileDataRead);
                            }
                        }
                    }

                    // Mise à jour de la tile principale
                    ShellTile MainTileToFind = ShellTile.ActiveTiles.FirstOrDefault();

                    StandardTileData MainNewAppTileData = new StandardTileData
                    {
                        Title = "hfr8",
                        Count = nbFav
                    };
                    MainTileToFind.Update(MainNewAppTileData);
                }
            });
        }

        private void AffichageMp(bool refresh)
        {
            if ((mpListBox.ItemsSource == null || refresh == true) && !mpLoaded)
            {
                mpLoaded = true;
                mpObject.Clear();
                // Affichage Mp
                IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication();
                if (appStorage.FileExists("mp.xml")) appStorage.DeleteFile("mp.xml");
                Dispatcher.BeginInvoke(() =>
                {
                    SystemTray.ProgressIndicator.IsVisible = true;
                    SystemTray.ProgressIndicator.Text = "Chargement des messages privés...";
                });
                //http://forum.hardware.fr/forum1.php?config=hfr.inc&cat=prive&page=1
                string urlMp = "http://forum.hardware.fr/forum1.php?cat=prive&page=1";
                HtmlWeb.LoadAsync(urlMp, store["HFRcookies"] as CookieContainer, (s, args) =>
                {
                    if (args.Error != null)
                    {
                        if (args.Error.ToString().Contains("NotFound"))
                        {
                            Dispatcher.BeginInvoke(() =>
                            {
                                MessageBox.Show("Impossible d'accéder au serveur. Vérifiez votre connectivité ou l'état de HFR.");
                                try { SystemTray.ProgressIndicator.IsVisible = false; }
                                catch { }
                                //globalTextblock.Text = "erreur !";
                            });
                        }
                        else if (args.Error.ToString().Contains("WebException"))
                        {
                            AffichageFavoris();
                        }
                    }
                    else
                    {
                        int i = 0;
                        string[] mpTitle = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "cCatTopic" && (bool)x.GetAttributeValue("title", "").Contains("Sujet") == true).
                                                    Select(y => y.InnerText).ToArray();

                        string[] mpNumberOfPages = args.Document.DocumentNode.Descendants("td").Where(x => (string)x.GetAttributeValue("class", "") == "sujetCase4").
                                                    Select(y => y.InnerText).ToArray();

                        string[] mpSender = args.Document.DocumentNode.Descendants("td").Where(x => (bool)x.GetAttributeValue("class", "").Contains("sujetCase6")).
                                                    Select(y => y.InnerText).ToArray();

                        string[] mpUri = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "cCatTopic" && (bool)x.GetAttributeValue("title", "").Contains("Sujet") == true).
                                                    Select(y => y.GetAttributeValue("href", "")).ToArray();

                        string[] mpLastPost = args.Document.DocumentNode.Descendants("td").Where(x => (bool)x.GetAttributeValue("class", "").Contains("sujetCase9") == true).
                                                    Select(y => y.InnerText).ToArray();

                        string[] mpIsUnRead = args.Document.DocumentNode.Descendants("img").Where(x => ((string)x.GetAttributeValue("alt", "") == "Off" || (string)x.GetAttributeValue("alt", "") == "On") && (bool)x.GetAttributeValue("title", "").Contains("ouveau message")).
                                Select(y => y.GetAttributeValue("alt", "")).ToArray();

                        //string[] favorisBalise = args.Document.DocumentNode.Descendants("a").Where(x => (bool)x.GetAttributeValue("href", "").Contains("#t")).
                        //        Select(y => y.GetAttributeValue("href", "")).ToArray();


                        string numberOfPagesMpLine = "";
                        foreach (string line in mpTitle)
                        {
                            // Nombre de pages
                            if (mpNumberOfPages[i] != "&nbsp;") numberOfPagesMpLine = mpNumberOfPages[i];
                            else numberOfPagesMpLine = "1";

                            // Id Mp
                            string mpId;
                            int firstMpId = HttpUtility.HtmlDecode(mpUri[i]).IndexOf("&post=") + "&post=".Length;
                            int lastMpId = HttpUtility.HtmlDecode(mpUri[i]).LastIndexOf("&page");
                            mpId = HttpUtility.HtmlDecode(mpUri[i]).Substring(firstMpId, lastMpId - firstMpId);

                            // Formatage topic name
                            string mpTitleLine;
                            mpTitleLine = TopicNameShortener.Shorten(HttpUtility.HtmlDecode(line));

                            // Conversion date
                            string mpSingleLastPostTimeString;
                            mpSingleLastPostTimeString = Regex.Replace(Regex.Replace(HttpUtility.HtmlDecode(mpLastPost[i].Substring(0, 28)), "à", ""), "-", "/");
                            DateTime mpSingleLastPostDT;
                            mpSingleLastPostDT = DateTime.Parse(mpSingleLastPostTimeString, new CultureInfo("fr-FR"));
                            double mpSingleLastPostTime;
                            mpSingleLastPostTime = Convert.ToDouble(mpSingleLastPostDT.ToFileTime());

                            // Nom du dernier posteur
                            string mpLastPostUser;
                            mpLastPostUser = HttpUtility.HtmlDecode(mpLastPost[i].Substring(28, mpLastPost[i].Length - 28));

                            // Nom de l'expéditeur
                            string mpSenderLine;
                            mpSenderLine = mpSender[i];

                            // Temps depuis dernier post
                            string mpLastPostText;
                            TimeSpan timeSpent;
                            timeSpent = DateTime.Now.Subtract(mpSingleLastPostDT);
                            mpLastPostText = HFRClasses.MpDate.Run(mpSingleLastPostDT);

                            // Ajout à l'objet
                            //public string MpTitle { get; set; }
                            //public string MpId { get; set; }
                            //public string MpSender { get; set; }
                            //public string MpNumberOfPages { get; set; }
                            //public double MpPage { get; set; }
                            mpObject.Add(new HFRClasses.Mp()
                            {
                                MpTitle = mpTitleLine,
                                MpId = mpId,
                                MpSender = mpSenderLine,
                                MpNumberOfPages = mpNumberOfPages[i],
                                MpUri = "/Mp.xaml?senderpseudo=" + mpSenderLine + "&mpid=" + mpId + "&numberofpages=" + numberOfPagesMpLine + "&sujet=" + mpTitleLine,
                                MpPage = 0,
                                MpIsUnRead = mpIsUnRead[i],
                                MpLastPostText = mpLastPostText,
                                MpLastPostDateDouble = mpSingleLastPostTime
                            });
                            i++;

                        }
                        Dispatcher.BeginInvoke(() =>
                        {

                            //XmlSerializer serializer = new XmlSerializer(mpObject.GetType());
                            //using (var file = appStorage.OpenFile("mp.xml", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                            //{
                            //    using (var writer = new StreamWriter(file))
                            //    {
                            //        serializer.Serialize(writer.BaseStream, mpObject);
                            //    }
                            //}


                            // Tri et binding
                            mpListBox.ItemsSource = null;
                            var mpParDate = mpObject.OrderBy(x => 1 / x.MpLastPostDateDouble).ToList();
                            mpListBox.ItemsSource = mpParDate;
                            ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;

                            try { SystemTray.ProgressIndicator.IsVisible = false; }
                            catch { }
                            //globalTextblock.Text = "";
                        });
                    }
                });
            }
            //if (pivotNumber == "3")
            //{
            //    pivotNumber = "";
            //    if (isoStore.FileExists("mp.xml"))
            //    {
            //        XmlSerializer serializer = new XmlSerializer(mpObject.GetType());
            //        using (var file = isoStore.OpenFile("mp.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            //        {
            //            using (var reader = new StreamReader(file))
            //            {

            //                object deserialized = serializer.Deserialize(reader.BaseStream);
            //                mpObjectCache = (List<HFRClasses.Mp>)deserialized;

            //                // Tri et binding
            //                var mpParDate = mpObjectCache.OrderBy(x => 1 / x.MpLastPostDateDouble).ToList();


            //                Dispatcher.BeginInvoke(() =>
            //                {
            //                    //Binding
            //                    mpListBox.ItemsSource = mpParDate;

            //                    // Progress bar
            //                    globalProgressBar.Visibility = Visibility.Collapsed;
            //                    globalTextblock.Text = "";

            //                });
            //            }
            //        }
            //    }
            //}
        }


        private void AffichageCategories()
        {
            // Vérification si présence du cache
            var appStorage = IsolatedStorageFile.GetUserStoreForApplication();
            if (!appStorage.FileExists("categories.xml"))
            {

                // Récupération du cookie
                CookieContainer container = new CookieContainer();
                if (store.Contains("HFRcookies"))
                {
                    container = store["HFRcookies"] as CookieContainer;
                }
                else
                {
                    container = null;
                }

                //Chargement des catégories vers l'objet + hash
                //HtmlWeb.LoadAsync("http://www.scrubs-fr.net/perso/hfr7/cat0/home.htm", container, (s, args) =>
                HtmlWeb.LoadAsync("http://forum.hardware.fr/", container, (s, args) =>
                {
                    if (args.Document == null)
                    {
                        Dispatcher.BeginInvoke(() => MessageBox.Show("Serveur en cours de maintenance. Veuillez réessayer plus tard.")); ;
                    }
                    else
                    {
                        string[] nameCategories = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "cCatTopic").
                                                    Select(y => y.InnerText).ToArray();

                        string[] nameSousCategories = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "Tableau" && (bool)x.GetAttributeValue("href", "").Contains("/hfr/") == true).
                                                     Select(x => x.InnerText).ToArray();

                        string[] catSousCategories = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "Tableau" && (bool)x.GetAttributeValue("href", "").Contains("/hfr/") == true).
                                                     Select(x => x.GetAttributeValue("href", "").Split('/')[2]).ToArray();

                        string[] uriSousCategorie = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "Tableau" && (bool)x.GetAttributeValue("href", "").Contains("/hfr/") == true).
                                                     Select(x => x.GetAttributeValue("href", "")).ToArray();

                        string[] idCategories = args.Document.DocumentNode.Descendants("tr").Where(x => (string)x.GetAttributeValue("class", "") == "cat cBackCouleurTab1").
                                                     Select(x => x.GetAttributeValue("id", "").Split('t')[1]).ToArray();

                        int i = 0;
                        string CategorieNameValue = "";
                        string ancienneCatLue = "";
                        string idCategorieValue = "";
                        foreach (string line in nameSousCategories)
                        {
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "Hardware") { CategorieNameValue = "Hardware"; idCategorieValue = idCategories[0]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "HardwarePeripheriques") { CategorieNameValue = "Périphériques"; idCategorieValue = idCategories[1]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "OrdinateursPortables") { CategorieNameValue = "PC portables"; idCategorieValue = idCategories[2]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "gsmgpspda") { CategorieNameValue = "Techno. Mobiles"; idCategorieValue = idCategories[3]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "OverclockingCoolingModding") { CategorieNameValue = "Modding"; idCategorieValue = idCategories[4]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "apple") { CategorieNameValue = "Apple"; idCategorieValue = idCategories[5]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "VideoSon") { CategorieNameValue = "Vidéo & Son"; idCategorieValue = idCategories[6]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "Photonumerique") { CategorieNameValue = "Photo Numérique"; idCategorieValue = idCategories[7]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "JeuxVideo") { CategorieNameValue = "Jeux-vidéo"; idCategorieValue = idCategories[8]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "WindowsSoftware") { CategorieNameValue = "Windows & Software"; idCategorieValue = idCategories[9]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "reseauxpersosoho") { CategorieNameValue = "Réseaux grand public"; idCategorieValue = idCategories[10]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "systemereseauxpro") { CategorieNameValue = "Réseaux Pro"; idCategorieValue = idCategories[11]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "OSAlternatifs") { CategorieNameValue = "OS Alternatifs"; idCategorieValue = idCategories[12]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "Programmation") { CategorieNameValue = "Programmation"; idCategorieValue = idCategories[13]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "Graphisme") { CategorieNameValue = "Graphisme"; idCategorieValue = idCategories[14]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "AchatsVentes") { CategorieNameValue = "Achats & Ventes"; idCategorieValue = idCategories[15]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "EmploiEtudes") { CategorieNameValue = "Emploi & Etudes"; idCategorieValue = idCategories[16]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "Setietprojetsdistribues") { CategorieNameValue = "Projets distribués"; idCategorieValue = idCategories[17]; }
                            if (HttpUtility.HtmlDecode(catSousCategories[i]) == "Discussions") { CategorieNameValue = "Discussions"; idCategorieValue = idCategories[18]; }

                            if (HttpUtility.HtmlDecode(catSousCategories[i]) != ancienneCatLue) catObject.Add(new HFRClasses.Categories()
                            {
                                CategorieNameCat = CategorieNameValue,
                                SousCategorieNameCat = "Toutes",
                                SousCategorieUriStringCat = "/ListTopics.xaml?souscaturi=/hfr/" + HttpUtility.HtmlDecode(catSousCategories[i]) + "/liste_sujet-1.htm&listpagenumber=1&souscatname=" + CategorieNameValue + "&idcat=" + idCategorieValue + "&from=categorielist"
                            });

                            ancienneCatLue = HttpUtility.HtmlDecode(catSousCategories[i]);
                            catObject.Add(new HFRClasses.Categories() { CategorieNameCat = CategorieNameValue, SousCategorieNameCat = HttpUtility.HtmlDecode(line), SousCategorieUriStringCat = "/ListTopics.xaml?souscaturi=" + uriSousCategorie[i] + "&listpagenumber=1&souscatname=" + CategorieNameValue + "&idcat=" + idCategorieValue + "&from=categorielist" });
                            //docCategories.Root.Add(sousCategorie);

                            // Indentation
                            i++;
                        }
                        if (nameCategories.Contains("Section réservée aux modérateurs / admins / superadmins")) catObject.Add(new HFRClasses.Categories() { CategorieNameCat = "cat0", SousCategorieNameCat = "Toutes", SousCategorieUriStringCat = "/ListTopics.xaml?souscaturi=" + HttpUtility.UrlEncode("/forum1.php?cat=0") + "&listpagenumber=1&souscatname=Toutes&idcat=0&from=categorielist" });

                        Dispatcher.BeginInvoke(() =>
                        {
                            List<GroupCat<Categories>> DataSource = GroupCat<Categories>.CreateGroups(catObject, System.Threading.Thread.CurrentThread.CurrentUICulture, (Categories cat) => { return cat.CategorieNameCat; }, true);
                            this.categoriesGroup.ItemsSource = DataSource;
                            // Serialization
                            XmlSerializer serializer = new XmlSerializer(catObject.GetType());
                            using (var file = appStorage.OpenFile("categories.xml", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                            {
                                using (var writer = new StreamWriter(file))
                                {
                                    serializer.Serialize(writer.BaseStream, catObject);
                                }
                            }
                        });
                        try { SystemTray.ProgressIndicator.IsVisible = false; }
                        catch { }
                    }
                });
            }
            else
            {
                XmlSerializer serializer = new XmlSerializer(catObject.GetType());
                using (var file = appStorage.OpenFile("categories.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    using (var reader = new StreamReader(file))
                    {
                        // Déserialization
                        object deserialized = serializer.Deserialize(reader.BaseStream);
                        catObject = (List<HFRClasses.Categories>)deserialized;
                        // Tri
                        //var souscatParCat = from categorie in catObject
                        //                    group categorie by categorie.CategorieNameCat into c
                        //                    select new GroupCat<HFRClasses.Categories>(c.Key, c);

                        // Binding
                        List<GroupCat<Categories>> DataSource = GroupCat<Categories>.CreateGroups(catObject, System.Threading.Thread.CurrentThread.CurrentUICulture, (Categories cat) => { return cat.CategorieNameCat; }, true);

                        Dispatcher.BeginInvoke(() =>
                        {
                            this.categoriesGroup.ItemsSource = DataSource;
                        });
                    }
                }
                try { SystemTray.ProgressIndicator.IsVisible = false; }
                catch { }
            }
        }

        public void DeleteAllStorage()
        {
            // On supprime tout
            foreach (string fileName in isoStore.GetFileNames("*"))
            {
                try
                {
                    isoStore.DeleteFile(fileName);
                }
                catch { }
            };
            foreach (var line in store) listFileStore.Add(line.Key.ToString());
            if (listFileStore != null) foreach (string line in listFileStore) store.Remove(line);
        }

        public void Update()
        {
            // Supprimer les nouveaux éléments du store lors d'une MAJ
            try
            {
                if (isoStore.FileExists("categories.xml")) isoStore.DeleteFile("categories.xml");
                store.Add("vibrateLoad", "true");
            }
            catch { }
            AppSettings.CheckAndAdd(currentVersion);
            MessageBox.Show("L'installation de la version hfr8 " + currentVersion + " s'est effectuée avec succès ! Vous n'avez pas besoin de vous reconnecter et vos paramètres sont sauvegardés :)");
        }


        //private void AffichageFavorisCache()
        //{
        //    IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication();
        //    // Déserialization favoris
        //    if (appStorage.FileExists("favoris.xml"))
        //    {
        //        XmlSerializer serializer = new XmlSerializer(favObject.GetType());
        //        using (var file = appStorage.OpenFile("favoris.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read))
        //        {
        //            using (var reader = new StreamReader(file))
        //            {

        //                object deserialized = serializer.Deserialize(reader.BaseStream);
        //                favObjectCache = (List<HFRClasses.TopicFav>)deserialized;
        //                // Tri
        //                var favParCatCache = from topic in favObjectCache
        //                                     group topic by topic.TopicCatNameFav into c
        //                                     select new GroupFav<HFRClasses.TopicFav>(c.Key, c);

        //                var favLast = favObjectCache.OrderBy(x => 1 / x.TopicLastPostDateDouble).ToList().Take(3);

        //                Dispatcher.BeginInvoke(() =>
        //                {
        //                    // Affichage nombre
        //                    //drapalTileImage.Source = new BitmapImage(new Uri("Images/tilewelcomepage/drapal_num.png", UriKind.Relative));
        //                    favorisTileTextBlock.Text = favObjectCache.Count.ToString();
        //                    if (store.Contains("mpnum")) mpTileTextBlock.Text = store["mpnum"] as string;

        //                    //if (favObject.Count < 10 && favObject.Count > 0)
        //                    //{
        //                    //    favorisTileTextBlock.Margin = new Thickness(50, 0, 0, 0);
        //                    //}

        //                    // Binding
        //                    favTopics.ItemsSource = favParCatCache;
        //                    lastFavListBox.ItemsSource = favLast;

        //                    // Animations
        //                    lastFavSB.Begin();

        //                });
        //            }
        //        }
        //    }
        //}

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (favTopicsGroupViewOpened == true)
            {
                e.Cancel = true;
            }
            else if (catGroupViewOpened == true)
            {
                e.Cancel = true;
                WelcomePagePA.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                foreach (JournalEntry entry in NavigationService.BackStack)
                {
                    NavigationService.RemoveBackEntry();
                }
                NavigationService.RemoveBackEntry();
            }
        }


        public class GroupFav<T> : List<T>
        {
            public delegate string GetKeyDelegate(T item);
            public string Key { get; private set; }

            public GroupFav(string key)
            {
                Key = key;
            }

            private static List<GroupFav<T>> CreateGroups(Microsoft.Phone.Globalization.SortedLocaleGrouping slg)
            {
                List<GroupFav<T>> list = new List<GroupFav<T>>();
                return list;
            }

            public static List<GroupFav<TopicFav>> CreateGroups(List<TopicFav> items, CultureInfo ci, GetKeyDelegate getKey, bool sort)
            {
                Microsoft.Phone.Globalization.SortedLocaleGrouping slg = new Microsoft.Phone.Globalization.SortedLocaleGrouping(ci);
                List<GroupFav<TopicFav>> list = new List<GroupFav<TopicFav>>();
                list.Add(new GroupFav<TopicFav>(" "));
                list.Add(new GroupFav<TopicFav>("Hardware"));
                list.Add(new GroupFav<TopicFav>("Périphériques"));
                list.Add(new GroupFav<TopicFav>("PC portables"));
                list.Add(new GroupFav<TopicFav>("Techno. Mobiles"));
                list.Add(new GroupFav<TopicFav>("Modding"));
                list.Add(new GroupFav<TopicFav>("Apple"));
                list.Add(new GroupFav<TopicFav>("Vidéo & Son"));
                list.Add(new GroupFav<TopicFav>("Photo Numérique"));
                list.Add(new GroupFav<TopicFav>("Jeux-vidéo"));
                list.Add(new GroupFav<TopicFav>("Windows & Software"));
                list.Add(new GroupFav<TopicFav>("Réseaux grand public"));
                list.Add(new GroupFav<TopicFav>("Réseaux Pro"));
                list.Add(new GroupFav<TopicFav>("OS Alternatifs"));
                list.Add(new GroupFav<TopicFav>("Programmation"));
                list.Add(new GroupFav<TopicFav>("Graphisme"));
                list.Add(new GroupFav<TopicFav>("Achats & Ventes"));
                list.Add(new GroupFav<TopicFav>("Emploi & Etudes"));
                list.Add(new GroupFav<TopicFav>("Projets distribués"));
                list.Add(new GroupFav<TopicFav>("Discussions"));

                items.Add(new TopicFav()
                    {
                        TopicNameFav = " ",
                        TopicCatIdFav = " ",
                        TopicIdFav = " ",
                        TopicCatNameFav = " ",
                        TopicUriFav = "",
                        TopicLastPostDateDouble = 0,
                        TopicLastPost = "",
                        TopicNumberOfPages = "",
                        TopicPageNumber = "",
                    });
                foreach (TopicFav item in items)
                {
                    int i = 0;
                    int index = -1;
                    foreach (var it in list)
                    {
                        string currentKey = it.Key;
                        if (currentKey == item.TopicCatNameFav)
                        {
                            index = i;
                        }
                        i++;
                    }
                        if (index!=-1) list[index].Add(item);
                    }
                return list;
            }
        }

        private void Deconnexion()
        {
            MessageBoxResult m = MessageBox.Show("Êtes-vous sûr de vouloir vous déconnecter ? Cela supprimera toutes vos préférences et videra le cache.", "", MessageBoxButton.OKCancel);
            if (m == MessageBoxResult.Cancel) { }
            if (m == MessageBoxResult.OK)
            {
                DeleteAllStorage();
                NavigationService.Navigate(new Uri("/ConnectPage.xaml", UriKind.Relative));
            }
        }

        public class GroupCat<T> : List<T>
        {
            public delegate string GetKeyDelegate(T item);
            public string Key { get; private set; }

            public GroupCat(string key)
            {
                Key = key;
            }

            private static List<GroupCat<T>> CreateGroups(Microsoft.Phone.Globalization.SortedLocaleGrouping slg)
            {
                List<GroupCat<T>> list = new List<GroupCat<T>>();
                return list;
            }

            public static List<GroupCat<Categories>> CreateGroups(List<Categories> items, CultureInfo ci, GetKeyDelegate getKey, bool sort)
            {
                Microsoft.Phone.Globalization.SortedLocaleGrouping slg = new Microsoft.Phone.Globalization.SortedLocaleGrouping(ci);
                List<GroupCat<Categories>> list = new List<GroupCat<Categories>>();
                list.Add(new GroupCat<Categories>(" "));
                list.Add(new GroupCat<Categories>("Hardware"));
                list.Add(new GroupCat<Categories>("Périphériques"));
                list.Add(new GroupCat<Categories>("PC portables"));
                list.Add(new GroupCat<Categories>("Techno. Mobiles"));
                list.Add(new GroupCat<Categories>("Modding"));
                list.Add(new GroupCat<Categories>("Apple"));
                list.Add(new GroupCat<Categories>("Vidéo & Son"));
                list.Add(new GroupCat<Categories>("Photo Numérique"));
                list.Add(new GroupCat<Categories>("Jeux-vidéo"));
                list.Add(new GroupCat<Categories>("Windows & Software"));
                list.Add(new GroupCat<Categories>("Réseaux grand public"));
                list.Add(new GroupCat<Categories>("Réseaux Pro"));
                list.Add(new GroupCat<Categories>("OS Alternatifs"));
                list.Add(new GroupCat<Categories>("Programmation"));
                list.Add(new GroupCat<Categories>("Graphisme"));
                list.Add(new GroupCat<Categories>("Achats & Ventes"));
                list.Add(new GroupCat<Categories>("Emploi & Etudes"));
                list.Add(new GroupCat<Categories>("Projets distribués"));
                list.Add(new GroupCat<Categories>("Discussions"));

                items.Add(new Categories()
                {
                    CategorieNameCat = " ",
                    SousCategorieNameCat = "",
                    SousCategorieUriStringCat = ""
                });
                foreach (Categories item in items)
                {
                    int i = 0;
                    int index = -1;
                    foreach (var it in list)
                    {
                        string currentKey = it.Key;
                        if (currentKey == item.CategorieNameCat)
                        {
                            index = i;
                        }
                        i++;
                    }
                    if (index != -1) list[index].Add(item);
                }
                return list;
            }
        }

        //public class GroupCat<T> : IEnumerable<T>
        //{
        //    public GroupCat(string name, IEnumerable<T> items)
        //    {
        //        this.TitleCat = name;
        //        this.ItemsCat = new List<T>(items);
        //    }

        //    public override bool Equals(object obj)
        //    {
        //        GroupCat<T> that = obj as GroupCat<T>;

        //        return (that != null) && (this.TitleCat.Equals(that.TitleCat));
        //    }

        //    public string TitleCat
        //    {
        //        get;
        //        set;
        //    }

        //    public IList<T> ItemsCat
        //    {
        //        get;
        //        set;
        //    }

        //    #region IEnumerable<T> Members

        //    public IEnumerator<T> GetEnumerator()
        //    {
        //        return this.ItemsCat.GetEnumerator();
        //    }

        //    #endregion

        //    #region IEnumerable Members

        //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //    {
        //        return this.ItemsCat.GetEnumerator();
        //    }

        //    #endregion
        //}

        private void WelcomePagePA_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation.ToString().Contains("Landscape"))
            {

                pseudoTextBlock.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {

                pseudoTextBlock.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void Deconnexion_Click(object sender, RoutedEventArgs e)
        {
            Deconnexion();
        }

        private void UpdateAvatar(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator.IsVisible = true;
            WebClient avatarWebClient = new WebClient();
            avatarWebClient.OpenReadCompleted += new OpenReadCompletedEventHandler(avatarWebClient_OpenReadCompleted);
            avatarWebClient.OpenReadAsync(new Uri("http://forum-images.hardware.fr/images/" + store["userAvatarFile"] as string));

        }

        void avatarWebClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                try
                {
                    string fileName = store["userAvatarFile"] as string;
                    if (isoStore.FileExists(fileName))
                    {
                        isoStore.DeleteFile(fileName);
                    }
                    using (var isfs = new IsolatedStorageFileStream(fileName, FileMode.CreateNew, isoStore))
                    {
                        long fileLen = e.Result.Length;
                        byte[] b = new byte[fileLen];
                        e.Result.Read(b, 0, b.Length);
                        if (b[0] == 71 && b[1] == 73 && b[2] == 70)
                        {
                            // File is a GIF, we need to convert it!
                            var image = new ExtendedImage();
                            var gifDecoder = new ImageTools.IO.Gif.GifDecoder();
                            gifDecoder.Decode(image, new MemoryStream(b));
                            image.WriteToStream(isfs, "avatar.png");

                        }
                        else
                        {
                            isfs.Write(b, 0, b.Length);
                            isfs.Flush();
                        }
                        Dispatcher.BeginInvoke(() =>
                        {
                            using (var isoStream = isoStore.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                            {
                                BitmapImage image = new BitmapImage();
                                image.SetSource(isoStream);
                                avatarImage.Source = image;
                                SystemTray.ProgressIndicator.IsVisible = false;
                            }

                        });
                    }

                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(() => { MessageBox.Show("Erreur n°1 dans la sauvegarde de l'avatar : " + ex.Message); });
                }
            }
            else
            {
                Dispatcher.BeginInvoke(() => { MessageBox.Show("Erreur n°2 dans la sauvegarde de l'avatar : " + e.Error.Message); });
            }
        }


        private void welcomePivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (welcomePivot.SelectedIndex != 2 && store.Contains("HFRcookies"))
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = true;
            }
            else
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
            }

            if (welcomePivot.SelectedIndex == 3)
            {
                AffichageMp(false);
            }
        }

        private void appbar_refresh_Click(object sender, EventArgs e)
        {
            if (welcomePivot.SelectedIndex != 3)
            {
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                AffichageFavoris();
            }
            else
            {
                mpLoaded = false;
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IsEnabled = false;
                AffichageMp(true);
            }
        }

        private void deconnexionMenu_Click(object sender, EventArgs e)
        {
            Deconnexion();
        }

        private void aboutMenu_Click(object sender, EventArgs e)
        {
            XDocument docAbout = XDocument.Load("about.xml");
            string about = docAbout.Descendants("root").First().Value;
            about = Regex.Replace(about, "#CURRENTVERSION", "v" + currentVersion);
            MessageBox.Show(about);
        }

        private void appbar_settings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        private void PinTopicClick(object sender, RoutedEventArgs e)
        {
            TopicFav topicFavObject = ((sender as MenuItem).DataContext) as TopicFav;
            string backgroundImageTile;
            // Create the Tile object and set some initial properties for the Tile.
            ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(topicFavObject.TopicUriFav));
            if (TileToFind == null)
            {
                backgroundImageTile = HFRClasses.GetCatName.ShortNameFromId(topicFavObject.TopicCatIdFav);

                // The Count value of 12 shows the number 12 on the front of the Tile. Valid values are 1-99.
                // A Count value of 0 indicates that the Count should not be displayed.
                StandardTileData NewTileData = new StandardTileData
                {
                    BackgroundImage = new Uri("images/tiles/" + backgroundImageTile + "_new.png", UriKind.Relative),
                    BackBackgroundImage = new Uri("Images/tiles/unread.png", UriKind.Relative),
                    Title = topicFavObject.TopicNameFav,
                    BackContent = "Nouveaux messages !",
                    BackTitle = topicFavObject.TopicNameFav
                };

                // Create the Tile and pin it to Start. This will cause a navigation to Start and a deactivation of our application.
                // TopicUriFav = "/ReadTopic.xaml?idcat=" + topicCatId + "&idtopic=" + topicId + "&topicname=" + HttpUtility.UrlEncode(line) + "&pagenumber=" + pageNumber + "&reponseid=" + reponseId + "&numberofpages=" + numberOfPagesTopicLine,
                ShellTile.Create(new Uri("/FromTileToTopic.xaml?" + topicFavObject.TopicUriFav.Split('?')[1], UriKind.Relative), NewTileData);

            }
            else
            {
                MessageBox.Show("Ce favori est déjà épinglé sur la page d'accueil.");
            }
        }

        private void PinCatClick(object sender, RoutedEventArgs e)
        {

        }

        private void GoToReadFavClick(object sender, RoutedEventArgs e)
        {

        }

        private void drapalTile_Click(object sender, RoutedEventArgs e)
        {
            welcomePivot.SelectedIndex = 1;
        }

        private void mpTileButton_Click(object sender, RoutedEventArgs e)
        {
            welcomePivot.SelectedIndex = 3;
        }

        private void appbar_new_mp_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewMp.xaml", UriKind.Relative));
        }

        private void GoFirstPageClick(object sender, RoutedEventArgs e)
        {
            TopicFav topicFavObject = ((sender as MenuItem).DataContext) as TopicFav;
            NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + topicFavObject.TopicCatIdFav + "&idtopic=" + topicFavObject.TopicIdFav + "&topicname=" + HttpUtility.HtmlEncode(topicFavObject.TopicNameFav) + "&pagenumber=1&numberofpages=" + topicFavObject.TopicNumberOfPages, UriKind.Relative));
        }

        private void GoLastPageClick(object sender, RoutedEventArgs e)
        {
            TopicFav topicFavObject = ((sender as MenuItem).DataContext) as TopicFav;
            NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + topicFavObject.TopicCatIdFav + "&idtopic=" + topicFavObject.TopicIdFav + "&topicname=" + HttpUtility.HtmlEncode(topicFavObject.TopicNameFav) + "&pagenumber=" + topicFavObject.TopicNumberOfPages + "&numberofpages=" + topicFavObject.TopicNumberOfPages, UriKind.Relative));

        }

        private void topicHyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            TopicFav topicFavObject = ((sender as HyperlinkButton).DataContext) as TopicFav;
            //favObject.Remove(topicFavObject);
            //topicFavObject.TopicIsUnread = "Off";
            //favObject.Add(topicFavObject);
            //var itemtest = favTopics.ItemsSource = from topic in favObject
            //                                group topic by topic.TopicCatNameFav into c
            //                                select new GroupFav<HFRClasses.TopicFav>(c.Key, c);

            //var favItemSource = favTopics.ItemsSource;
            foreach (HFR7.WelcomePage.GroupFav<HFR7.HFRClasses.TopicFav> catSource in favTopics.ItemsSource)
            {
                //    if (catSource.TitleFav == topicFavObject.TopicCatNameFav)
                //    {
                //        foreach (TopicFav topicFavSource in catSource.ItemsFav)
                //        {
                //            if (topicFavSource.TopicIdFav == topicFavObject.TopicIdFav)
                //            {
                //                topicFavSource.TopicNameFav = "False";
                //                //BindingExpression binding = topicNameTextBlock.GetBindingExpression(TextBlock.TextProperty);
                //                //binding.UpdateSource();
                //                //favTopics.Bin;
                //            }
                //        }
                //    }
                //}

                //HyperlinkButton topicLink = (HyperlinkButton)sender;
                ////if (topicFavObject.TopicNumberOfPages == topicFavObject.TopicPageNumber)
                ////{
                //TextBlock topicSubtitleTextBlock = (TextBlock)topicLink.FindName("subtitleTopicTextBlock");
                //TextBlock topicNameTextBlock = (TextBlock)topicLink.FindName("topicNameTextBlock");
                //topicNameTextBlock.Foreground = new SolidColorBrush(Colors.Gray);
                //topicSubtitleTextBlock.Foreground = new SolidColorBrush(Colors.Gray);
            }
        }

        private void donateButton_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("hfr8 est gratuite et sans pub, et le restera toujours. Néanmoins, elle constitue un énorme investissement temporel du développeur. Alors n'hésitez pas à m'offrir une bière :)", "Pourquoi donner ?", MessageBoxButton.OK);
            var wbt = new Microsoft.Phone.Tasks.WebBrowserTask();
            wbt.Uri = new Uri("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=5JBRWJ8KBXMBE", UriKind.Absolute);
            wbt.Show();

        }

        private void hfrRehostTileButton_Click_1(object sender, RoutedEventArgs e)
        {

        }

        string favTopicsState = "displayed";
        private void favTopics_ManipulationStateChanged_1(object sender, EventArgs e)
        {
            ShowHideCatFav();
        }

        private void ShowHideCatFav()
        {
            if (FindViewport(favTopics).Viewport.Top < 50 && favTopicsState == "hidden")
            {
                Dispatcher.BeginInvoke(() =>
                {
                    enterFavHeader.Begin();
                    enterCatHeader.Begin();
                });
                favTopicsState = "displayed";
            }
            if (FindViewport(favTopics).Viewport.Top >= 50 && favTopicsState == "displayed")
            {
                Dispatcher.BeginInvoke(() =>
                {
                    quitFavHeader.Begin();
                    quitCatHeader.Begin();
                });
                favTopicsState = "hidden";
            }
        }


        string catGroupState = "displayed";
        private void categoriesGroup_ManipulationStateChanged_1(object sender, EventArgs e)
        {
            if (Math.Abs(FindViewport(categoriesGroup).Viewport.Top) < 50 && catGroupState == "hidden")
            {
                Dispatcher.BeginInvoke(() => enterCatHeader.Begin());
                catGroupState = "displayed";
            }
            if (Math.Abs(FindViewport(categoriesGroup).Viewport.Top) >= 50 && catGroupState == "displayed")
            {
                Dispatcher.BeginInvoke(() => quitCatHeader.Begin());
                catGroupState = "hidden";
            }
        }



        private static ViewportControl FindViewport(DependencyObject parent)
        {
            var childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childCount; i++)
            {
                var elt = VisualTreeHelper.GetChild(parent, i);
                if (elt is ViewportControl) return (ViewportControl)elt;
                var result = FindViewport(elt);
                if (result != null) return result;
            }
            return null;
        }
    }
}