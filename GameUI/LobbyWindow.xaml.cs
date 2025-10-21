using CodenamesClient.GameUI.ViewModels;
using CodenamesGame.Domain.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CodenamesClient.GameUI
{
    public partial class LobbyWindow : UserControl
    {
        private LobbyViewModel _viewModel;
        public LobbyWindow(Gamemode gamemode)
        {
            InitializeComponent();
            this._viewModel = new LobbyViewModel(gamemode);
            this.DataContext = this._viewModel;
        }
    }
}
