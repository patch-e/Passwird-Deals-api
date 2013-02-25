using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Net;
using System.Configuration;

using api;
using api.Deals.Version1;
using Newtonsoft.Json;
using MoonAPNS;
using NLog;

namespace PushPasswird
{
    /// <summary>
    /// Main program
    /// </summary>
	class Program
	{
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		static void Main(string[] args)
		{
            //setup app name
            var appName = ConfigurationManager.AppSettings[@"appName"];

            //setup key values
            var isDevelopment = Convert.ToBoolean(ConfigurationManager.AppSettings[@"isDevelopment"]);
            var devKey = ConfigurationManager.AppSettings[@"devKeyFile"];
            var key = ConfigurationManager.AppSettings[@"keyFile"];
            if (isDevelopment) { key = devKey; }
            
            //set api endpoint
            var apiUrl = ConfigurationManager.AppSettings[@"apiUrl"];

            //polling amount in minutes
            var pollingMinutes = Convert.ToInt32(ConfigurationManager.AppSettings[@"pollingMinutes"]);

            //infinite pooling loop
            while (true)
            {
                var shouldSendNotification = false;
                LastDeal lastDeal;
                Deal currentDeal;
                RootDeal rootDeal;

                //grab a copy of current deals
                using (WebClient wc = new WebClient())
                {
                    try
                    {
                        //parse deal json into objects
                        var json = wc.DownloadString(apiUrl);
                        rootDeal = JsonConvert.DeserializeObject<RootDeal>(json);
                        currentDeal = new Deal(rootDeal.deals[0]);

                        //Console.WriteLine("\n >Got deals: " + rootDeal.deals.Count.ToString());
                    }
                    catch
                    {
                        Console.WriteLine(@"!Failed to parse deals.");
                        continue;
                    }
                }

                //reset loop if no deals were in the collection
                if (rootDeal.deals.Count <= 0)
                {
                    Console.WriteLine(@"!No deals found.");
                    continue;
                }

                try
                {
                    //get the most recent lastDeal saved to the database for comparison
                    using (PushModelContainer context = new PushModelContainer())
                    {
                        lastDeal = (from ld in context.LastDeals
                                    orderby ld.id descending
                                    select ld).FirstOrDefault();
                    }
                }
                catch
                {
                    Console.WriteLine(@"!Could not fetch last deal.");
                    continue;
                }
                
                //compare the last deal with the current deal
                //Console.WriteLine(@" >Comparing    (lastDeal): " + lastDeal.headline);
                //Console.WriteLine(@" >Comparing (currentDeal): " + currentDeal.headline);
                //considering the headline + datePosted to be unique key for deals
                if ( (currentDeal.headline == lastDeal.headline &&
                    currentDeal.datePosted == lastDeal.datePosted ) ||
                    currentDeal.headline == "Currently experiencing technical difficulties.")
                {
                    //found the previous lastDeal
                    //Logger.Info(@" >Found repeat deal.");
                }
                else
                {
                    //found a new deal, send a notification
                    Logger.Info(@" >Found new deal.");
                    shouldSendNotification = true;
                    if (lastDeal.headline == "root") shouldSendNotification = true;

                    try
                    {
                        //save the current deal as the last deal to the database
                        using (PushModelContainer context = new PushModelContainer())
                        {
                            //save the current deal as a new last deal record in the database
                            lastDeal = new LastDeal();
                            lastDeal.headline = currentDeal.headline;
                            lastDeal.datePosted = currentDeal.datePosted;
                            lastDeal.body = currentDeal.body;
                            lastDeal.image = currentDeal.image;
                            lastDeal.isExpired = currentDeal.isExpired;

                            context.LastDeals.AddObject(lastDeal);
                            context.SaveChanges();
                        }
                    }
                    catch
                    {
                        Console.WriteLine(@"!Failed to save last deal.");
                        continue;
                    }
                }

                if (shouldSendNotification)
                {
                    //configure and start Apple APNS
                    PushNotification push;
                    try
                    {
                        push = new PushNotification(false, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, key), ConfigurationManager.AppSettings["certPassword"]);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(@"!Failed to start service.");
                        continue;
                    }

                    IList<DeviceToken> deviceTokens = null;
                    try
                    {
                        //get a list of all registered device tokens
                        using (PushModelContainer context = new PushModelContainer())
                        {
                            var q = (from dt in context.DeviceTokens
                                     where dt.Development.Equals(isDevelopment)
                                     where dt.AppName.Equals(appName)
                                     select dt);
                            deviceTokens = q.ToList<DeviceToken>();

                            foreach (DeviceToken deviceToken in deviceTokens)
                            {
                                deviceToken.BadgeCount++;
                            }

                            context.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(@"!Failed to fetch device tokens.");
                        continue;
                    }

                    //hold a list of payloads to be pushed
                    var payloads = new List<NotificationPayload> { };

                    //loops for every registered device token
                    foreach (DeviceToken deviceToken in deviceTokens)
                    {
                        //add the payload to the list
                        var payload = new NotificationPayload(deviceToken.Token, modifyHeadline(currentDeal.headline), deviceToken.BadgeCount, "default");
                        payloads.Add(payload);
                    }

                    //send the payloads and get a list of rejected payloads for deletion
                    Logger.Info(@" >Delivering payloads...");
                    var rejectedTokens = push.SendToApple(payloads);
                    if (rejectedTokens.Count > 0)
                    {
                        //delete all rejected device tokens
                        using (PushModelContainer context = new PushModelContainer())
                        {
                            foreach (var rejectedToken in rejectedTokens)
                            {
                                Logger.Error(@"Deleting token: " + rejectedToken);
                                DeviceToken dt = new DeviceToken();
                                dt.Token = rejectedToken;

                                context.DeviceTokens.DeleteObject(dt);
                            }
                            context.SaveChanges();
                        }
                    }
                    Logger.Info(@" >All payloads delivered.");
                }
                else
                {
                    //sleep the thread for a duration before repeating the while loop
                    //Console.WriteLine(String.Format(" >Sleeping for {0} minute(s)...", pollingMinutes));
                    Thread.Sleep((pollingMinutes * 60) * 1000);
                    //Console.WriteLine(@" >Waking up...");
                }
            }
		}

        #region Utilities
        private static String modifyHeadline(String headline)
        {
            headline = headline.Replace(@"""", @"");
            headline = headline.Replace(@"&amp;", @"&");
            return headline.Truncate(200);
        }
        #endregion
	}

    /// <summary>
    /// Provides extensions to the String class.
    /// </summary>
    public static class StringExt
    {
        //Truncate string extension
        //String.Truncate(int)
        public static string Truncate(this string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}