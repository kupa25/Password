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
using Windows.UI.Xaml.Input;
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
            this.PasswordModal.Visibility = Visibility.Collapsed;
            
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
        /// Show Password modal.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The e. </param>
        private void ShowPasswordModal(object sender, RoutedEventArgs e)
        {
            ShowPasswordModal(null);
        }

        private void ShowPasswordModal(Password selectedPassword)
        {
            if (selectedPassword != null)
            {
                //Display the password
                TitleBlock.Text = "Title: " + selectedPassword.Title;
                UserNameBlock.Text = "UserName: " + selectedPassword.UserName;
                PasswordBlock.Text = "Password: " + selectedPassword.PasswordText;

                //TitleTextBox.Text = selectedPassword.Title;
                //UserNameTextBox.Text = selectedPassword.UserName;
                //PasswordTextBox.Password = selectedPassword.PasswordText;

                TitleBlock.Visibility = UserNameBlock.Visibility = PasswordBlock.Visibility = Visibility.Visible;
                TitleTextBox.Visibility = UserNameTextBox.Visibility = PasswordTextBox.Visibility = Visibility.Collapsed;
                //TitleTextBox.IsEnabled = UserNameTextBox.IsEnabled = PasswordTextBox.IsEnabled = false;

            }
            else
            {
                TitleBlock.Visibility = UserNameBlock.Visibility = PasswordBlock.Visibility = Visibility.Collapsed;
                TitleTextBox.Visibility = UserNameTextBox.Visibility = PasswordTextBox.Visibility = Visibility.Visible;
                this.TitleTextBox.Focus(FocusState.Keyboard);
            }

            this.PasswordModal.Visibility = Visibility.Visible;

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
            if (string.IsNullOrWhiteSpace(UserNameTextBox.Text))
            {
                TitleBorder.BorderThickness = new Thickness(4);
                return;
            }

            TitleBorder.BorderThickness = new Thickness(0);

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
                this.PasswordModal.Visibility = Visibility.Collapsed;

                this.RefreshScreen();
            }
        }

        /// <summary>
        /// The password view_ tapped.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The e. </param>
        private void PasswordViewTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(Grid))
            {
                this.PasswordModal.Visibility = Visibility.Collapsed;
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

        private void PasswordTextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if(e.Key == VirtualKey.Enter)
            {
                this.AddPasswordClick(sender, null);
            }
        }

        private void PasswordTapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedItem = (Password)PasswordView.SelectedItem;

            ShowPasswordModal(selectedItem);
        }
    }
}
