using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;

namespace PasswordManager.Helper.Domain
{
    public class RatingNotifier
    {
        const string NoButton = "None";
        const string IsRatedKey = "IsRated";
        const string RatingRetryKey = "RatingRetry";
        const string RatingCounterKey = "RatingCounter";

        public async static Task TriggerNotificationAsyncTask(string title, string message, string yes, string no, string later, int interval, int maxRetry)
        {
            Debug.WriteLine("Entring Ratings");

            var counter = 0;
            var rated = false;
            var retryCount = 0;

            try
            {
                var settingsContainer = ApplicationData.Current.RoamingSettings;

                if (settingsContainer.Values.ContainsKey(IsRatedKey))
                    rated = Convert.ToBoolean(settingsContainer.Values[IsRatedKey]);
                if (settingsContainer.Values.ContainsKey(RatingRetryKey))
                    retryCount = Convert.ToInt32(settingsContainer.Values[RatingRetryKey]);
                if (settingsContainer.Values.ContainsKey(RatingCounterKey))
                    counter = Convert.ToInt32(settingsContainer.Values[RatingCounterKey]);

                // Increment the usage counter
                counter = counter + 1;

                // Store the current values in roaming app storage
                SaveSettings(rated, counter, retryCount);
            }
            catch
            {
                //TODO: Manage errors here
            }


            // Create a dialog window
            MessageDialog md = new MessageDialog(message, title);

            if (!rated && // app was not rated
                retryCount < maxRetry && // not yet exceeded the max number reminders (e.g. max 3 times)
                counter >= interval*(retryCount + 1))
                // surpassed the usage threshold for asking the user (e.g. every 15 times)
            {

                // User wants to rate the app
                md.Commands.Add(new UICommand(yes, async (s) =>
                {
                    // Store the current values in roaming app storage
                    SaveSettings(true, 0, 0);

                    // Launch the app's review page in the Windows Store using protocol link
                    await Launcher.LaunchUriAsync(new Uri(
                        String.Format("ms-windows-store:REVIEW?PFN={0}",
                            Windows.ApplicationModel.Package.Current.Id.FamilyName)));
                }));


                // User refuses to rate the app now but maybe later
                if (!string.IsNullOrEmpty(later) && later != NoButton)
                {
                    md.Commands.Add(new UICommand(later,
                        (s) => { SaveSettings(retryCount + 1 >= maxRetry, 0, retryCount + 1); }));
                }

                // User does not want to rate the app at all
                if (!string.IsNullOrEmpty(no) && no != NoButton)
                {
                    md.Commands.Add(new UICommand(no, (s) => { SaveSettings(true, 0, retryCount + 1); }));
                }

                // Prompt the user
                if (yes != NoButton || no != NoButton || later != NoButton)
                {
                    await md.ShowAsync();
                }
            }
        }

        private static void SaveSettings(bool rated, int counter, int retry)
        {
            //
            // update
            //
            ApplicationData.Current.RoamingSettings.Values[IsRatedKey] = Convert.ToString(rated);
            ApplicationData.Current.RoamingSettings.Values[RatingRetryKey] = Convert.ToString(retry);
            ApplicationData.Current.RoamingSettings.Values[RatingCounterKey] = Convert.ToString(counter);
        }
    }
}
