using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using CodenamesClient.Properties.Langs;

namespace CodenamesClient.GameUI
{
    public class AudioManager
    {
        private static AudioManager _instance;
        public static AudioManager Instance => _instance ?? (_instance = new AudioManager());

        private Dictionary<string, MediaPlayer> _tracks = new Dictionary<string, MediaPlayer>();

        private DispatcherTimer _fadeTimer;
        private string _currentTrackKey;
        private string _targetTrackKey;

        public double MasterVolume { get; private set; } = 0.5;
        public double MusicVolume { get; private set; } = 0.5;
        public double SfxVolume { get; private set; } = 0.5;
        private const double FadeSpeed = 0.05;

        private MediaPlayer _sfxPlayer = new MediaPlayer();

        private AudioManager()
        {
            _fadeTimer = new DispatcherTimer();
            _fadeTimer.Interval = TimeSpan.FromMilliseconds(50);
            _fadeTimer.Tick += FadeTimer_Tick;

            _sfxPlayer.MediaEnded += (s, e) => _sfxPlayer.Close();
        }

        /// <summary>
        /// Configures all music layers in the game.
        /// </summary>
        /// <param name="trackConfig">Dictionary with “KeyName” and “FilePath”</param>
        public void LoadTracks(Dictionary<string, string> trackConfig)
        {
            foreach (var player in _tracks.Values) player.Close();
            _tracks.Clear();

            try
            {
                foreach (var entry in trackConfig)
                {
                    var player = new MediaPlayer();
                    player.Open(new Uri(entry.Value, UriKind.RelativeOrAbsolute));

                    player.MediaEnded += (s, e) => {
                        player.Position = TimeSpan.Zero;
                        player.Play();
                    };

                    player.Volume = 0;

                    _tracks.Add(entry.Key, player);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.errorPlayingAudio} {ex.Message}");
            }
        }

        /// <summary>
        /// Starts simultaneous playback of all tracks, but only the ‘startKey’ is heard.
        /// </summary>
        public void StartPlayback(string startKey)
        {
            if (!_tracks.ContainsKey(startKey)) return;

            _currentTrackKey = startKey;
            _targetTrackKey = null;

            foreach (var item in _tracks)
            {
                var key = item.Key;
                var player = item.Value;

                player.Volume = (key == startKey) ? (MusicVolume * MasterVolume) : 0;

                player.Position = TimeSpan.Zero;
                player.Play();
            }
        }

        /// <summary>
        /// Performs a smooth transition (crossfade) to a new track.
        /// </summary>
        public void TransitionTo(string nextTrackKey)
        {
            if (!_tracks.ContainsKey(nextTrackKey)) return;
            if (_currentTrackKey == nextTrackKey) return;

            _targetTrackKey = nextTrackKey;
            _fadeTimer.Start();
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            bool transitionFinished = true;
            double targetMaxVol = MusicVolume * MasterVolume;

            foreach (var item in _tracks)
            {
                string key = item.Key;
                MediaPlayer player = item.Value;

                if (key == _targetTrackKey)
                {
                    if (player.Volume < targetMaxVol)
                    {
                        player.Volume += FadeSpeed;
                        transitionFinished = false;
                    }
                }
                else
                {
                    if (player.Volume > 0)
                    {
                        player.Volume -= FadeSpeed;
                        transitionFinished = false;
                    }
                }
            }
            if (transitionFinished)
            {
                _fadeTimer.Stop();
                _currentTrackKey = _targetTrackKey;
                _targetTrackKey = null;
            }
        }

        public void SetMasterVolume(double volume)
        {
            MasterVolume = volume;
            UpdateAllVolumes();
            _sfxPlayer.Volume = SfxVolume * MasterVolume;
        }

        public void SetMusicVolume(double volume)
        {
            MusicVolume = volume;
            UpdateAllVolumes();
        }

        public void SetSfxVolume(double volume)
        {
            SfxVolume = volume;
            _sfxPlayer.Volume = SfxVolume * MasterVolume;
        }

        private void UpdateAllVolumes()
        {
            if (!string.IsNullOrEmpty(_currentTrackKey) && _tracks.ContainsKey(_currentTrackKey))
            {
                _tracks[_currentTrackKey].Volume = MusicVolume * MasterVolume;
            }
        }

        public void PlaySoundEffect(string filename)
        {
            try
            {
                _sfxPlayer.Open(new Uri(filename, UriKind.RelativeOrAbsolute));
                _sfxPlayer.Volume = SfxVolume * MasterVolume;
                _sfxPlayer.Play();
            }
            catch (FileNotFoundException fileEx)
            {
                MessageBox.Show(fileEx.Message, Lang.errorAudioFileNotFound, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Lang.errorPlayingAudio} {ex.Message}", Lang.errorAudioError, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void StopAllAudio()
        {
            _fadeTimer.Stop();

            _currentTrackKey = null;
            _targetTrackKey = null;

            foreach (var player in _tracks.Values)
            {
                player.Stop();
            }
        }
    }
}