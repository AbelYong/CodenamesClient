using CodenamesClient.GameUI.ViewModels;
using CodenamesClient.Operation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CodenamesClient.GameUI.BoardUI
{
    /// <summary>
    /// Lógica de interacción para BoardPage.xaml
    /// </summary>
    public partial class BoardPage : Page
    {
        private BoardViewModel _viewModel;
        private readonly Random _random = new Random();
        private const int AGENT_CODE = 0;
        private const int BYSTANDER_CODE = 1;
        private const int ASSASSIN_CODE = 2;

        public BoardPage()
        {
            InitializeComponent();
            _viewModel = new BoardViewModel();
            this.DataContext = _viewModel;
        }

        private void Click_QuitMatch(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Click_Keyword(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton clickedButton)
            {
                int row = Grid.GetRow(clickedButton);
                int column = Grid.GetColumn(clickedButton);
                int code = _viewModel.AgentsMatrix[row, column];
                switch (code)
                {
                    case AGENT_CODE:
                        clickedButton.Background = GetAgentCardImage();
                        clickedButton.IsEnabled = false;
                        break;
                    case BYSTANDER_CODE:
                        clickedButton.Background = GetBystanderCardImage();
                        clickedButton.IsEnabled = false;
                        break;
                    case ASSASSIN_CODE:
                        clickedButton.Background = GetAssassinCardImage();
                        clickedButton.IsEnabled = false;
                        break;
                }
            }
        }

        private ImageBrush GetAgentCardImage()
        {
            int remainingAgents = _viewModel.AgentNumbers.Count;
            int random = _random.Next(remainingAgents);
            int agentToReveal = _viewModel.AgentNumbers.ElementAt(random);
            ImageBrush card = PictureHandler.GetImage(agentToReveal);
            _viewModel.AgentNumbers.Remove(agentToReveal);
            return card;
        }

        private ImageBrush GetBystanderCardImage()
        {
            int remainingBystanders = _viewModel.BystanderNumbers.Count;
            int random = _random.Next(remainingBystanders);
            int bystanderToReveal = _viewModel.BystanderNumbers.ElementAt(random);

            int imageIndex = bystanderToReveal + BoardViewModel.MAX_AGENTS;
            ImageBrush card = PictureHandler.GetImage(imageIndex);
            _viewModel.BystanderNumbers.Remove(bystanderToReveal);
            return card;
        }

        private ImageBrush GetAssassinCardImage()
        {
            int random = _random.Next(BoardViewModel.MAX_ASSASSINS);
            int assasssinToReveal = BoardViewModel.MAX_AGENTS + BoardViewModel.MAX_BYSTANDERS + random;
            ImageBrush card = PictureHandler.GetImage(assasssinToReveal);
            return card;
        }
    }
}
