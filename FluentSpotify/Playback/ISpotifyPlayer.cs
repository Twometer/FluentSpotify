using FluentSpotify.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.Playback
{
    public interface ISpotifyPlayer
    {
        bool IsPlaying { get; }

        Track CurrentTrack { get; }

        int Position { get; }

        Task Initialize();

        Task Pause();

        Task Play();

        Task PlayTrack(Track track);

        Task PlayTrack(Playlist context, int trackIdx);

        Task PlayPlaylist(Playlist playlist);

        Task Seek(int positionMs);

        Task SetShuffle(bool shuffle);

        Task SetRepeat(RepeatMode mode);

        Task Previous();

        Task Next();

        Task TogglePlayback();

        Task SetVolume(double vol);
    }
}
