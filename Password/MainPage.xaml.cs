using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Password
{
    using PasswordManager.Domain;
    using Windows.Storage;
    using Windows.UI.Popups;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

        public MainPage()
        {
            this.InitializeComponent();
            this.AddPassword.Visibility = Visibility.Collapsed;
            RetrievePassword();
        }

        private void AddPasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            this.AddPassword.Visibility = this.AddPassword.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            this.TitleTextBox.Focus(FocusState.Keyboard);
        }

        private void RetrievePassword()
        {
            List<Password> passwordList = new List<Password>();

            foreach (KeyValuePair<string, object> pair in this.settings.Values)
            {
                passwordList.Add(new Password { Title = pair.Key.ToString(), UserName = pair.Value.ToString() });
            }

            this.itemsViewSource.Source = passwordList;
            
        }

        private void AddPassword_Click(object sender, RoutedEventArgs e)
        {
            this.settings.Values.Add(this.TitleTextBox.Text, this.UserNameTextBox.Text + this.PasswordTextBox.Password);

            this.TitleTextBox.Text = this.UserNameTextBox.Text = this.PasswordTextBox.Password = string.Empty;
            this.AddPassword.Visibility = Visibility.Collapsed;

            this.RetrievePassword();
        }

        private void PasswordView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageDialog test = new MessageDialog("hello");
            test.ShowAsync();
        }
    }
}
