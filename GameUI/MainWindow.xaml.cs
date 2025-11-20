using CodenamesClient.GameUI.BoardUI;
using CodenamesClient.GameUI.Pages;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Network;
using CodenamesGame.SessionService;
using System;
using System.ComponentModel;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;

namespace CodenamesClient.GameUI
{
    public partial class MainWindow : Window
    {
        private PlayerDM _player;
        private SessionOperation _sessionOperation;

        public MainWindow()
        {
            InitializeComponent();
            Closing += MainWindowClosing;
            Closed += MainWindowClosed;
            SessionOperation.OnKicked += OnKickedFromServer;
        }

        public void SetPlayer(PlayerDM player)
        {
            _player = player;
        }

        public void SetSessionOperation(SessionOperation session)
        {
            _sessionOperation = session;
        }

        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            if (MainFrame.Content is Page currentPage)
            {
                if (currentPage is MainMenuPage mainMenu)
                {
                    _player = mainMenu.GetViewModel().Player;
                    _sessionOperation = mainMenu.GetViewModel().Session;
                }
                else if (currentPage is LobbyPage lobby)
                {
                    //TODO
                }
                else if (currentPage is BoardPage)
                {
                    //TODO
                }
            }
        }

        private void MainWindowClosed(object sender, EventArgs e)
        {
            try
            {
                _sessionOperation?.Disconnect(_player);
                LobbyOperation.Instance.Disconnect();
                MatchmakingOperation.Instance.Disconnect();

            }
            catch (Exception ex) when (ex is CommunicationException || ex is CommunicationObjectFaultedException || ex is CommunicationObjectFaultedException)
            {
                //FIXME
                MessageBox.Show(ex.Message);
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
