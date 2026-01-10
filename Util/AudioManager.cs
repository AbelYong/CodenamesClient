using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using CodenamesClient.Properties.Langs;
using CodenamesGame.Util;

namespace CodenamesClient.GameUI
{
    public class AudioManager
    {
        private static AudioManager _instance;
        public static AudioManager Instance => _instance ?? (_instance = new AudioManager());
        private readonly Dictionary<string, MediaPlayer> _tracks = new Dictionary<string, MediaPlayer>();
        private readonly MediaPlayer _sfxPlayer = new MediaPlayer();
        private readonly DispatcherTimer _fadeTimer;
        private string _currentTrackKey;
        private string _targetTrackKey;
        private const double FadeSpeed = 0.05;

        public double MasterVolume
        {
            get;
            private set; 
        } = 0.5;

        public double MusicVolume
        { 
            get;
            private set; 
        } = 0.5;

        public double SfxVolume
        { 
            get; 
            private set; 
        } = 0.5;

        private AudioManager()
        {
            _fadeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _fadeTimer.Tick += FadeTimer_Tick;

            _sfxPlayer.MediaEnded += (s, e) => _sfxPlayer.Close();
        }

        public void LoadTracks(Dictionary<string, string> trackConfig)
        {
            foreach (var player in _tracks.Values)
            {
                player.Close();
            }
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
            catch (Exception ex) when (ex is ArgumentNullException || ex is ArgumentException) 
            {
                MessageBox.Show(Lang.errorPlayingAudio);
                CodenamesGameLogger.Log.Debug("Exception while loading audio tracks: ", ex);
            }
        }

        public void StartPlayback(string startKey)
        {
            if (!_tracks.ContainsKey(startKey))
            {
                return;
            }

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

        public void TransitionTo(string nextTrackKey)
        {
            if (!_tracks.ContainsKey(nextTrackKey))
            {
                return;
            }
            if (_currentTrackKey == nextTrackKey)
            {
                return;
            }

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
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(Lang.errorPlayingAudio, Lang.errorAudioFileNotFound, MessageBoxButton.OK, MessageBoxImage.Warning);
                CodenamesGameLogger.Log.Debug("Audio file not found: ", ex);
            }
            catch (Exception ex) when (ex is UriFormatException || ex is ArgumentNullException || ex is ArgumentException)
            {
                MessageBox.Show(Lang.errorPlayingAudio, Lang.errorAudioError, MessageBoxButton.OK, MessageBoxImage.Warning);
                CodenamesGameLogger.Log.Debug("Exception while playing audio: ", ex);
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