using FluentSpotify.API;
using FluentSpotify.Model;
using FluentSpotify.Playback;
using FluentSpotify.UI;
using FluentSpotify.Util;
using FluentSpotify.Web;
using Newtonsoft.Json.Linq;
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

        private SpotifyPlayer player;

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

            PlaybackContainer.Navigate(new Uri("ms-appx-web:///Assets/DrmContainer.html", UriKind.Absolute));
            player = new SpotifyPlayer(PlaybackContainer);

            Spotify.Playback = player;

            Spotify.Playback.PlaybackStateChanged += Playback_PlaybackStateChanged;
            Spotify.Playback.TrackPositionChanged += Playback_TrackPositionChanged;
        }

        private void Playback_TrackPositionChanged(object sender, EventArgs e)
        {
            if (Spotify.Playback.CurrentTrack == null)
                return;

            var pos = Spotify.Playback.Position;
            var percentage = Spotify.Playback.Position / Spotify.Playback.CurrentTrack.Duration.TotalMilliseconds * 100;
            TimeSlider.Value = percentage;
            ElapsedTimeLabel.Text = TimeSpan.FromMilliseconds(pos).ToString(@"m\:ss");
        }

        private void Playback_PlaybackStateChanged(object sender, EventArgs e)
        {
            var pb = Spotify.Playback;

            if (pb.IsPlaying)
            {
                SongLabel.Text = pb.CurrentTrack.Name;
                ArtistLabel.Text = pb.CurrentTrack.ArtistsString;
                TotalTimeLabel.Text = pb.CurrentTrack.DurationString;

                var image = pb.CurrentTrack.Images.FindByResolution(300);
                ThumbnailImage.Source = new BitmapImage() { UriSource = new Uri(image.Url, UriKind.Absolute), DecodePixelWidth = (int)Math.Floor(ThumbnailImage.Width), DecodePixelHeight = (int)Math.Floor(ThumbnailImage.Height) };
            }
            PlaybackFontIcon.Glyph = pb.IsPlaying ? ((char)59241).ToString() :  ((char)59240).ToString();
            
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

        private void PlaybackContainer_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            player.Initialize();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            player.Previous();
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            player.TogglePlayback();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            player.Next();
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            player?.SetVolume(e.NewValue / 100.0);
        }

        private void PlaybackContainer_ScriptNotify(object sender, NotifyEventArgs e)
        {
            var val = JObject.Parse(e.Value);
            player.HandleEvent(val);
        }
    }
}
