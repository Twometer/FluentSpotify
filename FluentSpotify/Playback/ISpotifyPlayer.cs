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

        /// <summary>
        /// Raised when the current track or player state is changed
        /// </summary>
        event EventHandler PlaybackStateChanged;

        /// <summary>
        /// Raised when the position in the track changed
        /// </summary>
        event EventHandler TrackPositionChanged;
    
        /// <summary>
        /// Called when the player is (re)activated. Do initialization here.
        /// </summary>
        Task Initialize();

        /// <summary>
        /// Called every 1000ms while the player is active. Any regular updates e.g. network fetching can be done here
        /// </summary>
        Task Update();

        /// <summary>
        /// Called to pause playback
        /// </summary>
        Task Pause();

        /// <summary>
        /// Called to resume playback
        /// </summary>
        Task Play();

        /// <summary>
        /// Called to start playing a specific track
        /// </summary>
        Task PlayTrack(Track track);

        /// <summary>
        /// Called to start playing a specific track in a playlist.
        /// </summary>
        Task PlayTrack(Playlist context, int trackIdx);

        /// <summary>
        /// Called to start playing a a playlist.
        /// </summary>
        Task PlayPlaylist(Playlist playlist);

        /// <summary>
        /// Called to seek to a specific position in the track
        /// </summary>
        Task Seek(int positionMs);

        /// <summary>
        /// Called to update whether the player should shuffle play the current context, if it exists
        /// </summary>
        Task SetShuffle(bool shuffle);

        /// <summary>
        /// Called to update whether the player should repeat the current track, context, oor not at all.
        /// </summary>
        Task SetRepeat(RepeatMode mode);

        /// <summary>
        /// Called to switch to the previous track
        /// </summary>
        Task Previous();

        /// <summary>
        /// Called to switch to the next track
        /// </summary>
        Task Next();

        /// <summary>
        /// Called to play/pause depending on the current state.
        /// </summary>
        Task TogglePlayback();

        /// <summary>
        /// Called to set the volume of the current player
        /// </summary>
        Task SetVolume(double vol);
    }
}
