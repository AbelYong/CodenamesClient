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
            DrawWords();
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
                        clickedButton.Content = string.Empty;
                        clickedButton.IsEnabled = false;
                        break;
                    case BYSTANDER_CODE:
                        clickedButton.Background = GetBystanderCardImage();
                        clickedButton.Content = string.Empty;
                        clickedButton.IsEnabled = false;
                        break;
                    case ASSASSIN_CODE:
                        clickedButton.Background = GetAssassinCardImage();
                        clickedButton.Content = string.Empty;
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
            int random = _random.Next(PictureHandler.NUMBER_OF_BYSTANDER_PICTURES);

            int imageIndex = random + PictureHandler.NUMBER_OF_AGENT_PICTURES;
            ImageBrush card = PictureHandler.GetImage(imageIndex);
            return card;
        }

        private ImageBrush GetAssassinCardImage()
        {
            int random = _random.Next(BoardViewModel.MAX_GLOBAL_ASSASSINS);
            int assasssinToReveal = BoardViewModel.MAX_GLOBAL_AGENTS + BoardViewModel.MAX_GLOBAL_BYSTANDERS + random;
            ImageBrush card = PictureHandler.GetImage(assasssinToReveal);
            return card;
        }

        private void DrawWords()
        {
            int rows = gridBoard.RowDefinitions.Count;
            int columns = gridBoard.ColumnDefinitions.Count;
            int totalSpots = rows * columns;
            foreach (KeyValuePair<int, string> word in _viewModel.Keywords)
            {
                int flatIndex = word.Key;
                int row = flatIndex / columns;
                int column = flatIndex % columns;
                ToggleButton toggleButton = gridBoard.Children.OfType<ToggleButton>()
                    .FirstOrDefault(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == column);
                toggleButton.Content = word.Value;
            }
        }
    }
}
