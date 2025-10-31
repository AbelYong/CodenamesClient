using CodenamesClient.Properties.Langs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CodenamesClient.Operation
{
    public static class PictureHandler
    {
        private static readonly List<String> _agentPicturePaths = new List<string>
        {
            //Agents
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
            //Neutrals
            "/Assets/BoardUI/Neutrals/neutral01.png",
            "/Assets/BoardUI/Neutrals/neutral02.png",
            "/Assets/BoardUI/Neutrals/neutral03.png",
            "/Assets/BoardUI/Neutrals/neutral04.png",
            "/Assets/BoardUI/Neutrals/neutral05.png",
            "/Assets/BoardUI/Neutrals/neutral06.png",
            "/Assets/BoardUI/Neutrals/neutral07.png",
            //Assassins
            "/Assets/BoardUI/Assassins/assassin01.png",
            "/Assets/BoardUI/Assassins/assassin02.png",
            "/Assets/BoardUI/Assassins/assassin03.png"
        };

        //If image index is zero (default) or out of bounds, returns default picture
        public static ImageBrush GetImage(int imageIndex)
        {
            const int DEFAULT_IMAGE = 0;
            string path = _agentPicturePaths[DEFAULT_IMAGE];
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
            catch (UriFormatException)
            {
                MessageBox.Show(Lang.globalProfilePictureError);
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show(Lang.globalProfilePictureError);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(Lang.globalProfilePictureError);
            }
            return null;
        }
    }
}
