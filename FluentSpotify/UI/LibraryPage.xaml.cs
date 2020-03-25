using FluentSpotify.API;
using FluentSpotify.Model;
using FluentSpotify.Util;
using FluentSpotify.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentSpotify.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LibraryPage : Page, IScrollNotify
    {
        private PagedRequest<Track> request;

        private PagedLoader<Track> loader;

        public LibraryPage()
        {
            this.InitializeComponent();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TrackList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Track track)
                Spotify.Playback.CurrentPlayer.PlayTrack(track);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            request = Spotify.Tracks.GetLibrary();
            loader = new PagedLoader<Track>(request, TrackList);
            await loader.Begin();
            MetaLabel.Text = request.Total + " songs";
        }

        public void OnScroll(double current, double max)
        {
            loader.OnScroll(current, max);
        }
    }
}
