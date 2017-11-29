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
using System.IO.IsolatedStorage;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.Phone.Shell;
using HFR7.HFRClasses;

namespace HFR7
{
    public partial class FromTileToTopic : PhoneApplicationPage
    {
        CookieContainer container = new CookieContainer();
        IsolatedStorageSettings store = IsolatedStorageSettings.ApplicationSettings;
        IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        List<HFRClasses.TopicFav> favObject = new List<HFRClasses.TopicFav>();
        string topicIdTile;
        string uriTopic;
        string pageNumberTile;
        string reponseIdTile;
        string numberOfPagesTile;
        string idCatTile;
        string topicNameTile;

        public FromTileToTopic()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Récupération de l'ID du topic
            NavigationContext.QueryString.TryGetValue("idtopic", out topicIdTile);

            // Récupération du numéro de la page
            NavigationContext.QueryString.TryGetValue("pagenumber", out pageNumberTile);

            // Récupération de l'anchor
            NavigationContext.QueryString.TryGetValue("reponseid", out reponseIdTile);

            // Récupération du nombre de pages
            NavigationContext.QueryString.TryGetValue("numberofpages", out numberOfPagesTile);

            // Récupération de l'ID de la catégorie
            NavigationContext.QueryString.TryGetValue("idcat", out idCatTile);

            // Récupération du nom du topic
            NavigationContext.QueryString.TryGetValue("topicname", out topicNameTile);
            topicNameTile = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(topicNameTile));

            // Nettoyage du cache
            if (isoStore.DirectoryExists("topics"))
            {
                foreach (string fileName in isoStore.GetFileNames("topics/*"))
                {
                    isoStore.DeleteFile("topics/" + fileName);
                }
            }

            // Récupération du cookie du store
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
                FindFav();
            }
            else
            {
                MessageBox.Show("Une erreur de récupération des cookies s'est produite.");
            }
        }


        private void FindFav()
        {
            // Affichage favoris
            favObject.Clear();
            string urlFav = "http://forum.hardware.fr/forum1f.php?owntopic=" + (string)store["favorisType"];
            HtmlWeb.LoadAsync(urlFav, container as CookieContainer, (s, args) =>
            {
                if (args.Error != null)
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Impossible d'accéder au serveur. Vérifiez votre connectivité ou l'état de HFR.");
                        progressBar.Visibility = System.Windows.Visibility.Collapsed;
                        globalTextblock.Text = "erreur !";
                    });
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

                    string[] mpArray = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "red").
                                                Select(y => y.InnerText).ToArray();

                    string numberOfPagesTopicLine = "";
                    int j = 0;

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
                                    BackContent = "Pas de nouveaux messages",
                                    BackgroundImage = new Uri("Images/tiles/" + HFRClasses.GetCatName.ShortNameFromId(tileCatId) + ".png", UriKind.Relative),
                                    BackBackgroundImage = new Uri("Images/tiles/read.png", UriKind.Relative)

                                };
                                // Update the Application Tile
                                tile.Update(NewTileDataRead);
                            }
                        }
                    }
                    ////Mise à jour de la tile principale
                    //int numberOfUnreadFav = favObject.Count;
                    //ShellTile MainTileToFind = ShellTile.ActiveTiles.FirstOrDefault();

                    //StandardTileData MainNewAppTileData = new StandardTileData
                    //{
                    //    Count = numberOfUnreadFav
                    //};
                    //MainTileToFind.Update(MainNewAppTileData);
                    // Recherche du topic
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

                            if (topicId == topicIdTile)
                            {
                                uriTopic = "/ReadTopic.xaml?idcat=" + topicCatId + "&idtopic=" + topicIdTile + "&topicname=" + HttpUtility.UrlEncode(line) + "&pagenumber=" + pageNumber + "&jump=" + reponseId + "&numberofpages=" + numberOfPagesTopicLine + "&back=fftt";
                                if (numberOfPagesTopicLine == pageNumber)
                                {
                                    foreach (ShellTile tile in ShellTile.ActiveTiles)
                                    {
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

                                            if (tilePostId == topicId)
                                            {
                                                StandardTileData NewTileData = new StandardTileData
                                                {
                                                    BackContent = "Pas de nouveaux messages",
                                                    BackgroundImage = new Uri("Images/tiles/" + HFRClasses.GetCatName.ShortNameFromId(tileCatId) + ".png", UriKind.Relative),
                                                    BackBackgroundImage = new Uri("Images/tiles/read.png", UriKind.Relative)
                                                };
                                                tile.Update(NewTileData);
                                            }
                                        }
                                    }
                                }
                            }
                            j++;
                        }
                        i++;
                    }

                    if (uriTopic == null)
                    {
                        uriTopic = "/ReadTopic.xaml?idcat=" + idCatTile + "&idtopic=" + topicIdTile + "&topicname=" + topicNameTile + "&pagenumber=" + pageNumberTile + "&jump=bas&numberofpages=" + numberOfPagesTile + "&page=" + numberOfPagesTile + "&back=fftt";
                    }
                    Dispatcher.BeginInvoke(() => NavigationService.Navigate(new Uri(uriTopic, UriKind.Relative)));
                }

            });
        }
    }
}