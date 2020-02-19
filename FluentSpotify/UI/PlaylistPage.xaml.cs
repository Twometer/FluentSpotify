using FluentSpotify.API;
using FluentSpotify.Model;
using FluentSpotify.Util;
using FluentSpotify.Web;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentSpotify.UI
{

    public sealed partial class PlaylistPage : Page
    {
        private Playlist playlist;

        private PagedRequest<Track> request;

        public PlaylistPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var playlist = e.Parameter as Playlist;
            this.playlist = playlist;

            HeaderLabel.Text = playlist.Name;
            DescLabel.Text = playlist.Description;
            DescLabel.Visibility = string.IsNullOrWhiteSpace(playlist.Description) ? Visibility.Collapsed : Visibility.Visible;

            OwnerLabel.Text = $"by {playlist.Owner}";
            MetaLabel.Text = $"{playlist.TrackCount} songs";

            var image = playlist.Images.FindByResolution(300);
            PlaylistImage.Source = new BitmapImage() { UriSource = new Uri(image.Url, UriKind.Absolute), DecodePixelWidth = (int)Math.Floor(PlaylistImage.Width), DecodePixelHeight = (int)Math.Floor(PlaylistImage.Height) };

            request = Spotify.Tracks.GetTracks(playlist.Id);
            LoadMore();
        }

        private async void LoadMore()
        {
            if (request.HasNext)
            {
                var tracks = await request.Next();
                foreach (var track in tracks)
                    TrackList.Items.Add(track);
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Spotify.Playback.PlayPlaylist(playlist);
        }

        private void TrackList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Track)
                Spotify.Playback.PlayTrack(playlist, TrackList.Items.IndexOf(e.ClickedItem));
        }
    }
}
