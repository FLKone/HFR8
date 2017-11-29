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
    public class TimeSpentTopic
    {
        public static string Run(TimeSpan timeSpent, string favorisLastPostUser)
        {
            if (timeSpent.Ticks > 0)
            {
                if (timeSpent.Days > 365)
                {
                    return("il y a + d'un an par " + favorisLastPostUser);
                }
                else
                {
                    if (timeSpent.Days > 31)
                    {
                        return("il y a " + Math.Round(Convert.ToDecimal(timeSpent.Days / 31), 0).ToString() + " mois par " + favorisLastPostUser);
                    }
                    else
                    {
                        if (timeSpent.Days > 1)
                        {
                            return("il y a " + timeSpent.Days + " jours par " + favorisLastPostUser);
                        }
                        else
                        {
                            if (timeSpent.Days == 1)
                            {
                                return("hier par " + favorisLastPostUser);
                            }
                            else
                            {
                                if (timeSpent.Hours > 1)
                                {
                                    return("il y a " + timeSpent.Hours + "h par " + favorisLastPostUser);
                                }
                                else
                                {
                                    if (timeSpent.Hours == 1)
                                    {
                                        return("il y a 1h par " + favorisLastPostUser);
                                    }
                                    else
                                    {
                                        if (timeSpent.Minutes > 1)
                                        {
                                            return("il y a " + timeSpent.Minutes + " min par " + favorisLastPostUser);
                                        }
                                        else
                                        {
                                            if (timeSpent.Minutes == 1)
                                            {
                                                return ("il y a 1 min par " + favorisLastPostUser);
                                            }
                                            else
                                            {
                                                return ("il y a " + timeSpent.Seconds + "s par " + favorisLastPostUser);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return("dernier message par " + favorisLastPostUser);
            }
        }
    }
}
