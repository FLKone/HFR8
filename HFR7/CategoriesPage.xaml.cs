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
using HtmlAgilityPack;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Linq;
using System.Windows.Data;
using System.Xml.Serialization;
using System.Windows.Media.Imaging;


namespace HFR7
{
    public partial class CategoriesPage : PhoneApplicationPage
    {
        System.IO.IsolatedStorage.IsolatedStorageSettings store = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
        XDocument docCategories = new XDocument();
        bool groupViewOpened = false;

        // Instanciation de la classe
        List<HFRClasses.Categories> catObject = new List<HFRClasses.Categories>();

        public CategoriesPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
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

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (groupViewOpened == true)
            {
                e.Cancel = true;
            }
            else
            {
                NavigationService.Navigate(new Uri("/WelcomePage.xaml", UriKind.Relative));
                e.Cancel = true;
            }
            
        }

        private void CategoriesPagePA_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            ImageBrush brush = new ImageBrush();
            switch (e.Orientation)
            {
                case PageOrientation.Landscape:
                case PageOrientation.LandscapeLeft:
                case PageOrientation.LandscapeRight:
                    brush.ImageSource = new BitmapImage(new Uri("Background_Landscape.jpg", UriKind.Relative));
                    break;
 
                case PageOrientation.Portrait:
                case PageOrientation.PortraitUp:
                case PageOrientation.PortraitDown:
                    brush.ImageSource = new BitmapImage(new Uri("Background_Portrait.jpg", UriKind.Relative));
                    break;
            }
            LayoutRoot.Background = brush;
        }

        private void categoriesGroup_GroupViewOpened(object sender, GroupViewOpenedEventArgs e)
        {
            groupViewOpened = true;
        }

        private void categoriesGroup_GroupViewClosing(object sender, GroupViewClosingEventArgs e)
        {
            groupViewOpened = false;
        }
    }
}