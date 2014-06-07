using Windows.Networking.Connectivity;

namespace PasswordManager.Helper.Utility
{
    public class Helper
    {
        public static bool IsInternetAvailable
        {
            get
            {
                ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
                return connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            }
        }
    }
}
