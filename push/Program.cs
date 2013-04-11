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

/// <summary>
/// Console application that initializes a PasswirdPoller object which spawns a new thread every minute
/// to consume the Passwird Deals API. This thread compares the newest deal from the API call with the 
/// previous newest deal that was persisted to a database during a previous call.
/// 
/// If the comparison doesn't match, the thread gets a list of registered device tokens and sends a
/// push notification payload via the Apple APNS.
/// </summary>
class PasswirdPoller {

    #region Logging

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region Properties

    public string appName { get; set; }
    public bool isDevelopment { get; set; }
    public string devKey { get; set; }
    public string key { get; set; }
    public string apiUrl { get; set; }
    public int pollingMinutes { get; set; }

    #endregion

    #region Instance Methods

    /// <summary>
    /// Access the Passwird Deals API for a list of deals. Checks the newest deal posted against
    /// a deal that was persisted to the database. If they do not match, push notifications are sent
    /// to all registered devices for the new deal.
    /// </summary>
    private void PollDeals() {
        bool isNewDeal = false, shouldSendNotification = false;
        LastDeal lastDeal;
        Deal currentDeal;
        RootDeal rootDeal;

        //grab a copy of current deals
        using (WebClient wc = new WebClient())
        {
            try {
                //parse deal json into objects
                var json = wc.DownloadString(this.apiUrl);
                rootDeal = JsonConvert.DeserializeObject<RootDeal>(json);
                currentDeal = new Deal(rootDeal.deals[0]);
            }
            catch (Exception e) {
                LogError("!Failed to parse deals.", e);
                return;
            }
        }

        //return if no deals were in the collection
        if (rootDeal.deals.Count <= 0) {
            LogError("!No deals found.", null);
            return;
        }

        try {
            //get the most recent lastDeal saved to the database for comparison
            using (PushModelContainer context = new PushModelContainer())
            {
                lastDeal = (from ld in context.LastDeals
                            orderby ld.id descending
                            select ld).FirstOrDefault();
            }
        }
        catch (Exception e) {
            LogError("!Could not fetch last deal.", e);
            return;
        }

        //compare the last deal with the current deal
        //considering the headline to be unique key for deals, not ideal obviously
        //but I am limited in what I can get back from scraping the passwird site
        if (currentDeal.headline == @"Currently experiencing technical difficulties.") {
            isNewDeal = false;
        } else if (currentDeal.headline.Truncate(25) != lastDeal.headline.Truncate(25)) {
            isNewDeal = true;
        }

        if (isNewDeal) {
            //found a new deal, send a notification
            Logger.Info(@" >Found new deal.");
            Logger.Info(@" >" + currentDeal.headline);

            try {
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

                shouldSendNotification = true;
                if (lastDeal.headline == "root") shouldSendNotification = true;
            }
            catch (Exception e) {
                LogError("!Failed to save last deal.", e);
                return;
            }
        }

        //if a new deal was found, start sending notifications
        if (shouldSendNotification) {
            SendNotifications(currentDeal.headline);
        }
    }

    /// <summary>
    /// Initialize a connection the the Apple Push Notification Service (APNS) and deliver a new deal notification
    /// payload to all registered devices in the database.
    /// </summary>
    /// <param name="currentHeadline">The headline to be sent in the push notification payload</param>
    private void SendNotifications(string currentHeadline) {
        //configure and start Apple APNS
        PushNotification push;
        try {
            push = new PushNotification(false, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, this.key), ConfigurationManager.AppSettings["certPassword"]);
        }
        catch (Exception e) {
            LogError("!Failed to start service.", e);
            return;
        }

        IList<DeviceToken> deviceTokens = null;
        try {
            //get a list of all registered device tokens
            using (PushModelContainer context = new PushModelContainer())
            {
                var q = (from dt in context.DeviceTokens
                         where dt.Development.Equals(this.isDevelopment)
                         where dt.AppName.Equals(this.appName)
                         where dt.Active.Equals(true)
                         select dt);
                deviceTokens = q.ToList<DeviceToken>();

                foreach (DeviceToken deviceToken in deviceTokens) {
                    if (deviceToken.BadgeCount > 50) {
                        deviceToken.BadgeCount = 0;
                    } else {
                        deviceToken.BadgeCount++;
                    }
                }

                context.SaveChanges();
            }
        }
        catch (Exception e) {
            LogError("!Failed to fetch device tokens.", e);
            return;
        }

