using FluentSpotify.API;
using FluentSpotify.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static FluentSpotify.Util.AppSettings;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentSpotify.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {


        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var account = await Spotify.Account.GetAccount();
            AccountName.Text = account.DisplayName;
            AccountPicture.ProfilePicture = new BitmapImage() { UriSource = new Uri(account.ImageUrl, UriKind.Absolute), DecodePixelWidth = (int)Math.Floor(AccountPicture.Width), DecodePixelHeight = (int)Math.Floor(AccountPicture.Height) }; ; ;

            switch (AppTheme)
            {
                case Theme.Light:
                    LightThemeRadioButton.IsChecked = true;
                    break;
                case Theme.Dark:
                    DarkThemeRadioButton.IsChecked = true;
                    break;
                case Theme.Windows:
                    WindowsThemeRadioButton.IsChecked = true;
                    break;
            }
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog()
            {
                Title = "Log out?",
                Content = "Do you want to log out of Fluent Spotify?",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                RequestedTheme = RequestedTheme
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Spotify.Auth.Logout();
            }
        }

        private void LightThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            MainPage.Current.RequestedTheme = ElementTheme.Light;
            AppTheme = Theme.Light;
        }

        private void DarkThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            MainPage.Current.RequestedTheme = ElementTheme.Dark;
            AppTheme = Theme.Dark;
        }

        private void WindowsThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            MainPage.Current.RequestedTheme = MainPage.Current.WindowsDefaultTheme;
            AppTheme = Theme.Windows;
        }
    }
}
