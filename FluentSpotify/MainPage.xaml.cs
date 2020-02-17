using FluentSpotify.API;
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
        public MainPage()
        {
            this.InitializeComponent();

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            AddPlaylist("Discover Weekly", false);
            AddPlaylist("enbo Club-Essentials", false);
            AddPlaylist("Spotify", true);
            AddPlaylist("Folder 1", true);
            AddPlaylist("Folder 2", true);
        }

        public void AddPlaylist(string name, bool isFolder)
        {
            NavView.MenuItems.Add(new MS.NavigationViewItem
            {
                Content = name,
                Icon = new SymbolIcon(isFolder ? (Symbol)59575 : Symbol.MusicInfo),
                Tag = "list-8127590879354"
            });
        }

        private void NavView_ItemInvoked(MS.NavigationView sender, MS.NavigationViewItemInvokedEventArgs args)
        {

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Spotify.Instance.Initialize();

            if (!Spotify.Instance.KeyStore.Authenticated)
                await new LoginDialog().ShowAsync();
        }
    }
}
