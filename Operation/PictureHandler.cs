using CodenamesClient.Properties.Langs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CodenamesClient.Operation
{
    public static class PictureHandler
    {
        public const int NUMBER_OF_AGENT_PICTURES = 15;
        public const int NUMBER_OF_BYSTANDER_PICTURES = 7;
        public const int NUMBER_OF_ASSASSIN_PICTURES = 3;
        private const int _DEFAULT_IMAGE = 0;
        private static readonly List<string> _agentPicturePaths = new List<string>
        {
            "/Assets/BoardUI/Agents/agent01.png",
            "/Assets/BoardUI/Agents/agent02.png",
            "/Assets/BoardUI/Agents/agent03.png",
            "/Assets/BoardUI/Agents/agent04.png",
            "/Assets/BoardUI/Agents/agent05.png",
            "/Assets/BoardUI/Agents/agent06.png",
            "/Assets/BoardUI/Agents/agent07.png",
            "/Assets/BoardUI/Agents/agent08.png",
            "/Assets/BoardUI/Agents/agent09.png",
            "/Assets/BoardUI/Agents/agent10.png",
            "/Assets/BoardUI/Agents/agent11.png",
            "/Assets/BoardUI/Agents/agent12.png",
            "/Assets/BoardUI/Agents/agent13.png",
            "/Assets/BoardUI/Agents/agent14.png",
            "/Assets/BoardUI/Agents/agent15.png",
            "/Assets/BoardUI/Neutrals/neutral01.png",
            "/Assets/BoardUI/Neutrals/neutral02.png",
            "/Assets/BoardUI/Neutrals/neutral03.png",
            "/Assets/BoardUI/Neutrals/neutral04.png",
            "/Assets/BoardUI/Neutrals/neutral05.png",
            "/Assets/BoardUI/Neutrals/neutral06.png",
            "/Assets/BoardUI/Neutrals/neutral07.png",
            "/Assets/BoardUI/Assassins/assassin01.png",
            "/Assets/BoardUI/Assassins/assassin02.png",
            "/Assets/BoardUI/Assassins/assassin03.png"
        };

        public static string GetImagePath(int imageIndex)
        {
            if (imageIndex >= 0 && imageIndex < _agentPicturePaths.Count)
            {
                return _agentPicturePaths[imageIndex];
            }
            return _agentPicturePaths[_DEFAULT_IMAGE];
        }

        public static ImageBrush GetImage(int imageIndex)
        {
            string path = _agentPicturePaths[_DEFAULT_IMAGE];
            if (imageIndex >= 0 && imageIndex < _agentPicturePaths.Count)
            {
                path = _agentPicturePaths[imageIndex];
                return PathToImageBrush(path);
            }
            else
            {
                return PathToImageBrush(path);
            }
        }

        private static ImageBrush PathToImageBrush(string path)
        {
            try
            {
                string packUriString = "pack://application:,,," + path;
                Uri imageUri = new Uri(packUriString, UriKind.Absolute);
                BitmapImage profilePicture = new BitmapImage(imageUri);
                return new ImageBrush(profilePicture);
            }
            catch (Exception ex) when (ex is UriFormatException || ex is ArgumentException || ex is ArgumentNullException || ex is FileNotFoundException)
            {
                MessageBox.Show(Lang.globalProfilePictureError);
                CodenamesGame.Util.CodenamesGameLogger.Log.Debug("Exception while trying to convert images to ImageBrush: ", ex);
            }
            return null;
        }
    }
}
