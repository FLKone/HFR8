using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Collections.Generic;

namespace HFR7.HFRClasses
{
    public class AppSettings
    {
        public string settingName { get; set; }
        public object settingValue { get; set; }



        public static void CheckAndAdd(string currentVersion)
        {
            string activateFavAgent;
            string activateMpNotif;
            string runUnderLockscreen;
            // Activer les background agent et notifications ??           
            MessageBoxResult activateMpNotifResult = MessageBox.Show("Voulez-vous activer les notifications de nouveau message privé ? Cette fonction nécessite l'activation d'un agent de fond qui utilise 20 ko de connexion data par heure. Celui-ci est désactivable dans les préférences de l'appli.", "Activer la notification de nouveau message privé ?", MessageBoxButton.OKCancel);
            if (activateMpNotifResult == MessageBoxResult.Cancel)
            {
                activateMpNotif = "false";
                MessageBoxResult activateFavAgentResult = MessageBox.Show("Voulez-vous activer l'agent de mise à jour des live tiles des sujets ? Cette fonction permet de mettre à jour automatiquement l'état (nouveaux messages ou non) des sujets que vous avez épinglés en page d'accueil. Ce service consomme environ 20 ko par heure. Celui-ci est désactivable dans les préférences de l'appli.", "Activer l'agent de mise à jour des live tiles ?", MessageBoxButton.OKCancel);
                if (activateFavAgentResult == MessageBoxResult.Cancel)
                {
                    activateFavAgent = "false";
                }
                else if (activateFavAgentResult == MessageBoxResult.OK)
                {
                    activateFavAgent = "true";
                }
                else
                {
                    activateFavAgent = "false";
                }
            }
            else if (activateMpNotifResult == MessageBoxResult.OK)
            {
                activateFavAgent = "true";
                activateMpNotif = "true";
            }
            else
            {
                activateFavAgent = "false";
                activateMpNotif = "false";
            }

            // Activer le run under lockscreen ?
            MessageBoxResult lockScreenNotifResult = MessageBox.Show("Si vous êtes en train de charger un sujet et que vous mettez votre téléphone en veille/que l'écran s'éteint, ce service vous permettra d'éviter bon nombre d'erreurs (il permet à la connexion de persister le temps que le sujet soit téléchargé). Ce service est désactivable dans les préférences de l'appli et votre choix sera pris en compte lors du prochain lancement.", "Autoriser le fonctionnement lors de l'écran éteint ?", MessageBoxButton.OKCancel);
            if (lockScreenNotifResult == MessageBoxResult.Cancel)
            {
                runUnderLockscreen = "false";
            }
            else
            {
                runUnderLockscreen = "true";
            }

            IsolatedStorageSettings store = IsolatedStorageSettings.ApplicationSettings;
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            List<AppSettings> listSettings = new List<AppSettings>();
            listSettings.Add(new AppSettings() { settingName = "favorisType", settingValue = "1" }); // Type de favoris ? (Par défaut drapeaux cyans + étoiles)
            listSettings.Add(new AppSettings() { settingName = "displayImages", settingValue = "always" });  // Afficher les images ? (Par défaut "Toujours")
            listSettings.Add(new AppSettings() { settingName = "displayAvatars", settingValue = "always" }); // Afficher les avatars ? (Par défaut "Toujours")
            listSettings.Add(new AppSettings() { settingName = "disableLandscape", settingValue = "false" }); // Désactiver le mode paysage ? (Par défaut "Non")
            listSettings.Add(new AppSettings() { settingName = "refreshFavWP", settingValue = "false" }); // Rafraichir à chaque retour ? (Par défaut "Non")
            listSettings.Add(new AppSettings() { settingName = "activateCache", settingValue = "true" }); // Activer le préchargement ? (Par défaut "Oui")
            listSettings.Add(new AppSettings() { settingName = "pinchToZoomOption", settingValue = "false" }); // Activer le pinch to zoom ? (Par défaut "Non")
            listSettings.Add(new AppSettings() { settingName = "isMpNotified", settingValue = "false" }); // Création du mp notified
            listSettings.Add(new AppSettings() { settingName = "launch", settingValue = (bool)true }); // Premier lancement
            listSettings.Add(new AppSettings() { settingName = "activateFavAgent", settingValue = activateFavAgent });
            listSettings.Add(new AppSettings() { settingName = "activateMpNotif", settingValue = activateMpNotif });
            listSettings.Add(new AppSettings() { settingName = "runUnderLockScreen", settingValue = runUnderLockscreen });
            listSettings.Add(new AppSettings() { settingName = "isConnected", settingValue = "true" });
            listSettings.Add(new AppSettings() { settingName = "vibrateLoad", settingValue = "true" });
            listSettings.Add(new AppSettings() { settingName = "fontSizeValue", settingValue = 13 });
            listSettings.Add(new AppSettings() { settingName = "currentVersion", settingValue = currentVersion });


            foreach (AppSettings setting in listSettings)
            {
                if (!store.Contains(setting.settingName))
                {
                    store.Add(setting.settingName, setting.settingValue);
                }
            }

            store.Remove("currentVersion");
            store.Add("currentVersion", currentVersion);

            if (store.Contains("userPseudo"))
            {
                string pseudoTemp = (string)store["userPseudo"];
                store.Remove("userPseudo");
                store.Add("userPseudo", pseudoTemp);
            }

            if (!isoStore.DirectoryExists("topics")) isoStore.CreateDirectory("topics"); // Création du dossier topics
        }
    }
}
