// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainPage.xaml.cs" company="Kshitij Upadhyay">
//   Working copy by Kshitij Upadhyay
// </copyright>
// <summary>
//   An empty page that can be used on its own or navigated to within a Frame.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PasswordManager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Windows.ApplicationModel;
    using Windows.System;
    using Windows.UI.ApplicationSettings;

    using Newtonsoft.Json;

    using PasswordManager.Domain;

    using Windows.Storage;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using PasswordManager.Utility;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            Application.Current.Suspending += this.CurrentOnSuspending;
            Application.Current.Resuming += this.Current_Resuming;
            SettingsPane.GetForCurrentView().CommandsRequested += this.CommandsRequested;

            this.InitializeComponent();
            this.AddPassword.Visibility = Visibility.Collapsed;
            this.RefreshScreen();

            //System.Threading.Timer = new System.Threading.Timer(this.TimerCall,null,)
        }

        public void TimerCall()
        {
            //Nothing yet
        }

        private void CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Add(new SettingsCommand("privacypolicy", "Privacy policy", GetPrivacyPolicyAsync));
        }

        private async void GetPrivacyPolicyAsync(IUICommand command)
        {
            await Launcher.LaunchUriAsync(new Uri("http://kshitijwebspace.azurewebsites.net/Help"));
        }

        void Current_Resuming(object sender, object e)
        {
            this.RefreshScreen();
        }

        private void CurrentOnSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        {
            // TODO : This is the time to save app data in case the process is terminated.
        }

        /// <summary>
        /// Show Add password modal.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The e. </param>
        private void ShowAddPasswordModal(object sender, RoutedEventArgs e)
        {
            this.AddPassword.Visibility = Visibility.Visible;
            this.TitleTextBox.Focus(FocusState.Keyboard);

            this.BottomAppBar.IsOpen = false;
        }

        /// <summary>
        /// Refresh the Grid View
        /// </summary>
        private void RefreshScreen()
        {
            var passwordList = Storage.RetreivePassword();

            this.itemsViewSource.Source = null;
            this.itemsViewSource.Source = passwordList;
            this.PasswordView.SelectedIndex = -1;
        }

        /// <summary>
        /// Takes the users input and save the password
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The e. </param>
        private void AddPasswordClick(object sender, RoutedEventArgs e)
        {
            var passwordToBeSaved = new Password
            {
                KeyGuid = Guid.NewGuid(),
                UserName = this.UserNameTextBox.Text,
                Title = this.TitleTextBox.Text,
                PasswordText = this.PasswordTextBox.Password
            };

            Storage.AddPassword(passwordToBeSaved);

            this.TitleTextBox.Text = this.UserNameTextBox.Text = this.PasswordTextBox.Password = string.Empty;
            this.AddPassword.Visibility = Visibility.Collapsed;

            this.RefreshScreen();
        }

        /// <summary>
        /// The password view_ tapped.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The e. </param>
        private async void PasswordViewTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var selectedItem = (Password)((GridView)sender).SelectedItem;

            if (selectedItem != null)
            {
                string password = selectedItem.PasswordText;

                var msg = new MessageDialog(password ?? "Please delete this and recreate");

                // Create the button manually so that we can associate default action and cancel button action
                msg.Commands.Add(new UICommand("Delete", this.DeletePassword));
                msg.Commands.Add(new UICommand("Close", null));
                msg.DefaultCommandIndex = msg.CancelCommandIndex = 1;

                await msg.ShowAsync();
            }
            else
            {
                Debug.WriteLine("selected item was null");
            }
        }

        private void DeletePassword(IUICommand command)
        {

            var selectedItem = ((Password)this.PasswordView.SelectedItem);
            Debug.WriteLine("About to delete :" + selectedItem);

            Storage.DeletePassword(selectedItem);

            this.RefreshScreen();
        }

        private async void RemovePasswordList(object sender, RoutedEventArgs e)
        {
            MessageDialog confirm = new MessageDialog("This will remove your entire password.  Are you sure?");
            confirm.Commands.Add(new UICommand("OK", this.RemovePasswordList));
            confirm.Commands.Add(new UICommand("Cancel"));
            confirm.DefaultCommandIndex = 0;
            confirm.CancelCommandIndex = 1;

            await confirm.ShowAsync();
        }

        private void RemovePasswordList(IUICommand command)
        {
            Storage.RemovePasswordList();

            this.RefreshScreen();
        }

        private void MyCustomGridView_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this.AddPassword.Visibility = Visibility.Collapsed;
        }

        private void PasswordTextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if(e.Key == VirtualKey.Enter)
            {
                this.AddPasswordClick(sender, null);
            }
        }
    }
}
