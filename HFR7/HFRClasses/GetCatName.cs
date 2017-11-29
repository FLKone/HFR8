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
            if (id == "16") { return("Périphériques"); }
            if (id == "15") { return("PC portables"); }
            if (id == "23") { return("Techno. Mobiles"); }
            if (id == "2") { return("Modding"); }
            if (id == "25") { return("Apple"); }
            if (id == "3") { return("Vidéo & Son"); }
            if (id == "14") { return("Photo Numérique"); }
            if (id == "5") { return("Jeux-vidéo"); }
            if (id == "4") { return("Windows & Software"); }
            if (id == "22") { return ("Réseaux grand public"); }
            if (id == "21") { return("Réseaux Pro"); }
            if (id == "11") { return("OS Alternatifs"); }
            if (id == "10") { return("Programmation"); }
            if (id == "12") { return("Graphisme"); }
            if (id == "6") { return("Achats & Ventes"); }
            if (id == "8") { return("Emploi & Etudes"); }
            if (id == "9") { return("Projets distribués"); }
            if (id == "13") { return ("Discussions"); }
            if (id == "0") { return ("Section réservée"); }
            else return ("Erreur");
        }

        public static string IdFromPlainName(string name)
              {
            if (name == "Hardware") return ("1");
            if (name == "Périphériques") return ("16");
            if (name == "PC portables") return ("15");
            if (name == "Techno. Mobiles") return ("23");
            if (name == "Modding") return ("2");
            if (name == "Apple") return ("25");
            if (name == "Vidéo & Son") return ("3");
            if (name == "Photo Numérique") return ("14");
            if (name == "Jeux-vidéo") return ("5");
            if (name == "Windows & Software") return ("4");
            if (name == "Réseaux grand public") return ("22");
            if (name == "Réseaux Pro") return ("21");
            if (name == "OS Alternatifs") return ("11");
            if (name == "Programmation") return ("10");
            if (name == "Graphisme") return ("12");
            if (name == "Achats & Ventes") return ("6");
            if (name == "Emploi & Etudes") return ("8");
            if (name == "Projets distribués") return ("9");
            if (name == "Discussions") return ("13");
            if (name == "Section réservée") return ("0");
            else return ("13");
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
