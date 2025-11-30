using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using CodenamesClient.Properties.Langs;

namespace CodenamesClient.GameUI
{
    public static class AudioManager
    {
        private static MediaPlayer _musicPlayer = new MediaPlayer();
        private static MediaPlayer _sfxPlayer = new MediaPlayer();

        public static double MasterVolume { get; private set; } = 0.5;
        public static double MusicVolume { get; private set; } = 0.5;
        public static double SfxVolume { get; private set; } = 0.5;

        static AudioManager()
        {
            _sfxPlayer.MediaEnded += (s, e) => {
                _sfxPlayer.Close();
            };

            _sfxPlayer.MediaFailed += (s, e) => {
                MessageBox.Show($"{Lang.errorPlayingAudio} {e.ErrorException.Message}", Lang.errorAudioError, MessageBoxButton.OK, MessageBoxImage.Warning);
            };
        }

        public static void StartMusic(string filename)
        {
            try
            {
                _musicPlayer.Open(new Uri(filename, UriKind.Relative));
                _musicPlayer.MediaEnded += (s, e) =>
                {
                    _musicPlayer.Position = TimeSpan.Zero;
                    _musicPlayer.Play();
                };
                UpdateMusicVolume();
                _musicPlayer.Play();
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

        public static void PlaySoundEffect(string filename)
        {
            try
            {
                _sfxPlayer.Stop();
                _sfxPlayer.Open(new Uri(filename, UriKind.Relative));
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

        public static void SetMasterVolume(double volume)
        {
            MasterVolume = volume;
            UpdateMusicVolume();
        }

        public static void SetMusicVolume(double volume)
        {
            MusicVolume = volume;
            UpdateMusicVolume();
        }

        public static void SetSfxVolume(double volume)
        {
            SfxVolume = volume;
        }

        private static void UpdateMusicVolume()
        {
            _musicPlayer.Volume = MusicVolume * MasterVolume;
        }
    }
}