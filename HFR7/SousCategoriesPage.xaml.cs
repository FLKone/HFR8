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
using System.Xml.Linq;
using System.IO;
using HtmlAgilityPack;

namespace HFR7
{
    public partial class SousCategoriesPage : PhoneApplicationPage
    {
        XDocument docSousCat = new XDocument();
        string catUriShort;
        string catUri;
        string catName;
        public SousCategoriesPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            //Récupération de l'URI de la catégorie
            NavigationContext.QueryString.TryGetValue("caturi", out catUri);
            ////Réduction de l'URI
            catUriShort = catUri.Substring(4).Replace("/", "-");
            //Récupération du nom du forum
            NavigationContext.QueryString.TryGetValue("catname", out catName);
            ApplicationTitle.Text = catName.ToUpper();


            //Vérification si premier lancement de la catégorie
            var appStorage = IsolatedStorageFile.GetUserStoreForApplication();
            string fileName = "firstlaunch" + catUriShort + ".txt";
            if (!appStorage.FileExists(fileName))
            {
                //PREMIER LANCEMENT
                ////Affichage du canvas d'attente
                premierLancementCanvas.Visibility = System.Windows.Visibility.Visible;
                ////Création du XML dans l'ISF
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("souscat" + catUriShort + ".xml", FileMode.Create, isoStore))
                {
                    XDocument docModel = XDocument.Load("XMLFile.xml");
                    docModel.Save(isoStream);
                }
                using (IsolatedStorageFileStream isoStreamCategories = new IsolatedStorageFileStream("souscat" + catUriShort + ".xml", FileMode.Open, isoStore))
                {
                    docSousCat = XDocument.Load(isoStreamCategories);
                }

                ////Chargement des catégories vers le XML
                IsolatedStorageFileStream isoStreamSave = new IsolatedStorageFileStream("souscat" + catUriShort + ".xml", FileMode.Create, isoStore);
                HtmlWeb.LoadAsync("http://forum.hardware.fr" + catUri, null, (s, args) =>
                {
                    int i = 0;
                    string[] categories = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "cHeader").
                                                Select(y => y.InnerText).ToArray();
                    string[] uriCategorie = args.Document.DocumentNode.Descendants("a").Where(x => (string)x.GetAttributeValue("class", "") == "cHeader").
                            Select(x => x.GetAttributeValue("href", "")).ToArray();

                    foreach (string line in categories)
                    {
                        XElement categorie = new XElement("SousCategorie", new XAttribute("Name", HttpUtility.HtmlDecode(line)), new XAttribute("Uri", uriCategorie[i]));
                        docSousCat.Root.Add(categorie);
                        i++;
                    }
                    docSousCat.Save(isoStreamSave);
                    isoStreamSave.Close();
                    LoadSousCategories();
                    premierLancementCanvas.Visibility = System.Windows.Visibility.Collapsed;
                });
                ////Sauvegarde du premier lancement
                using (var file = appStorage.OpenFile(fileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
                {
                    using (var writer = new StreamWriter(file))
                    {
                        writer.Write("true");
                    }
                }
            }
            else
            {
                LoadSousCategories();
            }
        }

        private void LoadSousCategories()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("souscat" + catUriShort + ".xml", FileMode.Open, isoStore);
            XDocument docCategories = XDocument.Load(isoStream);
            //Ajout du Hyperlink "Toutes"
            var linqArray = docCategories.Root.Descendants("SousCategorie");
            HyperlinkButton toutesCatHLB = new HyperlinkButton();
            toutesCatHLB.FontSize = 35;
            toutesCatHLB.Style = (Style)this.Resources["HyperlinkButtonStyle1"];
            toutesCatHLB.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            toutesCatHLB.NavigateUri = new Uri("/HFR7;component/ListTopics.xaml?souscaturi=" + catUri + "&souscatname=" + catName, UriKind.Relative);
            toutesCatHLB.Content = "Toutes";
            souscategorieSP.Children.Add(toutesCatHLB);

            foreach (var component in linqArray)
            {
                try
                {
                    Convert.ToInt32(component.Attribute("Name").Value);
                }

                catch
                {
                    HyperlinkButton newHLB = new HyperlinkButton();
                    newHLB.FontSize = 35;
                    newHLB.Style = (Style)this.Resources["HyperlinkButtonStyle1"];
                    newHLB.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    newHLB.NavigateUri = new Uri("/HFR7;component/ListTopics.xaml?souscaturi=" + component.Attribute("Uri").Value + "&souscatname=" + component.Attribute("Name").Value.Replace("&", "et"), UriKind.Relative);
                    newHLB.Content = component.Attribute("Name").Value;
                    souscategorieSP.Children.Add(newHLB);
                }
            }
            isoStream.Close();

        }
    }
}