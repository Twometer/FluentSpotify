using FluentSpotify.API;
using FluentSpotify.Model;
using FluentSpotify.Playback;
using FluentSpotify.UI;
using FluentSpotify.Util;
using FluentSpotify.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly ThrottledExecution throttledExecution = new ThrottledExecution(TimeSpan.FromMilliseconds(1000));

        private readonly IDictionary<string, Playlist> loadedPlaylists = new Dictionary<string, Playlist>();

        private SpotifyPlayer player;

        private string lastNav;
        private string lastTrack;

        private bool isMute;
        private bool ignoreNextSeek;

        public MainPage()
        {
            this.InitializeComponent();

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            ContentFrame.Navigate(typeof(HomePage));
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
            if (args.IsSettingsInvoked)
                tag = "settings";

            if (tag == lastNav)
                return;

            if (tag.StartsWith("list-"))
            {
                var id = tag.Substring("list-".Length);
                var playlist = loadedPlaylists[id];
                ContentFrame.Navigate(typeof(PlaylistPage), playlist, args.RecommendedNavigationTransitionInfo);
            }
            else if (tag == "settings")
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else if (tag == "home")
            {
                ContentFrame.Navigate(typeof(HomePage));
            }
            else if (tag == "liked")
            {
                ContentFrame.Navigate(typeof(LibraryPage));
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

            Debug.WriteLine("Initialized");

            TimeSlider.ThumbToolTipValueConverter = new PercentageToTimeConverter();
        }

        private void Playback_TrackPositionChanged(object sender, EventArgs e)
        {
            PlaybackFontIcon.Glyph = Spotify.Playback.IsPlaying ? ((char)59241).ToString() : ((char)59240).ToString();

            if (Spotify.Playback.CurrentTrack == null)
                return;

            var pos = Spotify.Playback.Position;
            var percentage = Spotify.Playback.Position / Spotify.Playback.CurrentTrack.Duration.TotalMilliseconds * 100;

            TimeSlider.Value = percentage;
            ElapsedTimeLabel.Text = TimeSpan.FromMilliseconds(pos).ToString(@"m\:ss");
            ignoreNextSeek = true;
        }

        private void Playback_PlaybackStateChanged(object sender, EventArgs e)
        {
            var pb = Spotify.Playback;

            if (pb.IsPlaying && pb.CurrentTrack?.Id != lastTrack)
            {
                SongLabel.Text = pb.CurrentTrack.Name;
                ArtistLabel.Text = pb.CurrentTrack.ArtistsString;
                TotalTimeLabel.Text = pb.CurrentTrack.DurationString;

                var image = pb.CurrentTrack.Images.FindByResolution(300);
                ThumbnailImage.Source = new BitmapImage() { UriSource = new Uri(image.Url, UriKind.Absolute), DecodePixelWidth = (int)Math.Floor(ThumbnailImage.Width), DecodePixelHeight = (int)Math.Floor(ThumbnailImage.Height) };
                lastTrack = pb.CurrentTrack.Id;
            }

            TimeSlider.IsEnabled = pb.CurrentTrack != null;
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

        private async void PlaybackContainer_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            await player.Initialize();
        }

        private async void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (player.Position > 3000)
                await player.Seek(0);
            else
                await player.Previous();
        }

        private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            await player.TogglePlayback();
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            await player.Next();
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            player?.SetVolume(e.NewValue / 100.0);
            MuteFontIcon.Glyph = ((char)59239).ToString();
            isMute = false;
        }

        private void PlaybackContainer_ScriptNotify(object sender, NotifyEventArgs e)
        {
            var val = JObject.Parse(e.Value);
            player.HandleEvent(val);
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                var track = args.ChosenSuggestion as Track;
                player.PlayTrack(track);
            }
            else
            {
                var query = args.QueryText;
            }
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var query = sender.Text;
                if (query.Length < 3)
                {
                    sender.ItemsSource = null;
                    return;
                }

                throttledExecution.Run(async () =>
                {
                    var results = await Spotify.Search.Search(query, 5);
                    sender.ItemsSource = results;
                });
            }
        }

        private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }

        private async void TimeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ignoreNextSeek)
            {
                ignoreNextSeek = false;
                return;
            }
            var ms = player.CurrentTrack.Duration.TotalMilliseconds * (e.NewValue / 100.0);
            await player.Seek((int)ms);
        }

        private async void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (isMute)
            {
                player?.SetVolume(VolumeSlider.Value / 100.0);
                MuteFontIcon.Glyph = ((char)59239).ToString();
            }
            else
            {
                MuteFontIcon.Glyph = ((char)59215).ToString();
                await player.SetVolume(0);
            }
            isMute = !isMute;
        }

        private async void RepeatButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var b = RepeatButton.IsChecked;
            if (b.HasValue)
            {

                if (b.Value)
                    await player.SetRepeat(RepeatMode.Context);
                else
                    await player.SetRepeat(RepeatMode.Off);

                RepeatFontIcon.Glyph = ((char)59630).ToString();
            }
            else
            {
                await player.SetRepeat(RepeatMode.Track);
                RepeatFontIcon.Glyph = ((char)59629).ToString();
            }
        }

        private async void ShuffleButton_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ShuffleButton.IsChecked.Value)
            {
                await player.SetShuffle(true);
            }
            else
            {
                await player.SetShuffle(false);
            }
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (ContentFrame.Content is IScrollNotify notify)
                notify.OnScroll(e.NextView.VerticalOffset, ScrollViewer.ExtentHeight - ScrollViewer.ViewportHeight);
        }
    }
}
