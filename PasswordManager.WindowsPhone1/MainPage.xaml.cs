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
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641
using PasswordManager.Helper.Domain;
using PasswordManager.Helper.Utility;

namespace PasswordManager
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.RefreshScreen();
            //var passwordlist = new List<Password>();
            //passwordlist.Add(new Password(){Key = "Hello", PasswordText = " World ", Title = "Hello"});

            //this.itemsViewSource.Source = passwordlist;
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
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void ClearPassword(object sender, RoutedEventArgs e)
        {
            //TODO: code for clearing
        }

        private void AddPassword(object sender, RoutedEventArgs e)
        {
            ShowPasswordModal(null);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordModal.Visibility = Visibility.Collapsed;
        }

        private void SavePassword_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            //{
            //    TitleBorder.BorderThickness = new Thickness(4);
            //    return;
            //}

            //TitleBorder.BorderThickness = new Thickness(0);

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
                //TODO:Error Handling
                //MessageDialog messagebox = new MessageDialog(addPasswordResults.Message, "Cannot add the Password");
                //messagebox.ShowAsync();
                //Debug.WriteLine(addPasswordResults.Message);
            }
            else
            {
                this.TitleTextBox.Text = this.UserNameTextBox.Text = this.PasswordBox.Password = string.Empty;
                this.PasswordModal.Visibility = Visibility.Collapsed;

                this.RefreshScreen();
            }
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowPasswordModal((Password)PasswordView.SelectedItem);
        }

        private void ShowPasswordModal(Password selectedPassword)
        {
            if (selectedPassword != null)
            {
                //Display the password

                TitleTextBox.Text = selectedPassword.Title;
                UserNameTextBox.Text = selectedPassword.UserName;
                PasswordTextBox.Text = selectedPassword.PasswordText;

                PasswordTextBox.IsEnabled = TitleTextBox.IsEnabled = UserNameTextBox.IsEnabled = false;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Collapsed;
                

                //DeleteButton.Visibility = CancelButton.Visibility = 
                    SaveButton.Visibility = Visibility.Collapsed;
//                EditButton.Visibility = Visibility.Visible;

                Storage.tempPassword = selectedPassword;
            }
            else
            {
                TitleTextBox.Text = UserNameTextBox.Text = PasswordTextBox.Text = string.Empty;

                TitleTextBox.IsEnabled = UserNameTextBox.IsEnabled = true;
                //DeleteButton.Visibility = EditButton.Visibility = 
                PasswordTextBox.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = PasswordBox.Visibility = Visibility.Visible;

                this.TitleTextBox.Focus(FocusState.Keyboard);
            }

            this.PasswordModal.Visibility = Visibility.Visible;

            //var bottomAppBar = this.BottomAppBar;
            //if (bottomAppBar != null) bottomAppBar.IsOpen = false;
        }
    }
}
