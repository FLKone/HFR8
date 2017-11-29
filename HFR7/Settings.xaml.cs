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
using System.Windows.Media.Imaging;
using Microsoft.Phone.Scheduler;
using System.IO.IsolatedStorage;
using System.Threading;
using Microsoft.Phone.Shell;

namespace HFR7
{
    public partial class Settings : PhoneApplicationPage
    {
        System.IO.IsolatedStorage.IsolatedStorageSettings store = System.IO.IsolatedStorage.IsolatedStorageSettings.ApplicationSettings;
        IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
        string currentTheme;
        string currentOrientation;
        bool pageLoaded;
        string periodicTaskName = "FavSchedulerAgent";
        public Settings()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if ((string)store["disableLandscape"] == "true") SettingsPA.SupportedOrientations = SupportedPageOrientation.Portrait;
            pageLoaded = true;
            // Background clair ou foncé
            if ((Visibility)Resources["PhoneDarkThemeVisibility"] == System.Windows.Visibility.Visible) currentTheme = "dark";
            else currentTheme = "light";
            //if (SettingsPA.Orientation == PageOrientation.Landscape || SettingsPA.Orientation == PageOrientation.LandscapeLeft || SettingsPA.Orientation == PageOrientation.LandscapeRight) currentOrientation = "landscape";
            //else currentOrientation = "portrait";
            //backgroundImageBrush.ImageSource = new BitmapImage(new Uri("Images/" + currentTheme + "/Background/Background_" + currentOrientation + ".jpg", UriKind.Relative));
            
            // Type de favoris ?
            favorisListPicker.SelectedIndex = Convert.ToInt16((string)store["favorisType"]) - 1;

            // Affichage des avatars ?
            if ((string)store["displayAvatars"] == "always") displayAvatarsListPicker.SelectedIndex = 0;
            if ((string)store["displayAvatars"] == "wifi") displayAvatarsListPicker.SelectedIndex = 1;
            if ((string)store["displayAvatars"] == "never") displayAvatarsListPicker.SelectedIndex = 2;

            // Affichage des images ?
            if ((string)store["displayImages"] == "always") displayImagesListPicker.SelectedIndex = 0;
            if ((string)store["displayImages"] == "wifi") displayImagesListPicker.SelectedIndex = 1;
            if ((string)store["displayImages"] == "never") displayImagesListPicker.SelectedIndex = 2;

            // WebBrowser intern ?
            if ((string)store["disableLandscape"] == "true")
            {
                disableLandscape.IsChecked = true;
                disableLandscape.Content = "Oui";
            }
            else
            {
                disableLandscape.IsChecked = false;
                disableLandscape.Content = "Non";
            }

            // Agent background ?
            if ((string)store["activateFavAgent"] == "true")
            {
                activateFavAgent.IsChecked = true;
                activateFavAgent.Content = "Oui";
            }
            else
            {
                activateFavAgent.IsChecked = false;
                activateFavAgent.Content = "Non";
            }

            // Cache ?
            if ((string)store["activateCache"] == "true")
            {
                activateCache.IsChecked = true;
                activateCache.Content = "Oui";
            }
            else
            {
                activateCache.IsChecked = false;
                activateCache.Content = "Non";
            }

            // Notification MP ?
            if ((string)store["activateMpNotif"] == "true")
            {
                activateMpNotif.IsChecked = true;
                activateMpNotif.Content = "Oui";
            }
            else
            {
                activateMpNotif.IsChecked = false;
                activateMpNotif.Content = "Non";
            }

            // Pinch to zoom ?
            if ((string)store["pinchToZoomOption"] == "true")
            {
                pinchToZoom.IsChecked = true;
                pinchToZoom.Content = "Oui";
            }
            else
            {
                pinchToZoom.IsChecked = false;
                pinchToZoom.Content = "Non";
            }

            // Run under lockscreen ?
            if ((string)store["runUnderLockScreen"] == "true")
            {
                runUnderLockScreen.IsChecked = true;
                runUnderLockScreen.Content = "Oui";
            }
            else
            {
                runUnderLockScreen.IsChecked = false;
                runUnderLockScreen.Content = "Non";
            }

            // Rafraichir les favoris à chaque retour ?
            if ((string)store["refreshFavWP"] == "true")
            {
                refreshFavWP.IsChecked = true;
                refreshFavWP.Content = "Oui";
            }
            else
            {
                refreshFavWP.IsChecked = false;
                refreshFavWP.Content = "Non";
            }

