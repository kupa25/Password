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

        #region Action Handlers
        /// <summary>
        /// The password view_ tapped.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The e. </param>
        private void PasswordViewTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(Grid))
            {
                this.ClosePasswordModal();
            }
        }

        private void PasswordBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                this.AddPasswordClick(sender, null);
            }
        }

        private void PasswordTapped(object sender, TappedRoutedEventArgs e)
        {
            ShowPasswordModal((Password)PasswordView.SelectedItem);
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

        /// <summary>
        /// Takes the users input and save the password
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The e. </param>
        private void AddPasswordClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
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
                PasswordText = this.PasswordBox.Password
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
                this.TitleTextBox.Text = this.UserNameTextBox.Text = this.PasswordBox.Password = string.Empty;
                this.PasswordModal.Visibility = Visibility.Collapsed;

                this.RefreshScreen();
            }
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = ((Password)this.PasswordView.SelectedItem);
            Debug.WriteLine("About to delete :" + selectedItem);

            Storage.DeletePassword(selectedItem);

            this.RefreshScreen();
        }

        private void EditButton_OnClick(object sender, RoutedEventArgs e)
        {
            TitleTextBox.IsEnabled = UserNameTextBox.IsEnabled = true;
            PasswordBox.Password = PasswordTextBox.Text;

            DeleteButton.Visibility = EditButton.Visibility = PasswordTextBox.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = SaveButton.Visibility = PasswordBox.Visibility = Visibility.Visible;

            this.TitleTextBox.Focus(FocusState.Keyboard);
        }


        private void CancelButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            this.ClosePasswordModal();
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

        #endregion

        private void ShowPasswordModal(Password selectedPassword)
        {
            if (selectedPassword != null)
            {
                //Display the password

                TitleTextBox.Text = selectedPassword.Title;
                UserNameTextBox.Text = selectedPassword.UserName;
                PasswordTextBox.Text = selectedPassword.PasswordText;

                TitleTextBox.IsEnabled = UserNameTextBox.IsEnabled = false;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;

                DeleteButton.Visibility = CancelButton.Visibility = SaveButton.Visibility = Visibility.Collapsed;
                EditButton.Visibility = Visibility.Visible;

                Storage.tempPassword = selectedPassword;
            }
            else
            {
                TitleTextBox.Text = UserNameTextBox.Text = PasswordTextBox.Text = string.Empty;

                TitleTextBox.IsEnabled = UserNameTextBox.IsEnabled = true;
                DeleteButton.Visibility = EditButton.Visibility = PasswordTextBox.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = PasswordBox.Visibility = Visibility.Visible;

                this.TitleTextBox.Focus(FocusState.Keyboard);
            }

            this.PasswordModal.Visibility = Visibility.Visible;

            var bottomAppBar = this.BottomAppBar;
            if (bottomAppBar != null) bottomAppBar.IsOpen = false;
        }

        private void ClosePasswordModal()
        {
            this.TitleBorder.BorderThickness = new Thickness(0);
            this.PasswordModal.Visibility = Visibility.Collapsed;
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

        private void ClearPassword(IUICommand command)
        {
            Storage.RemovePasswordList();
            this.RefreshScreen();
        }
    }
}
