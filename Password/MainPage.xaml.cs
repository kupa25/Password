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

    using Newtonsoft.Json;

    using PasswordManager.Domain;

    using Windows.Storage;
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
        }

        /// <summary>
        /// Show Add password modal.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
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
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
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
    }
}