        //hold a list of payloads to be pushed
        var payloads = new List<NotificationPayload> {};

        //loop for every registered device token
        foreach (DeviceToken deviceToken in deviceTokens) {
            //add the payload to the list
            var payload = new NotificationPayload(deviceToken.Token, ModifyHeadline(currentHeadline), deviceToken.BadgeCount, "default");
            payloads.Add(payload);
        }

        //send the payloads and get a list of rejected payloads for deletion
        Logger.Info(@" >Delivering payloads...");
        var rejectedTokens = push.SendToApple(payloads);
        if (rejectedTokens.Count > 0) {
            //deactivate all rejected device tokens
            using (PushModelContainer context = new PushModelContainer())
            {
                foreach (var rejectedToken in rejectedTokens) {
                    Logger.Warn(@"Deleting token: " + rejectedToken);
                    DeviceToken dt = new DeviceToken();
                    dt.Token = rejectedToken;
                    dt.Active = false;

                    context.DeviceTokens.AddObject(dt);
                }
                context.SaveChanges();
            }
        }
        Logger.Info(@" >All payloads delivered.");
        Logger.Info(@"");
    }

    #endregion

    #region Class Methods

    /// <summary>
    /// Replaces matching characters or string patterns from a deal headline that shouldn't be sent 
    /// in the notification payload. Also truncates the headline to 200 characters.
    /// </summary>
    /// <param name="headline">The deal headline to be truncated to have characters replaced</param>
    /// <returns>The deal headline with matching characters removed and truncated if necessary</returns>
    private static string ModifyHeadline(string headline) {
        headline = headline.Replace(@"""", @"");
        headline = headline.Replace(@"&amp;", @"&");
        return headline.Truncate(200);
    }

    /// <summary>
    /// Log an error message to the console and current log file.
    /// </summary>
    /// <param name="message">The message to be logged</param>
    /// <param name="e">The Exception thrown</param>
    private static void LogError(string message, Exception e) {
        Console.WriteLine(message);
        Logger.Error(message);
        if (e != null) {
            Logger.Error(e.Message);
        }
    }

    #endregion

    #region Startup

    /// <summary>
    /// Main startup function that spawns a new thread and sleeps every minute.
    /// </summary>
    static void Main() {
        PasswirdPoller pp = new PasswirdPoller();

        //setup app name
        pp.appName = ConfigurationManager.AppSettings[@"appName"];

        //setup key values
        pp.isDevelopment = Convert.ToBoolean(ConfigurationManager.AppSettings[@"isDevelopament"]);
        pp.devKey = ConfigurationManager.AppSettings[@"devKeyFile"];
        pp.key = ConfigurationManager.AppSettings[@"keyFile"];
        if (pp.isDevelopment) { pp.key = pp.devKey; }

        //set api endpoint
        pp.apiUrl = ConfigurationManager.AppSettings[@"apiUrl"];

        //polling amount in minutes
        pp.pollingMinutes = Convert.ToInt32(ConfigurationManager.AppSettings[@"pollingMinutes"]);

        while (true) {
            //spawn a new thread every minute that checks for a new deal and sends all necessary payloads
            new Thread(pp.PollDeals).Start();
            Thread.Sleep((pp.pollingMinutes * 60) * 1000);
        }
    }

    #endregion

}

#region Utilities

/// <summary>
/// Provides extensions to the string class.
/// </summary>
public static class StringExt
{
    /// <summary>
    /// Truncates a string at a maximum length character count.
    /// </summary>
    /// <param name="value">The string to truncase</param>
    /// <param name="maxLength">The length the string should be truncated to</param>
    /// <returns>The truncated string</returns>
    public static string Truncate(this string value, int maxLength) {
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}

#endregion