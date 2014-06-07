using System.Diagnostics;
using Windows.ApplicationModel.Background;
using Windows.Networking.Connectivity;
using Windows.Storage;

namespace PasswordManager.Background
{
    public sealed class BackgroundTask : IBackgroundTask
    {
        private static ApplicationDataContainer cloudStorage = ApplicationData.Current.RoamingSettings;
        private static ApplicationDataContainer localStorage = ApplicationData.Current.LocalSettings;

        public static bool IsInternetAvailable
        {
            get
            {
                ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
                return connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            }
        }

        public static void Sync()
        {
            Debug.WriteLine("Trying to Synchronize");

            if (IsInternetAvailable)
            {
                // (Version Check)

                int cloudVersion = cloudStorage.Values.ContainsKey("Version") ? (int)cloudStorage.Values["Version"] : 0;
                int localVersion = localStorage.Values.ContainsKey("Version") ? (int)localStorage.Values["Version"] : 0;

                if (cloudVersion > localVersion)
                {
                    // restore down from cloud

                    localStorage.Values.Clear();

                    foreach (var value in cloudStorage.Values)
                    {
                        localStorage.Values.Add(value);
                    }
                }
                else
                {
                    // Sync up
                    cloudStorage.Values.Clear();

                    foreach (var value in cloudStorage.Values)
                    {
                        cloudStorage.Values.Add(value);
                    }
                }
            }
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Running Background Task");
            Sync();
        }
    }
}
