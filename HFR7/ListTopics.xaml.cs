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
using System.Xml.Serialization;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.Globalization;
using HFR7.HFRClasses;
using Microsoft.Phone.Shell;

namespace HFR7
{
    public partial class ListTopics : PhoneApplicationPage
    {
        XDocument docTopicNorm = new XDocument();
        System.IO.IsolatedStorage.IsolatedStorageSettings store = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
        IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        CookieContainer container;
        string souscatUriShort;
        string souscatUri;
        string idCat;
        string souscatName;
        string from;
        string currentTheme;
        string currentOrientation;
        string pivot;
        string listPageNumber;
        BitmapImage bi = new BitmapImage();
        bool topicsGroupGroupViewOpened = false;
        List<HFRClasses.TopicNorm> topicsObject = new List<HFRClasses.TopicNorm>();
        List<HFRClasses.TopicNorm> topicsObjectCache = new List<HFRClasses.TopicNorm>();
        List<HFRClasses.TopicFav> drapObject = new List<HFRClasses.TopicFav>();


        public ListTopics()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            SystemTray.ProgressIndicator.IsIndeterminate = true;
            if ((string)store["disableLandscape"] == "true") ListTopicPage.SupportedOrientations = SupportedPageOrientation.Portrait;
            if (store.Contains("navigatedback"))
            {
                store.Remove("navigatedback");
                if (isoStore.DirectoryExists("topics"))
                {
                    foreach (string fileName in isoStore.GetFileNames("topics/*"))
                    {
                        isoStore.DeleteFile("topics/" + fileName);
                    }
                }
            }
            else
            {
                // Récupération de l'URI de la sous-catégorie
                NavigationContext.QueryString.TryGetValue("souscaturi", out souscatUri);
                souscatUri = HttpUtility.UrlDecode(souscatUri);

                // Récupération de l'ID de la catégorie
                NavigationContext.QueryString.TryGetValue("idcat", out idCat);

                // Récupération du nom du forum
                NavigationContext.QueryString.TryGetValue("souscatname", out souscatName);
                if (souscatName == null)
                {
                    souscatName = GetCatName.PlainNameFromId(idCat);
                }
                topicsPivot.Title = souscatName.ToUpper();

                // Récupération de la provenance
                NavigationContext.QueryString.TryGetValue("from", out from);
                if (from == "changepage") NavigationService.RemoveBackEntry();

                // Pivot
                NavigationContext.QueryString.TryGetValue("pivot", out pivot);

                // listPageNumber
                NavigationContext.QueryString.TryGetValue("listpagenumber", out listPageNumber);
                if (Convert.ToInt32(listPageNumber) > 1) ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true;
                else ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = false;
                if (!souscatUri.Contains("cat=0")) souscatUri = souscatUri.Substring(0, souscatUri.Length - 5) + listPageNumber + ".htm";
                

                if (pivot == "drap")
                {
                    topicsPivot.SelectedIndex = 1;
                }
                else if (pivot == "topics")
                {
                    topicsPivot.SelectedIndex = 0;
                }

                // Récupération du cookie
                if (store.Contains("HFRcookies"))
                {
                    container = store["HFRcookies"] as CookieContainer;
                }
                else
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

                // ItemsSources
                drapList.ItemsSource = null;
                topicsList.ItemsSource = null;

                // Téléchargement de la liste
                DownloadTopicsDrapals(Convert.ToInt32(listPageNumber));
            }
        }

