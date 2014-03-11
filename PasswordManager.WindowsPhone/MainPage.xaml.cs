using System.Threading.Tasks;
using Microsoft.Phone.Controls;
using Microsoft.WindowsAzure.MobileServices;
using PasswordManager.Library;


namespace PasswordManager.WindowsPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        private IMobileServiceTable<AppUser> AppUserTable = App.MobileService.GetTable<AppUser>(); 

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            //IList<Password> tempList = new List<Password>();

            //tempList.Add(new Password{ Title = "Kshitij", UserName = "Kupa", PasswordText = "test"});

            LoadFromMobileService();

            //ListSelector.ItemsSource = (IList) tempList;

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private async Task LoadFromMobileService()
        {
            var phoneUser = new AppUser
            {
                DeviceId = "test2",
                Id = "test2",
                SerializedPassword = "test2"
            };

            await AppUserTable.InsertAsync(phoneUser);

            return;
        }
    }
}