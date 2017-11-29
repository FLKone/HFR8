using System.Linq;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;
using System.IO.IsolatedStorage;
using System.Net;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows.Threading;
using HFR7.HFRClasses;
using System.Xml.Serialization;
using System.Windows;

namespace HFR7ScheduledAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;
        IsolatedStorageSettings store = IsolatedStorageSettings.ApplicationSettings;
        IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        CookieContainer container = new CookieContainer();
        List<HFR7.HFRClasses.TopicFav> favObject = new List<HFR7.HFRClasses.TopicFav>();
        string isMpNotified;
        string activateMpNotif;
        List<NotifTopics> notifTopicList = new List<NotifTopics>();
        /// <remarks>
        /// Le constructeur ScheduledAgent initialise le gestionnaire UnhandledException
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // S'abonner au gestionnaire d'exceptions prises en charge
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code à exécuter sur les exceptions non gérées
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Une exception non gérée s'est produite ; arrêt dans le débogueur
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Agent qui exécute une tâche planifiée
        /// </summary>
        /// <param name="task">
        /// La tâche appelée
        /// </param>
        /// <remarks>
        /// Cette méthode est appelée lorsqu'une tâche périodique ou  resource intensive est appelée
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            try
            {
                // If your application uses both PeriodicTask and ResourceIntensiveTask
                // you can branch your application code here. Otherwise, you don't need to.
                // Launch a toast to show that the agent is running.
                // The toast will not be shown if the foreground application is running.

                // Chargement des MP déjà notifiés
                using (var file = isoStore.OpenFile("isMpNotified.txt", System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    using (var reader = new StreamReader(file))
                    {
                        isMpNotified = reader.ReadToEnd();
                    }
                }

                // Préférences
                activateMpNotif = store["activateMpNotif"] as string;

                // Cookies
                if (store.Contains("listHFRcookies"))
                {
                    // Création du cookie de l'user
                    List<Cookie> listCookies = store["listHFRcookies"] as List<Cookie>;
                    foreach (Cookie c in listCookies)
                    {
                        container.Add(new Uri("https://forum.hardware.fr", UriKind.Absolute), c);
                    }
                    store.Remove("HFRcookies");
                    store.Add("HFRcookies", container);
                }
                else
                {
                    TileErreur();
                }

                DownloadFavPage();
            }
            catch
            {
                TileErreur();
            }
        }

        private void DownloadFavPage()
        {
            try
            {
                string urlFav = "https://forum.hardware.fr/forum1f.php?owntopic=" + (string)store["favorisType"];
                HtmlWeb.LoadAsync(urlFav, container as CookieContainer, (s, args) =>
                {
                    if (args.Error != null)
                    {
                        TileErreur();
                    }
                    else
                    {
                        try
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

                            string[] mpArray = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "red").
                                                        Select(y => y.InnerText).ToArray();

                            string numberOfPagesTopicLine = "";
                            int j = 0;
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
                                    topicNameFav = Regex.Replace(HttpUtility.HtmlDecode(line), "topic unique", "T.U.", RegexOptions.IgnoreCase);
                                    topicNameFav = Regex.Replace(HttpUtility.HtmlDecode(line), "topique unique", "T.U.", RegexOptions.IgnoreCase);
                                    topicNameFav = Regex.Replace(topicNameFav, "topic unik", "T.U.", RegexOptions.IgnoreCase);
                                    topicNameFav = Regex.Replace(topicNameFav, "topik unik", "T.U.", RegexOptions.IgnoreCase);

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
                                    favorisLastPostText = HFR7.HFRClasses.TimeSpentTopic.Run(timeSpent, favorisLastPostUser);
                                    favObject.Add(new HFR7.HFRClasses.TopicFav()
                                    {
                                        TopicNameFav = topicNameFav,
                                        TopicCatIdFav = topicCatId,
                                        TopicIdFav = topicId,
                                        TopicCatNameFav = HFR7.HFRClasses.GetCatName.PlainNameFromId(topicCatId),
                                        TopicUriFav = "?idcat=" + topicCatId + "&idtopic=" + topicId + "&topicname=" + HttpUtility.UrlEncode(line) + "&pagenumber=" + pageNumber + "&jump=" + reponseId + "&numberofpages=" + numberOfPagesTopicLine,
                                        TopicLastPostDateDouble = favorisSingleLastPostTime,
                                        TopicLastPost = favorisLastPostText,
                                        TopicNumberOfPages = numberOfPagesTopicLine,
                                        TopicPage = pageNumber,
                                        TopicJump = reponseId
                                    });
                                    j++;
                                }
                                i++;
                            }

                            ////////////////////////////////
                            // Mise à jour des tiles topics
                            ////////////////////////////////
                            foreach (ShellTile tile in ShellTile.ActiveTiles)
                            {
                                bool containsFav = false;
                                if (tile.NavigationUri.ToString() != "/")
                                {
                                    string tilePostId;
                                    int firstTilePostId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString()).IndexOf("&idtopic=") + "&idtopic=".Length;
                                    int lastTilePostId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString()).IndexOf("&topicname=", firstTilePostId);
                                    tilePostId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString().Substring(firstTilePostId, lastTilePostId - firstTilePostId));

                                    string tileCatId;
                                    int firstTileCatId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString()).IndexOf("?idcat=") + "?idcat=".Length;
                                    int lastTileCatId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString()).IndexOf("&idtopic=", firstTileCatId);
                                    tileCatId = HttpUtility.HtmlDecode(tile.NavigationUri.ToString().Substring(firstTileCatId, lastTileCatId - firstTileCatId));
                                    foreach (TopicFav fav in favObject)
                                    {
                                        if (tilePostId == fav.TopicIdFav)
                                        {
                                            containsFav = true;
                                        }
                                    }
                                    if (containsFav)
                                    {
                                        StandardTileData NewTileData = new StandardTileData
                                        {
                                            BackContent = "Nouveaux messages !",
                                            BackgroundImage = new Uri("Images/tiles/" + HFR7.HFRClasses.GetCatName.ShortNameFromId(tileCatId) + "_new.png", UriKind.Relative),
                                            BackBackgroundImage = new Uri("Images/tiles/unread.png", UriKind.Relative)
                                        };
                                        tile.Update(NewTileData);
                                    }
                                    else
                                    {
                                        StandardTileData NewTileData = new StandardTileData
                                        {
                                            BackContent = "Pas de nouveaux messages",
                                            BackgroundImage = new Uri("Images/tiles/" + HFR7.HFRClasses.GetCatName.ShortNameFromId(tileCatId) + ".png", UriKind.Relative),
                                            BackBackgroundImage = new Uri("Images/tiles/read.png", UriKind.Relative)

                                        };
                                        tile.Update(NewTileData);
                                    }
                                }
                            }

                            ////////////////////////////////
                            // Mise à jour de la tile principale
                            ////////////////////////////////
                            int numberOfUnreadFav = favObject.Count;
                            ShellTile MainTileToFind = ShellTile.ActiveTiles.FirstOrDefault();

                            StandardTileData MainNewAppTileData = new StandardTileData
                            {
                                Count = numberOfUnreadFav
                            };
                            MainTileToFind.Update(MainNewAppTileData);


                            ////////////////////////////////
                            // Mise à jour des MP
                            ////////////////////////////////
                            ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault();
                            if (mpArray.Length != 0 && isMpNotified == "false" && activateMpNotif == "true" && DateTime.Now.Hour > 9 && DateTime.Now.Hour < 23)
                            {
                                if (Convert.ToInt32(mpArray[0].Split(' ')[2]) > 0)
                                {
                                    ShellToast mpToast = new ShellToast();
                                    mpToast.Title = "HFR7";
                                    mpToast.Content = "1 nouveau message privé.";
                                    mpToast.Show();
                                    
                                    StandardTileData NewAppTileData = new StandardTileData
                                    {
                                        BackTitle = "HFR7",
                                        BackBackgroundImage = new Uri("Images/tiles/backtile.png", UriKind.Relative),
                                        BackContent = "Nouveau message privé !"
                                    };
                                    if (isoStore.FileExists("isMpNotified.txt")) isoStore.DeleteFile("isMpNotified.txt");
                                    using (var file = isoStore.OpenFile("isMpNotified.txt", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                                    {
                                        using (var writer = new StreamWriter(file))
                                        {
                                            writer.Write("true");
                                        }
                                    }
                                    TileToFind.Update(NewAppTileData);
                                }
                            }
                            else if (mpArray.Length == 0)
                            {
                                if (isoStore.FileExists("isMpNotified.txt")) isoStore.DeleteFile("isMpNotified.txt");
                                using (var file = isoStore.OpenFile("isMpNotified.txt", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                                {
                                    using (var writer = new StreamWriter(file))
                                    {
                                        writer.Write("false");
                                    }
                                }
                            }
                            //////////////////////////////////
                            //// Notifications
                            //////////////////////////////////
                            //ShellToast notifToast = new ShellToast();
                            //XmlSerializer serializer = new XmlSerializer(notifTopicList.GetType());
                            //using (var file = isoStore.OpenFile("notifications.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read))
                            //{
                            //    using (var reader = new StreamReader(file))
                            //    {

                            //        try
                            //        {
                            //            object deserialized = serializer.Deserialize(reader.BaseStream);
                            //            notifTopicList = (List<NotifTopics>)deserialized;
                            //        }
                            //        catch { }
                            //    }
                            //}
                            //foreach (TopicFav fav in favObject)
                            //{
                            //    int containsNotif = -1;
                            //    int h = 0;
                            //    foreach (NotifTopics notifTopic in notifTopicList)
                            //    {
                            //        if (fav.TopicIdFav == notifTopic.TopicId && notifTopic.TopicIsNotif == false && DateTime.Now.Hour > 9 && DateTime.Now.Hour < 23)
                            //        {
                            //            containsNotif = h;
                            //            notifToast.Title = "HFR7";
                            //            notifToast.Content = "Nouvelle réponse : " + fav.TopicNameFav;
                            //            notifToast.NavigationUri = new Uri("/ReadTopic.xaml?idcat=" + fav.TopicCatIdFav + "&idtopic=" + fav.TopicIdFav + "&topicname=" + HttpUtility.UrlEncode(fav.TopicNameFav) + "&pagenumber=" + fav.TopicPage + "&jump=" + fav.TopicJump + "&numberofpages=" + fav.TopicNumberOfPages, UriKind.Relative);
                            //            notifToast.Show();
                            //        }
                            //        h++;
                            //    }
                            //    if (containsNotif != -1)
                            //    {
                            //        notifTopicList.RemoveAt(containsNotif);
                            //        notifTopicList.Add(new NotifTopics()
                            //        {
                            //            TopicCatId = fav.TopicCatIdFav.ToString(),
                            //            TopicId = fav.TopicIdFav.ToString(),
                            //            TopicIsNotif = true,
                            //            TopicJump = fav.TopicJump.ToString(),
                            //            TopicName = fav.TopicNameFav.ToString(),
                            //            TopicNumberOfPages = fav.TopicNumberOfPages.ToString(),
                            //            TopicPageNumber = fav.TopicPage.ToString()
                            //        });
                            //    }
                            //    if (isoStore.FileExists("notifications.xml")) isoStore.DeleteFile("notifications.xml");
                            //    using (var file = isoStore.OpenFile("notifications.xml", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                            //    {
                            //        using (var writer = new StreamWriter(file))
                            //        {
                            //            try
                            //            {
                            //                serializer.Serialize(writer.BaseStream, notifTopicList);
                            //            }
                            //            catch { }
                            //        }
                            //    }
                            //}
                            NotifyComplete();
                        }
                        catch
                        {
                            TileErreur();
                        }
                    }
                });
            }
            catch
            {
                NotifyComplete();
            }
        }

        private void TileErreur()
        {
            ShellTile tile = ShellTile.ActiveTiles.FirstOrDefault();
            StandardTileData NewTileData = new StandardTileData
            {
                //BackTitle = "Erreur",
                //BackContent = "Dernière tentative à " + DateTime.Now.Hour + "h" + DateTime.Now.Minute,
                //Count = 0
            };
            // Update the Application Tile
            tile.Update(NewTileData);
            NotifyComplete();
        }
    }
}