        public void DownloadTopicsDrapals(int listPageNumber)
        {
            // Progress Bar
            SystemTray.ProgressIndicator.IsVisible = true;

            //=====================
            //CHARGEMENT DES DRAPAUX
            //=====================
            drapObject.Clear();
            string urlDrap = "https://forum.hardware.fr/forum1.php?cat=" + idCat + "&owntopic=1";
            HtmlWeb.LoadAsync(urlDrap, container, (s, args) =>
            {
                if (args.Error != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Erreur dans la récupération des drapeaux : " + args.Error.Message + ", " + args.Error.InnerException + ", " + args.Error.Data);
                        SystemTray.ProgressIndicator.IsVisible = false;
                    });
                }
                if (args.Document == null)
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show("Serveur en cours de maintenance. Veuillez réessayer plus tard."));
                }
                else
                {
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

                    string numberOfPagesTopicLine = "";
                    int i = 0;
                    int j = 0;
                    foreach (string line in favorisTopicNames)
                    {
                        // Nombre de pages
                        if (favorisTopicNumberOfPages[j] != "&nbsp;") numberOfPagesTopicLine = favorisTopicNumberOfPages[j];
                        else numberOfPagesTopicLine = "1";

                        string topicCatId;
                        int firstTopicCatId = HttpUtility.HtmlDecode(favorisTopicUri[j]).IndexOf("&cat=") + "&cat=".Length;
                        int lastTopicCatId = HttpUtility.HtmlDecode(favorisTopicUri[j]).IndexOf("&", firstTopicCatId);
                        topicCatId = HttpUtility.HtmlDecode(favorisTopicUri[j]).Substring(firstTopicCatId, lastTopicCatId - firstTopicCatId);

                        string topicId;
                        int firstTopicId = HttpUtility.HtmlDecode(favorisTopicUri[j]).IndexOf("&post=") + "&post=".Length;
                        int lastTopicId = HttpUtility.HtmlDecode(favorisTopicUri[j]).LastIndexOf("&page");
                        topicId = HttpUtility.HtmlDecode(favorisTopicUri[j]).Substring(firstTopicId, lastTopicId - firstTopicId);

                        string pageNumber;
                        string reponseId;
                        if (favorisIsHot[j] == "On")
                        {
                            int firstPageNumber = HttpUtility.HtmlDecode(favorisBalise[i]).IndexOf("&page=") + "&page=".Length;
                            int lastPageNumber = HttpUtility.HtmlDecode(favorisBalise[i]).LastIndexOf("&p=");
                            pageNumber = HttpUtility.HtmlDecode(favorisBalise[i]).Substring(firstPageNumber, lastPageNumber - firstPageNumber);
                            int firstReponseId = HttpUtility.HtmlDecode(favorisBalise[i]).IndexOf("#t") + "#t".Length;
                            int lastReponseId = HttpUtility.HtmlDecode(favorisBalise[i]).Length;
                            reponseId = "rep" + HttpUtility.HtmlDecode(favorisBalise[i]).Substring(firstReponseId, lastReponseId - firstReponseId);
                            i++;
                        }
                        else
                        {
                            pageNumber = numberOfPagesTopicLine;
                            reponseId = "bas";
                        }

                        // Formatage topic name
                        string topicNameFav;
                        topicNameFav = Regex.Replace(HttpUtility.HtmlDecode(line), "Topic unique", "T.U.");
                        topicNameFav = Regex.Replace(topicNameFav, "Topic Unique", "T.U.");

                        // Conversion date
                        string favorisSingleLastPostTimeString;
                        favorisSingleLastPostTimeString = Regex.Replace(Regex.Replace(HttpUtility.HtmlDecode(favorisLastPost[j].Substring(0, 28)), "à", ""), "-", "/");
                        DateTime favorisSingleLastPostDT;
                        favorisSingleLastPostDT = DateTime.Parse(favorisSingleLastPostTimeString, new CultureInfo("fr-FR"));
                        double favorisSingleLastPostTime;
                        favorisSingleLastPostTime = Convert.ToDouble(favorisSingleLastPostDT.ToFileTime());

                        // Nom du dernier posteur
                        string favorisLastPostUser;
                        favorisLastPostUser = HttpUtility.HtmlDecode(favorisLastPost[j].Substring(28, favorisLastPost[j].Length - 28));

                        // Temps depuis dernier post
                        string favorisLastPostText;
                        TimeSpan timeSpent;
                        timeSpent = DateTime.Now.Subtract(favorisSingleLastPostDT);

                        favorisLastPostText = HFRClasses.TimeSpentTopic.Run(timeSpent, favorisLastPostUser);

                        drapObject.Add(new HFRClasses.TopicFav()
                        {
                            TopicNameFav = topicNameFav,
                            TopicCatIdFav = topicCatId,
                            TopicIdFav = topicId,
                            TopicCatNameFav = HFRClasses.GetCatName.PlainNameFromId(topicCatId),
                            TopicGroup = "drapeaux",
                            TopicUriFav = "/ReadTopic.xaml?idcat=" + topicCatId + "&idtopic=" + topicId + "&topicname=" + HttpUtility.UrlEncode(line) + "&souscatname=" + HttpUtility.UrlEncode(souscatName) + "&souscaturi=" + HttpUtility.UrlEncode(souscatUri) + "&pagenumber=" + pageNumber + "&numberofpages=" + numberOfPagesTopicLine + "&from=listdrapcat&jump=" + reponseId,
                            TopicLastPostDateDouble = favorisSingleLastPostTime,
                            TopicLastPost = favorisLastPostText,
                            TopicIsHot = favorisIsHot[j],
                            TopicNumberOfPages = numberOfPagesTopicLine

                        });
                        j++;
                    }
                }

                // Binding
                Dispatcher.BeginInvoke(() =>
                {

                    // Tri et binding
                    //var drapParCat = from topic in drapObject
                    //                 group topic by topic.TopicGroup into c
                    //                 select new Group<HFRClasses.TopicFav>(c.Key, c);
                    //drapList.ItemsSource = drapParCat.ToList();
                    List<GroupFav<TopicFav>> DataSource = GroupFav<TopicFav>.CreateGroups(drapObject, System.Threading.Thread.CurrentThread.CurrentUICulture, (TopicFav tf) => { return tf.TopicCatNameFav; }, true);
                    drapList.ItemsSource = DataSource;
                    try { SystemTray.ProgressIndicator.IsVisible = false; }
                    catch { }
                    if (topicsList.ItemsSource != null) SystemTray.ProgressIndicator.IsVisible = false;
                });
            });

            //=====================
            //CHARGEMENT DES TOPICS
            //=====================

            // Si utilisation du cache, déserialization
            //if (cacheTopic && isoStore.FileExists("topics" + souscatUriShort + ".xml"))
            //{
            //    XmlSerializer serializer = new XmlSerializer(topicsObject.GetType());
            //    using (var file = isoStore.OpenFile("topics" + souscatUriShort + ".xml", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            //    {
            //        using (var reader = new StreamReader(file))
            //        {
            //            object deserialized = serializer.Deserialize(reader.BaseStream);
            //            topicsObjectCache = (List<HFRClasses.TopicNorm>)deserialized;
            //            // Tri
            //            var topicsParGroupeCache = from categorie in topicsObjectCache
            //                                       group categorie by categorie.TopicGroup into c
            //                                       orderby c.Key
            //                                       select new Group<HFRClasses.TopicNorm>(c.Key, c);

            //            // Binding
            //            Dispatcher.BeginInvoke(() => topicsList.ItemsSource = topicsParGroupeCache);
            //        }
            //    }
            //}
            //// Si le cache n'est pas utilisé, on télécharge la liste
            //else if (!cacheTopic)
            //{
            //if (isoStore.FileExists("topics" + souscatUriShort + ".xml")) isoStore.DeleteFile("topics" + souscatUriShort + ".xml");
            //string uriTopicsList = "https://forum.hardware.fr/forum1.php?cat=" + idCat+ "&subcat=" + souscatName + "&page=" + listPageNumber;
            string uriTopicsList = "https://forum.hardware.fr" + souscatUri;
            //string uriTopicsList = "http://www.scrubs-fr.net/perso/hfr7/cat0/catzero.htm";

            HtmlWeb.LoadAsync(uriTopicsList, container, (s, args) =>
            {
                if (args.Error != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Erreur dans la lecture de la liste de sujets. Vérifiez votre connectivité. Informations déboguage : " + args.Error.Message + ", " + args.Error.Data);
                        SystemTray.ProgressIndicator.IsVisible = false;
                    });
                }
                if (args.Document == null)
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show("Serveur en cours de maintenance. Veuillez réessayer plus tard.")); ;
                }
                else
                {
                    int i = 0;
                    string[] nameTopics = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "cCatTopic" && (bool)x.GetAttributeValue("title", "").Contains("Sujet") == true).
                        Select(y => y.InnerText).ToArray();
                    string[] idTopics = args.Document.DocumentNode.Descendants("a").Where(x => (bool)x.GetAttributeValue("title", "").Contains("Sujet n°") == true).
                        Select(y => y.GetAttributeValue("title", "").Split('°')[1]).ToArray();
                    string[] uriTopics = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "cCatTopic" && (bool)x.GetAttributeValue("title", "").Contains("Sujet") == true).
                        Select(x => x.GetAttributeValue("href", "")).ToArray();
                    //string[] iconTopics = args.Document.DocumentNode.Descendants("img").Where(x => (bool)x.GetAttributeValue("src", "").Contains("http://forum-images.hardware.fr/") == true && ((bool)x.GetAttributeValue("src", "").Contains("sondage") == true || (bool)x.GetAttributeValue("src", "").Contains("icon") == true)).
                    //    Select(x => x.GetAttributeValue("src", "")).ToArray();
                    string[] drapTopics = args.Document.DocumentNode.Descendants("td").Where(x => (string)x.GetAttributeValue("class", "") == "sujetCase5").
                        Select(x => x.InnerHtml).ToArray();
                    string[] lastPostTopics = args.Document.DocumentNode.Descendants("td").Where(x => (bool)x.GetAttributeValue("class", "").Contains("sujetCase9") == true).
                                                Select(y => y.InnerText).ToArray();
                    string[] numberOfPagesTopic = args.Document.DocumentNode.Descendants("td").Where(x => (string)x.GetAttributeValue("class", "") == "sujetCase4").
                                                Select(y => y.InnerText).ToArray();

                    // Suppression de la liste actuelle


                    string iconTopicsLine = "";
                    string drapTopicsLine = "";
                    string drapTopicPage = "";
                    string drapTopicReponse = "";
                    string numberOfPagesTopicLine = "";
                    foreach (string line in nameTopics)
                    {
                        // Nombre de pages
                        if (numberOfPagesTopic[i] != "&nbsp;") numberOfPagesTopicLine = numberOfPagesTopic[i];
                        else numberOfPagesTopicLine = "1";

                        //// Type de topic
                        //if (iconTopics[i].Contains("sondage")) iconTopicsLine = "Images/ImagesTopics/sondage.png";
                        //else if (iconTopics[i].Contains("flag1")) iconTopicsLine = "Images/ImagesTopics/drapal.png";
                        //else if (iconTopics[i].Contains("flag0")) iconTopicsLine = "Images/ImagesTopics/drapal.lu.png";
                        //else iconTopicsLine = "Images/ImagesTopics/topic.png";

                        // Drapeau ou pas
                        //if (drapTopics[i].Contains("flag0")) drapTopicsLine = "Images/ImagesTopics/drapal.lu.png";
                        //if (drapTopics[i].Contains("favoris")) drapTopicsLine = "etoile";

                        // Formatage du nom du sujet
                        string topicName;
                        topicName = TopicNameShortener.Shorten(HttpUtility.HtmlDecode(line));

                        // Conversion date
                        string favorisSingleLastPostTimeString;
                        favorisSingleLastPostTimeString = Regex.Replace(Regex.Replace(HttpUtility.HtmlDecode(lastPostTopics[i].Substring(0, 28)), "à", ""), "-", "/");
                        DateTime favorisSingleLastPostDT;
                        favorisSingleLastPostDT = DateTime.Parse(favorisSingleLastPostTimeString, new CultureInfo("fr-FR"));
                        double favorisSingleLastPostTime;
                        favorisSingleLastPostTime = Convert.ToDouble(favorisSingleLastPostDT.ToFileTime());

                        // Nom du dernier posteur
                        string favorisLastPostUser;
                        favorisLastPostUser = HttpUtility.HtmlDecode(lastPostTopics[i].Substring(28, lastPostTopics[i].Length - 28));

                        // Temps depuis dernier post
                        string favorisLastPostText;
                        TimeSpan timeSpent;
                        timeSpent = DateTime.Now.Subtract(favorisSingleLastPostDT);

                        favorisLastPostText = HFRClasses.TimeSpentTopic.Run(timeSpent, favorisLastPostUser);


                        // Ajout à l'instance
                        topicsObject.Add(new HFRClasses.TopicNorm()
                        {
                            TopicName = topicName,
                            TopicUri = "/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopics[i] + "&topicname=" + HttpUtility.UrlEncode(line) + "&souscaturi=" + HttpUtility.UrlEncode(souscatUri) + "&souscatname=" + HttpUtility.UrlEncode(souscatName) + "&pagenumber=1&numberofpages=" + numberOfPagesTopicLine + "&from=listtopicscat",
                            TopicUriDrap = "/ReadTopic.xaml?idcat=" + idCat + "&idtopic=" + idTopics[i] + "&topicname=" + HttpUtility.UrlEncode(line) + "&souscaturi=" + HttpUtility.UrlEncode(souscatUri) + "&souscatname=" + HttpUtility.UrlEncode(souscatName) + "&pagenumber=1&numberofpages=" + numberOfPagesTopicLine + "&from=listtopicscat",
                            TopicCatId = idCat,
                            TopicId = idTopics[i],
                            TopicGroup = "page " + listPageNumber,
                            TopicLastPostDateDouble = favorisSingleLastPostTime,
                            TopicLastPost = favorisLastPostText,
                            TopicNumberOfPages = numberOfPagesTopicLine
                        });
                        i++;
                    }
                    Dispatcher.BeginInvoke(() =>
                    {
                        // Tri
                        //var topicsParGroupe = from o in topicsObject
                        //                      group o by o.TopicGroup into c
                        //                      orderby c.Key
                        //                      select new Group<HFRClasses.TopicNorm>(c.Key, c);

                        // Binding
                        if (drapList.ItemsSource != null) SystemTray.ProgressIndicator.IsVisible = false;
                        List<GroupFav<TopicNorm>> DataSource = GroupFav<TopicNorm>.CreateGroups(topicsObject, System.Threading.Thread.CurrentThread.CurrentUICulture, (TopicNorm tf) => { return tf.TopicGroup; }, true);
                        topicsList.ItemsSource = DataSource;

                        //topicsList.ItemsSource = topicsParGroupe.ToList();
                    });

                    //// Serialization
                    //XmlSerializer serializer = new XmlSerializer(topicsObject.GetType());
                    //if (isoStore.FileExists("topics" + souscatUriShort + ".xml")) isoStore.DeleteFile("topics" + souscatUriShort + ".xml");
                    //using (var file = isoStore.OpenFile("topics" + souscatUriShort + ".xml", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                    //{
                    //    using (var writer = new StreamWriter(file))
                    //    {
                    //        serializer.Serialize(writer.BaseStream, topicsObject);
                    //    }
                    //}
                }
            });
        }

        public class Group<T> : IEnumerable<T>
        {
            public Group(string name, IEnumerable<T> items)
            {
                this.Title = name;
                this.Items = new List<T>(items);
            }

            public override bool Equals(object obj)
            {
                Group<T> that = obj as Group<T>;

                return (that != null) && (this.Title.Equals(that.Title));
            }

            public string Title
            {
                get;
                set;
            }

            public IList<T> Items
            {
                get;
                set;
            }

            #region IEnumerable<T> Members

            public IEnumerator<T> GetEnumerator()
            {
                return this.Items.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.Items.GetEnumerator();
            }

            #endregion
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
            public static List<GroupFav<TopicNorm>> CreateGroups(List<TopicNorm> items, CultureInfo ci, GetKeyDelegate getKey, bool sort)
            {
                Microsoft.Phone.Globalization.SortedLocaleGrouping slg = new Microsoft.Phone.Globalization.SortedLocaleGrouping(ci);
                List<GroupFav<TopicNorm>> list = new List<GroupFav<TopicNorm>>();
                list.Add(new GroupFav<TopicNorm>("page 1"));
                list.Add(new GroupFav<TopicNorm>("page 2"));
                list.Add(new GroupFav<TopicNorm>("page 3"));
                list.Add(new GroupFav<TopicNorm>("page 4"));
                list.Add(new GroupFav<TopicNorm>("page 5"));
                list.Add(new GroupFav<TopicNorm>("page 6"));
                list.Add(new GroupFav<TopicNorm>("page 7"));
                list.Add(new GroupFav<TopicNorm>("page 8"));
                list.Add(new GroupFav<TopicNorm>("page 9"));
                list.Add(new GroupFav<TopicNorm>("page 10"));
                list.Add(new GroupFav<TopicNorm>("page 11"));
                list.Add(new GroupFav<TopicNorm>("page 12"));
                list.Add(new GroupFav<TopicNorm>("page 13"));
                list.Add(new GroupFav<TopicNorm>("page 14"));
                list.Add(new GroupFav<TopicNorm>("page 15"));
                list.Add(new GroupFav<TopicNorm>("page 16"));
                list.Add(new GroupFav<TopicNorm>("page 17"));
                list.Add(new GroupFav<TopicNorm>("page 18"));
                list.Add(new GroupFav<TopicNorm>("page 19"));
                list.Add(new GroupFav<TopicNorm>("page 20"));

                //items.Add(new TopicNorm()
                //{
                //    TopicCatId="",
                //    TopicDrap="",
                //    TopicGroup
                //    TopicIconUri
                    
                //    TopicLastPostDateDouble = 0,
                //    TopicLastPost = "",
                //    TopicNumberOfPages = "",
                //    TopicPageNumber = "",
                //});
                foreach (TopicNorm item in items)
                {
                    int i = 0;
                    int index = -1;
                    foreach (var it in list)
                    {
                        string currentKey = it.Key;
                        if (currentKey == item.TopicGroup)
                        {
                            index = i;
                        }
                        i++;
                    }
                    if (index != -1) list[index].Add(item);
                }
                return list;
            }
            public static List<GroupFav<TopicFav>> CreateGroups(List<TopicFav> items, CultureInfo ci, GetKeyDelegate getKey, bool sort)
            {
                Microsoft.Phone.Globalization.SortedLocaleGrouping slg = new Microsoft.Phone.Globalization.SortedLocaleGrouping(ci);
                List<GroupFav<TopicFav>> list = new List<GroupFav<TopicFav>>();
                list.Add(new GroupFav<TopicFav>("Hardware"));
                list.Add(new GroupFav<TopicFav>("Périphériques"));
                list.Add(new GroupFav<TopicFav>("PC portables"));
                list.Add(new GroupFav<TopicFav>("Techno. Mobiles"));
                list.Add(new GroupFav<TopicFav>("Overclock & Tuning"));
                list.Add(new GroupFav<TopicFav>("Apple        "));
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
                    if (index != -1) list[index].Add(item);
                }
                return list;
            }
        }
        private void topicsList_ScrollingCompleted(object sender, EventArgs e)
        {
            string lol = topicsList.VerticalContentAlignment.ToString();
            //DownloadTopicsDrapals(false, 2);
        }

        private void appbar_refresh_Click(object sender, EventArgs e)
        {
            DownloadTopicsDrapals(1);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (topicsGroupGroupViewOpened == true)
            {
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);

                if(!store.Contains("navigatedback")) e.Cancel = true;
                store.Remove("navigatedback");
                store.Add("navigatedback", "true");
                NavigationService.GoBack();
                //if (from == "categorielist")
                //{
                //    NavigationService.Navigate(new Uri("/WelcomePage.xaml?pivot=2", UriKind.Relative));
                //}
                //else
                //{
                //    NavigationService.Navigate(new Uri("/WelcomePage.xaml?pivot=1", UriKind.Relative));
                //}
            }
        }

        //private void topicsGroup_GroupViewOpened(object sender, GroupViewOpenedEventArgs e)
        //{
        //    topicsGroupGroupViewOpened = true;
        //}

        //private void topicsGroup_GroupViewClosing(object sender, GroupViewClosingEventArgs e)
        //{
        //    topicsGroupGroupViewOpened = false;
        //}

        private void topicsPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void PinTopicClickFav(object sender, RoutedEventArgs e)
        {
            TopicFav topicFavObject = ((sender as MenuItem).DataContext) as TopicFav;
            string backgroundImageTile;
            string backContent;
            Uri backgroundImage;
            Uri backBackgroundImage;
            // Création de la tile
            ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(topicFavObject.TopicUriFav));

            // Message à afficher
            backgroundImageTile = HFRClasses.GetCatName.ShortNameFromId(topicFavObject.TopicCatIdFav);
            if (topicFavObject.TopicIsHot == "On")
            {
                backContent = "Nouveaux messages !";
                backBackgroundImage = new Uri("Images/tiles/unread.png", UriKind.Relative);
                backgroundImage = new Uri("images/tiles/" + backgroundImageTile + "_new.png", UriKind.Relative);
            }
            else
            {
                backContent = "Pas de nouveaux messages";
                backBackgroundImage = new Uri("Images/tiles/read.png", UriKind.Relative);
                backgroundImage = new Uri("images/tiles/" + backgroundImageTile + ".png", UriKind.Relative);
            }

            if (TileToFind == null)
            {

                // The Count value of 12 shows the number 12 on the front of the Tile. Valid values are 1-99.
                // A Count value of 0 indicates that the Count should not be displayed.
                StandardTileData NewTileData = new StandardTileData
                {
                    BackgroundImage = backgroundImage,
                    BackBackgroundImage = backBackgroundImage,
                    Title = topicFavObject.TopicNameFav,
                    BackContent = backContent,
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

        private void PinTopicClickSujet(object sender, RoutedEventArgs e)
        {
        //    TopicNorm topicNormObject = ((sender as MenuItem).DataContext) as TopicNorm;
        //    string backgroundImageTile;
        //    // Create the Tile object and set some initial properties for the Tile.
        //    ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains(topicNormObject.TopicUri));
        //    if (TileToFind == null)
        //    {
        //        backgroundImageTile = HFRClasses.GetCatName.ShortNameFromId(topicNormObject.TopicCatId);

        //        // The Count value of 12 shows the number 12 on the front of the Tile. Valid values are 1-99.
        //        // A Count value of 0 indicates that the Count should not be displayed.
        //        StandardTileData NewTileData = new StandardTileData
        //        {
        //            BackgroundImage = new Uri("images/tiles/" + backgroundImageTile + "_new.png", UriKind.Relative),
        //            BackBackgroundImage = new Uri("Images/tiles/unread.png", UriKind.Relative),
        //            Title = topicNormObject.TopicName,
        //            BackContent = "Nouveaux messages !",
        //            BackTitle = topicNormObject.TopicName
        //        };

        //        // Create the Tile and pin it to Start. This will cause a navigation to Start and a deactivation of our application.
        //        // TopicUriFav = "/ReadTopic.xaml?idcat=" + topicCatId + "&idtopic=" + topicId + "&topicname=" + HttpUtility.UrlEncode(line) + "&pagenumber=" + pageNumber + "&reponseid=" + reponseId + "&numberofpages=" + numberOfPagesTopicLine,
        //        ShellTile.Create(new Uri("/FromTileToTopic.xaml?" + topicNormObject.TopicUri.Split('?')[1], UriKind.Relative), NewTileData);
        //    }
        //    else
        //    {
        //        MessageBox.Show("Ce favori est déjà épinglé sur la page d'accueil.");
        //    }
            MessageBox.Show("Il est possible d'épingler uniquement des sujets favoris. Essayez à partir du pivot favoris ou de la page d'accueil !");
        }

        private void ForwardInTransition()
        {
            TransitionElement transitionElement = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn };
            ITransition transition = transitionElement.GetTransition((PhoneApplicationPage)(((PhoneApplicationFrame)Application.Current.RootVisual)).Content);
            transition.Completed += delegate
            {
                transition.Stop();
            };
            transition.Begin();
        }

        private void GoFavFirstPageClick(object sender, RoutedEventArgs e)
        {
            TopicFav topicFavObject = ((sender as MenuItem).DataContext) as TopicFav;
            NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + topicFavObject.TopicCatIdFav + "&idtopic=" + topicFavObject.TopicIdFav + "&topicname=" + HttpUtility.UrlEncode(topicFavObject.TopicNameFav) + "&pagenumber=1&numberofpages=" + topicFavObject.TopicNumberOfPages + "&from=listdrapcat", UriKind.Relative));
        }

        private void GoFavLastPageClick(object sender, RoutedEventArgs e)
        {
            TopicFav topicFavObject = ((sender as MenuItem).DataContext) as TopicFav;
            NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + topicFavObject.TopicCatIdFav + "&idtopic=" + topicFavObject.TopicIdFav + "&topicname=" + HttpUtility.UrlEncode(topicFavObject.TopicNameFav) + "&pagenumber=" + topicFavObject.TopicNumberOfPages + "&numberofpages=" + topicFavObject.TopicNumberOfPages + "&from=listdrapcat", UriKind.Relative));

        }

        private void GoNormFirstPageClick(object sender, RoutedEventArgs e)
        {
            TopicNorm topicNormObject = ((sender as MenuItem).DataContext) as TopicNorm;
            NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + topicNormObject.TopicCatId + "&idtopic=" + topicNormObject.TopicId + "&topicname=" + HttpUtility.UrlEncode(topicNormObject.TopicName) + "&pagenumber=1&numberofpages=" + topicNormObject.TopicNumberOfPages + "&from=listtopicscat", UriKind.Relative));

        }

        private void GoNormLastPageClick(object sender, RoutedEventArgs e)
        {
            TopicNorm topicNormObject = ((sender as MenuItem).DataContext) as TopicNorm;
            NavigationService.Navigate(new Uri("/ReadTopic.xaml?idcat=" + topicNormObject.TopicCatId + "&idtopic=" + topicNormObject.TopicId + "&topicname=" + HttpUtility.UrlEncode(topicNormObject.TopicName) + "&pagenumber=" + topicNormObject.TopicNumberOfPages + "&numberofpages=" + topicNormObject.TopicNumberOfPages + "&from=listtopicscat", UriKind.Relative));

        }

        private void appbar_lastpage_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ListTopics.xaml?souscaturi=" + souscatUri + "&listpagenumber=" + (Convert.ToInt32(listPageNumber) - 1) + "&souscatname=" + souscatName + "&idcat=" + idCat + "&from=changepage", UriKind.Relative));

        }

        private void appbar_nextpage_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/ListTopics.xaml?souscaturi=" + souscatUri + "&listpagenumber=" + (Convert.ToInt32(listPageNumber) + 1) + "&souscatname=" + souscatName + "&idcat=" + idCat + "&from=changepage", UriKind.Relative));
        }
    }
}