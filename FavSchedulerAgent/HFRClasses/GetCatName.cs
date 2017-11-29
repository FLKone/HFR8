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

namespace HFR7.HFRClasses
{
    public class GetCatName
    {
        public static string PlainNameFromId(string id)
        {
            if (id == "1") { return("Hardware"); }
            if (id == "16") { return("Hardware & Périphériques"); }
            if (id == "15") { return("Ordinateurs portables"); }
            if (id == "23") { return("Technologies Mobiles"); }
            if (id == "2") { return("Overclocking, Cooling & Tuning"); }
            if (id == "25") { return("Apple"); }
            if (id == "3") { return("Vidéo & Son"); }
            if (id == "14") { return("Photo Numérique"); }
            if (id == "5") { return("Jeux-vidéo"); }
            if (id == "4") { return("Windows & Software"); }
            if (id == "22") { return("Réseaux grand public/SoHo"); }
            if (id == "21") { return("Systèmes & Réseaux Pro"); }
            if (id == "11") { return("OS Alternatifs"); }
            if (id == "10") { return("Programmation"); }
            if (id == "12") { return("Graphisme"); }
            if (id == "6") { return("Achats & Ventes"); }
            if (id == "8") { return("Emploi & Etudes"); }
            if (id == "9") { return("Seti et projets distribués"); }
            if (id == "13") { return ("Discussions"); }
            else return ("Erreur");
        }

        public static string ShortNameFromId(string id)
        {
            if (id == "1") { return ("Hardware"); }
            if (id == "16") { return ("HardwarePeripheriques"); }
            if (id == "15") { return ("OrdinateursPortables"); }
            if (id == "23") { return ("gsmgpspda"); }
            if (id == "2") { return ("OverclockingCoolingTuning"); }
            if (id == "25") { return ("apple"); }
            if (id == "3") { return ("VideoSon"); }
            if (id == "14") { return ("Photonumerique"); }
            if (id == "5") { return ("JeuxVideo"); }
            if (id == "4") { return ("WindowsSoftware"); }
            if (id == "22") { return ("reseauxpersosoho"); }
            if (id == "21") { return ("systemereseauxpro"); }
            if (id == "11") { return ("OSAlternatifs"); }
            if (id == "10") { return ("Programmation"); }
            if (id == "12") { return ("Graphisme"); }
            if (id == "6") { return ("AchatsVentes"); }
            if (id == "8") { return ("EmploiEtudes"); }
            if (id == "9") { return ("Setietprojetsdistribues"); }
            if (id == "13") { return ("Discussions"); }
            else return ("Discussions");
        }
    }
}
