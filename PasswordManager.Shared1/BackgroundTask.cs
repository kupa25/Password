using System.Diagnostics;
using Windows.ApplicationModel.Background;

//using PasswordManager..Utility;

namespace PasswordManager.Background
{
    public sealed class BackgroundTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Running Background Task");
            //Storage.sync();
        }
    }
}
