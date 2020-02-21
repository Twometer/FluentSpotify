using FluentSpotify.Model;
using FluentSpotify.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.API
{
    public class TracksApi
    {
        public PagedRequest<Track> GetLibrary()
        {
            return Request.New("https://api.spotify.com/v1/me/tracks")
                .Authenticate("Bearer", Spotify.AccessToken)
                .GetPaged(obj => Track.Parse(obj));
        }

        public PagedRequest<Track> GetTracks(string playlistId)
        {
            return Request.New($"https://api.spotify.com/v1/playlists/{playlistId}/tracks")
                .Authenticate("Bearer", Spotify.AccessToken)
                .GetPaged(obj => Track.Parse(obj));
        }

    }
}
