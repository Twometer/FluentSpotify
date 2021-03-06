﻿using FluentSpotify.API;
using FluentSpotify.CLI;
using FluentSpotify.Model;
using FluentSpotify.Playback;
using FluentSpotify.UI;
using FluentSpotify.UI.Controller;
using FluentSpotify.Util;
using FluentSpotify.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
        public static MainPage Current;

        public ElementTheme WindowsDefaultTheme;

        private readonly ThrottledExecution throttledExecution = new ThrottledExecution(TimeSpan.FromMilliseconds(1000));

        private readonly IDictionary<string, Playlist> loadedPlaylists = new Dictionary<string, Playlist>();

        private DeviceListController deviceListController;

        private string lastNav;
        private string lastTrack;

        private bool isMute;

        public MainPage()
        {
            this.InitializeComponent();

            Current = this;
            WindowsDefaultTheme = RequestedTheme;

            switch (AppSettings.AppTheme)
            {
                case AppSettings.Theme.Light:
                    RequestedTheme = ElementTheme.Light;
                    break;
                case AppSettings.Theme.Dark:
                    RequestedTheme = ElementTheme.Dark;
                    break;
            }

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            deviceListController = new DeviceListController(this, DeviceFlyout);

            ContentFrame.Navigate(typeof(HomePage));
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is CmdOptions options)
            {
                if (string.IsNullOrEmpty(options.PlayerId))
                    return;
                Log.Info($"CMD specifies being attached to a player, transferring playback... playerId={options.PlayerId}");
                await Spotify.Playback.TransferPlayback(options.PlayerId);
            }
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
            UserNameLabel.Text = account.DisplayName;
            UserImage.ProfilePicture = new BitmapImage() { UriSource = new Uri(account.ImageUrl, UriKind.Absolute), DecodePixelWidth = (int)Math.Floor(UserImage.Width), DecodePixelHeight = (int)Math.Floor(UserImage.Height) }; ; ;

            loadedPlaylists.Clear();
            var playlists = await Spotify.Account.GetPlaylists();
            foreach (var list in playlists)
                AddPlaylist(list);

            PlaybackContainer.Navigate(new Uri("ms-appx-web:///Assets/DrmContainer.html", UriKind.Absolute));

            Spotify.Playback.PlaybackStateChanged += Playback_PlaybackStateChanged;
            Spotify.Playback.TrackPositionChanged += Playback_TrackPositionChanged;

            TimeSlider.ThumbToolTipValueConverter = new PercentageToTimeConverter();

            Spotify.Playback.LocalPlayer = new LocalPlayer(PlaybackContainer);

            await deviceListController.ReloadDeviceList();
            if (deviceListController.IsOtherDeviceActive())
            {
                Spotify.Playback.CurrentPlayer = new RemotePlayer(deviceListController.GetCurrentlyActivePlayer());
                await Spotify.Playback.CurrentPlayer.Initialize();
            }
            else
            {
                Spotify.Playback.CurrentPlayer = Spotify.Playback.LocalPlayer;
                // Player will be initialized later
            }



            Log.Info("Data download and init complete");
        }

        private void Playback_TrackPositionChanged(object sender, EventArgs e)
        {
            var player = Spotify.Playback.CurrentPlayer;

            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlaybackFontIcon.Glyph = player.IsPlaying ? ((char)59241).ToString() : ((char)59240).ToString();

                if (player.CurrentTrack == null)
                    return;

                var pos = player.Position;
                var percentage = player.Position / player.CurrentTrack.Duration.TotalMilliseconds * 100;

                TimeSlider.Value = percentage;
                ElapsedTimeLabel.Text = TimeSpan.FromMilliseconds(pos).ToString(@"m\:ss");
            });
        }

        private void Playback_PlaybackStateChanged(object sender, EventArgs e)
        {
            var player = Spotify.Playback.CurrentPlayer;

            _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (player.IsPlaying && player.CurrentTrack?.Id != lastTrack)
                {
                    SongLabel.Text = player.CurrentTrack.Name;
                    ArtistLabel.Text = player.CurrentTrack.ArtistsString;
                    TotalTimeLabel.Text = player.CurrentTrack.DurationString;

                    var image = player.CurrentTrack.Images.FindByResolution(300);
                    ThumbnailImage.Source = new BitmapImage() { UriSource = new Uri(image.Url, UriKind.Absolute), DecodePixelWidth = (int)Math.Floor(ThumbnailImage.Width), DecodePixelHeight = (int)Math.Floor(ThumbnailImage.Height) };
                    lastTrack = player.CurrentTrack.Id;
                }

                TimeSlider.IsEnabled = player.CurrentTrack != null;
            });
        }

        private void SwitchThemeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (RequestedTheme == ElementTheme.Light)
            {
                RequestedTheme = ElementTheme.Dark;
                AppSettings.AppTheme = AppSettings.Theme.Dark;
            }
            else
            {
                RequestedTheme = ElementTheme.Light;
                AppSettings.AppTheme = AppSettings.Theme.Light;
            }
        }

        private async void PlaybackContainer_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            await Spotify.Playback.LocalPlayer.Initialize();
        }

        private async void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            var player = Spotify.Playback.CurrentPlayer;
            if (player.Position > 3000)
                await player.Seek(0);
            else
                await player.Previous();
        }

        private async void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            var player = Spotify.Playback.CurrentPlayer;
            await player.TogglePlayback();
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            var player = Spotify.Playback.CurrentPlayer;
            await player.Next();
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var player = Spotify.Playback.CurrentPlayer;
            player?.SetVolume(e.NewValue / 100.0);
            MuteFontIcon.Glyph = ((char)59239).ToString();
            isMute = false;
        }

        private void PlaybackContainer_ScriptNotify(object sender, NotifyEventArgs e)
        {
            var val = JObject.Parse(e.Value);
            Spotify.Playback.LocalPlayer.HandleEvent(val);
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var player = Spotify.Playback.CurrentPlayer;
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

        private void TimeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            // TODO: Only update if changed by user
            /*var player = Spotify.Playback.CurrentPlayer;
            if (ignoreNextSeek)
            {
                ignoreNextSeek = false;
                return;
            }
            var ms = player.CurrentTrack.Duration.TotalMilliseconds * (e.NewValue / 100.0);
            await player.Seek((int)ms);*/
        }

        private async void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            var player = Spotify.Playback.CurrentPlayer;
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
            var player = Spotify.Playback.CurrentPlayer;
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
            var player = Spotify.Playback.CurrentPlayer;
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

        private void UserPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            UserPanel.Background = Resources["SystemChromeLowColor"] as Brush;
        }

        private void UserPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            UserPanel.Background = new SolidColorBrush(Colors.Transparent);
        }

    }
}
