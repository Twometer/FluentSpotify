﻿using FluentSpotify.API;
using FluentSpotify.Model;
using FluentSpotify.UI;
using FluentSpotify.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

using MS = Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FluentSpotify
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private IDictionary<string, Playlist> loadedPlaylists = new Dictionary<string, Playlist>();

        private string lastNav;

        public MainPage()
        {
            this.InitializeComponent();

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
        }

        private void AddPlaylist(Playlist playlist)
        {
            NavView.MenuItems.Add(new MS.NavigationViewItem
            {
                Content = playlist.Name,
                Icon = new SymbolIcon(Symbol.MusicInfo),
                Tag = "list-" + playlist.Id
            });
            loadedPlaylists[playlist.Id] = playlist;
        }

        private void NavView_ItemInvoked(MS.NavigationView sender, MS.NavigationViewItemInvokedEventArgs args)
        {
            var tag = args.InvokedItemContainer.Tag as string;

            if (tag == lastNav)
                return;

            if (tag.StartsWith("list-"))
            {
                var id = tag.Substring("list-".Length);
                var playlist = loadedPlaylists[id];
                ContentFrame.Navigate(typeof(PlaylistPage), playlist, args.RecommendedNavigationTransitionInfo);
            }

            lastNav = tag;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Spotify.Initialize();

            if (!Spotify.Auth.KeyStore.Authenticated)
                await new LoginDialog().ShowAsync();

            var account = await Spotify.Account.GetAccount();
            UserItem.Content = account.DisplayName;
            UserItem.Icon = new BitmapIcon() { UriSource = new Uri(account.ImageUrl, UriKind.Absolute), ShowAsMonochrome = false };

            loadedPlaylists.Clear();
            var playlists = await Spotify.Account.GetPlaylists();
            foreach (var list in playlists)
                AddPlaylist(list);
        }

        private void SwitchThemeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (RequestedTheme == ElementTheme.Light)
                RequestedTheme = ElementTheme.Dark;
            else
                RequestedTheme = ElementTheme.Light;
        }

        private async void UserItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var dialog = new ContentDialog()
            {
                Title = "Log out?",
                Content = "Do you want to log out of Fluent Spotify?",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No"
            };
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Spotify.Auth.Logout();
            }
        }
    }
}
