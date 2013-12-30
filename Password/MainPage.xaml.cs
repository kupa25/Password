// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainPage.xaml.cs" company="Kshitij Upadhyay">
//   Working copy by Kshitij Upadhyay
// </copyright>
// <summary>
//   An empty page that can be used on its own or navigated to within a Frame.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Password
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Newtonsoft.Json;

    using PasswordManager.Domain;

    using Windows.Storage;
    using Windows.UI.Popups;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// The settings.
        /// </summary>
        private ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

        public MainPage()
        {
            this.InitializeComponent();
            this.AddPassword.Visibility = Visibility.Collapsed;
            this.RefreshScreen();

            this.PasswordView.SelectedIndex = -1;

        }

        /// <summary>
        /// Show Add password modal.
        /// </summary>
        /// <param name="sender"> The sender. </param>
        /// <param name="e"> The e. </param>
        private void ShowAddPasswordModal(object sender, RoutedEventArgs e)
        {
            this.AddPassword.Visibility = this.AddPassword.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            this.TitleTextBox.Focus(FocusState.Keyboard);
        }

        /// <summary>
        /// The retrieve password.
        /// </summary>
        private void RefreshScreen()
        {
            var passwordList = new List<Password>();

            foreach (KeyValuePair<string, object> pair in this.settings.Values)
            {
                Password deserializedObj;
                try
                {
                    deserializedObj = JsonConvert.DeserializeObject<Password>(pair.Value.ToString());
                }
                catch (Exception)
                {
                    deserializedObj = new Password { Title = pair.Key, UserName = pair.Value.ToString() };
                }

                passwordList.Add(deserializedObj);
            }

            this.itemsViewSource.Source = passwordList;
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
                PasswordText = this.PasswordTextBox.Password
            };

            this.settings.Values.Add(this.TitleTextBox.Text, JsonConvert.SerializeObject(passwordToBeSaved));

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
                msg.Commands.Add(new UICommand("Close", null));
                msg.DefaultCommandIndex = msg.CancelCommandIndex = 0;

                await msg.ShowAsync();
            }
            else
            {
                Debug.WriteLine("selected item was null");
            }
        }
    }
}