            // Vibrer quand c'est chargé ?
            if ((string)store["vibrateLoad"] == "true")
            {
                vibrateLoad.IsChecked = true;
                vibrateLoad.Content = "Oui";
            }
            else
            {
                vibrateLoad.IsChecked = false;
                vibrateLoad.Content = "Non";
            }

            // Font size
            if ((int)store["fontSizeValue"] == 11) fontSizeListPicker.SelectedIndex = 0;
            if ((int)store["fontSizeValue"] == 13) fontSizeListPicker.SelectedIndex = 1;
            if ((int)store["fontSizeValue"] == 15) fontSizeListPicker.SelectedIndex = 2;
            if ((int)store["fontSizeValue"] == 17) fontSizeListPicker.SelectedIndex = 3;
            if ((int)store["fontSizeValue"] == 19) fontSizeListPicker.SelectedIndex = 3;
            
        }
        private void favorisListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pageLoaded)
            {
                try
                {
                    if (favorisListPicker.SelectedIndex == 0)
                    {
                        store.Remove("favorisType");
                        store.Add("favorisType", "1");
                    }
                    if (favorisListPicker.SelectedIndex == 1)
                    {
                        store.Remove("favorisType");
                        store.Add("favorisType", "2");
                    }
                    if (favorisListPicker.SelectedIndex == 2)
                    {
                        store.Remove("favorisType");
                        store.Add("favorisType", "3");
                    }
                }
                catch { }
            }
        }

        private void displayAvatarsListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pageLoaded)
            {
                try
                {
                    if (displayAvatarsListPicker.SelectedIndex == 0)
                    {
                        store.Remove("displayAvatars");
                        store.Add("displayAvatars", "always");
                    }
                    if (displayAvatarsListPicker.SelectedIndex == 1)
                    {
                        store.Remove("displayAvatars");
                        store.Add("displayAvatars", "wifi");
                    }
                    if (displayAvatarsListPicker.SelectedIndex == 2)
                    {
                        store.Remove("displayAvatars");
                        store.Add("displayAvatars", "never");
                    }
                }
                catch { }
            }

        }

        private void displayImagesListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pageLoaded)
            {
                try
                {
                    if (displayImagesListPicker.SelectedIndex == 0)
                    {
                        store.Remove("displayImages");
                        store.Add("displayImages", "always");
                    }
                    if (displayImagesListPicker.SelectedIndex == 1)
                    {
                        store.Remove("displayImages");
                        store.Add("displayImages", "wifi");
                    }
                    if (displayImagesListPicker.SelectedIndex == 2)
                    {
                        store.Remove("displayImages");
                        store.Add("displayImages", "never");
                    }
                }
                catch { }
            }

        }

        private void disableLandscape_Checked(object sender, RoutedEventArgs e)
        {
            store.Remove("disableLandscape");
            store.Add("disableLandscape", "true");
            disableLandscape.Content = "Oui";
        }

        private void disableLandscape_Unchecked(object sender, RoutedEventArgs e)
        {
            store.Remove("disableLandscape");
            store.Add("disableLandscape", "false");
            disableLandscape.Content = "Non";
        }



        private void activateFavAgent_Checked(object sender, RoutedEventArgs e)
        {
            store.Remove("activateFavAgent");
            store.Add("activateFavAgent", "true");
            activateFavAgent.Content = "Oui";
            PeriodicTask periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;
            StartPeriodicAgent();
        }

        private void activateFavAgent_Unchecked(object sender, RoutedEventArgs e)
        {
            store.Remove("activateFavAgent");
            store.Add("activateFavAgent", "false");
            activateFavAgent.Content = "Non";
            PeriodicTask periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;
            RemoveAgent(periodicTaskName);
            activateMpNotif.IsChecked = false;
        }


        private void activateCache_Checked(object sender, RoutedEventArgs e)
        {
            store.Remove("activateCache");
            store.Add("activateCache", "true");
            activateCache.Content = "Oui";
        }

        private void activateCache_Unchecked(object sender, RoutedEventArgs e)
        {
            store.Remove("activateCache");
            store.Add("activateCache", "false");
            activateCache.Content = "Non";
        }

        private void vibrateLoad_Checked(object sender, RoutedEventArgs e)
        {
            store.Remove("vibrateLoad");
            store.Add("vibrateLoad", "true");
            vibrateLoad.Content = "Oui";
        }

        private void vibrateLoad_Unchecked(object sender, RoutedEventArgs e)
        {
            store.Remove("vibrateLoad");
            store.Add("vibrateLoad", "false");
            vibrateLoad.Content = "Non";
        }


        private void pinchToZoom_Checked(object sender, RoutedEventArgs e)
        {
            store.Remove("pinchToZoomOption");
            store.Add("pinchToZoomOption", "true");
            pinchToZoom.Content = "Oui";
        }

        private void pinchToZoom_Unchecked(object sender, RoutedEventArgs e)
        {
            store.Remove("pinchToZoomOption");
            store.Add("pinchToZoomOption", "false");
            pinchToZoom.Content = "Non";
        }


        private void activateMpNotif_Checked(object sender, RoutedEventArgs e)
        {
            activateFavAgent.IsChecked = true;
            store.Remove("activateMpNotif");
            store.Add("activateMpNotif", "true");
            activateMpNotif.Content = "Oui";
        }

        private void activateMpNotif_Unchecked(object sender, RoutedEventArgs e)
        {
            store.Remove("activateMpNotif");
            store.Add("activateMpNotif", "false");
            activateMpNotif.Content = "Non";
        }

        private void StartPeriodicAgent()
        {
            string periodicTaskName = "FavSchedulerAgent";
            PeriodicTask periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;

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
                RemoveAgent(periodicTaskName);
            }

            periodicTask = new PeriodicTask(periodicTaskName);
            // The description is required for periodic agents. This is the string that the user
            // will see in the background services Settings page on the device.
            periodicTask.Description = "Ce service permet de vérifier toutes les 30 minutes si un des sujets épinglés en page d'accueil a reçu une réponse, et actualise le nombre de favoris non lus sur la tuile HFR7. Il consomme environ 9 ko par requête.";

            ScheduledActionService.Add(periodicTask);
        }

        private void RemoveAgent(string name)
        {
            try
            {
                ScheduledActionService.Remove(name);
                ShellTile MainTileToFind = ShellTile.ActiveTiles.FirstOrDefault();

                StandardTileData MainNewAppTileData = new StandardTileData
                {
                    Title = "hfr8",
                    Count = 0
                };
                MainTileToFind.Update(MainNewAppTileData);
            }
            catch (Exception)
            {
            }
        }

        // Interception bouton back
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            store.Save();
            e.Cancel = true;
            NavigationService.GoBack();
        }

        //private void favorisListPicker_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    favorisListPicker.Open();
        //}

        //private void displayAvatarsListPicker_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    displayAvatarsListPicker.Open();
        //}

        //private void displayImagesListPicker_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        //{
        //    displayImagesListPicker.Open();
        //}

        private void runUnderLockScreen_Checked(object sender, RoutedEventArgs e)
        {
            store.Remove("runUnderLockScreen");
            store.Add("runUnderLockScreen", "true");
            runUnderLockScreen.Content = "Oui";
        }

        private void runUnderLockScreen_Unchecked(object sender, RoutedEventArgs e)
        {
            store.Remove("runUnderLockScreen");
            store.Add("runUnderLockScreen", "false");
            runUnderLockScreen.Content = "Non";
        }

        private void refreshFavWP_Checked(object sender, RoutedEventArgs e)
        {
            store.Remove("refreshFavWP");
            store.Add("refreshFavWP", "true");
            refreshFavWP.Content = "Oui";
        }

        private void refreshFavWP_Unchecked(object sender, RoutedEventArgs e)
        {
            store.Remove("refreshFavWP");
            store.Add("refreshFavWP", "false");
            refreshFavWP.Content = "Non";
        }

        private void fontSizeListPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pageLoaded)
            {
                try
                {
                    if (fontSizeListPicker.SelectedIndex == 0)
                    {
                        store.Remove("fontSizeValue");
                        store.Add("fontSizeValue", 11);
                    }
                    if (fontSizeListPicker.SelectedIndex == 1)
                    {
                        store.Remove("fontSizeValue");
                        store.Add("fontSizeValue", 13);
                    }
                    if (fontSizeListPicker.SelectedIndex == 2)
                    {
                        store.Remove("fontSizeValue");
                        store.Add("fontSizeValue", 15);
                    }
                    if (fontSizeListPicker.SelectedIndex == 3)
                    {
                        store.Remove("fontSizeValue");
                        store.Add("fontSizeValue", 17);
                    }
                    if (fontSizeListPicker.SelectedIndex == 4)
                    {
                        store.Remove("fontSizeValue");
                        store.Add("fontSizeValue", 19);
                    }
                }
                catch { }
            }

        }      
    }
}