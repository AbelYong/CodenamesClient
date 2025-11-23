using CodenamesClient.GameUI.Pages;
using CodenamesGame.Network;
using CodenamesGame.SessionService;
using System;
using System.ServiceModel;
using System.Windows;

namespace CodenamesClient.GameUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Closed += MainWindowClosed;
            SessionCallbackHandler.OnKicked += OnKickedFromServer;
        }

        private void MainWindowClosed(object sender, EventArgs e)
        {
            try
            {
                SessionOperation.Instance.Disconnect();
                SocialOperation.Instance.Terminate();
                LobbyOperation.Instance.Disconnect();
                MatchmakingOperation.Instance.Disconnect();
                MatchOperation.Instance.Disconnect();
            }
            catch (Exception ex) when (ex is CommunicationException || ex is CommunicationObjectFaultedException || ex is CommunicationObjectFaultedException)
            {
                //Do nothing
            }
        }

        private void OnKickedFromServer(object sender, BanReason reason)
        {
            Dispatcher.Invoke(() =>
            {
                string message = "";
                switch (reason)
                {
                    case BanReason.TemporaryBan:
                        message = Properties.Langs.Lang.kickMessageTemp;
                        break;
                    case BanReason.PermanentBan:
                        message = Properties.Langs.Lang.kickMessagePerm;
                        break;
                    default:
                        message = Properties.Langs.Lang.globalDisconnected;
                        break;
                }

                MessageBox.Show(message, Properties.Langs.Lang.kickTitle, MessageBoxButton.OK, MessageBoxImage.Stop);

                MainFrame.Navigate(new LoginPage());
            });
        }
    }
}
