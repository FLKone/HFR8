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
    public class MpDate
    {
        public static string Run(DateTime date)
        {
            if (date.Ticks > 0)
            {
                if (date.Year != DateTime.Now.Year)
                {
                    return (date.Day + "/" + date.Month + "/" + date.Year.ToString().Substring(2, 2));
                }
                else
                {
                    if (DateTime.Now.Subtract(date).Days > 6)
                    {
                        return (date.Day + "/" + date.Month);
                    }
                    else
                    {
                        if (DateTime.Now.Subtract(date).Days >= 1 || DateTime.Now.Subtract(date).Hours > DateTime.Now.Hour)
                        {
                            if (date.DayOfWeek.ToString() == "Monday") return ("lun.");
                            if (date.DayOfWeek.ToString() == "Tuesday") return ("mar.");
                            if (date.DayOfWeek.ToString() == "Wednesday") return ("mer.");
                            if (date.DayOfWeek.ToString() == "Thursday") return ("jeu.");
                            if (date.DayOfWeek.ToString() == "Friday") return ("ven.");
                            if (date.DayOfWeek.ToString() == "Saturday") return ("sam.");
                            if (date.DayOfWeek.ToString() == "Sunday") return ("dim.");
                            else return ("");

                        }
                        else
                        {
                            return (date.ToString("HH:mm"));
                        }
                    }
                }
            }
            else
            {
                return ("");
            }
        }
    }
}
