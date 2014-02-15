// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainPage.xaml.cs" company="Kshitij Upadhyay">
//   Working copy by Kshitij Upadhyay
// </copyright>
// <summary>
//   An empty page that can be used on its own or navigated to within a Frame.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Windows.ApplicationModel.Background;
using Windows.UI.Core;
using PasswordManager.Helper.Domain;
using PasswordManager.Helper.Utility;

namespace PasswordManager
{
    using System;
    using System.Diagnostics;
    using Windows.ApplicationModel;
    using Windows.System;
    using Windows.UI.ApplicationSettings;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

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
            
            var taskRegistered = false;
            var exampleTaskName = "BackgroundTask";

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == exampleTaskName)
                {
                    taskRegistered = true;

                    task.Value.Completed += OnBackGroundTaskCompleted;
                    break;
                }
            }

            BackgroundTaskRegistration myTask = null;

            if (!taskRegistered)
            {
                var builder = new BackgroundTaskBuilder();

                builder.Name = exampleTaskName;
                builder.TaskEntryPoint = "PasswordManager.Background.BackgroundTask";
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.InternetAvailable, false));

                myTask = builder.Register();
                myTask.Completed += OnBackGroundTaskCompleted;
            }

            //Convert old version 1 storage
            Storage.Convert();
            Storage.sync();
            this.RefreshScreen();
            CallRatingNotifier();
        }

        #region Privacy Policy

        private void CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Add(new SettingsCommand("privacypolicy", "Privacy policy", GetPrivacyPolicyAsync));
        }

        private async void GetPrivacyPolicyAsync(IUICommand command)
        {
            await Launcher.LaunchUriAsync(new Uri("http://kshitijwebspace.azurewebsites.net/Help"));
        }

        #endregion

        #region Resume and Suspending

        async void Current_Resuming(object sender, object e)
        {
            Storage.sync();
            this.RefreshScreen();
            CallRatingNotifier();
        }

        private void CurrentOnSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        {
            // TODO : This is the time to save app data in case the process is terminated.
        }

        #endregion

        private static void CallRatingNotifier()
        {
            RatingNotifier.TriggerNotificationAsyncTask("Please Rate", "Would love to get rating from you",
                "Rate the App", "No, Thanks", "May be later", 5, 15);
        }

        private void OnBackGroundTaskCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("refreshing the screen");
            Dispatcher.RunAsync(CoreDispatcherPriority.High, RefreshScreen);
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

            var bottomAppBar = this.BottomAppBar;
            if (bottomAppBar != null) bottomAppBar.IsOpen = false;
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
                UserName = this.UserNameTextBox.Text,
                Title = this.TitleTextBox.Text,
                Key = this.TitleTextBox.Text,
                PasswordText = this.PasswordTextBox.Password
            };

            Results addPasswordResults = Storage.AddPassword(passwordToBeSaved);

            if (addPasswordResults.ResultsType == ResultsType.Error)
            {
                MessageDialog messagebox = new MessageDialog(addPasswordResults.Message, "Cannot add the Password");
                messagebox.ShowAsync();
                Debug.WriteLine(addPasswordResults.Message);
            }
            else
            {
                this.TitleTextBox.Text = this.UserNameTextBox.Text = this.PasswordTextBox.Password = string.Empty;
                this.AddPassword.Visibility = Visibility.Collapsed;

                this.RefreshScreen();
            }
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
                string message = "UserName: " + selectedItem.UserName + "\n" + "Password: " + selectedItem.PasswordText;

                var msg = new MessageDialog(message ?? "Please delete this and recreate");

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

        private async void ClearPasswordClick(object sender, RoutedEventArgs e)
        {
            MessageDialog confirm = new MessageDialog("This will remove your entire password.  Are you sure?");
            confirm.Commands.Add(new UICommand("OK", this.ClearPassword));
            confirm.Commands.Add(new UICommand("Cancel"));
            confirm.DefaultCommandIndex = 0;
            confirm.CancelCommandIndex = 1;

            await confirm.ShowAsync();
        }

        private void ClearPassword(IUICommand command)
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
