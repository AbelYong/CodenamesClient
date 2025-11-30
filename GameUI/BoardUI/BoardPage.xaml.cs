using CodenamesClient.GameUI.ViewModels;
using CodenamesClient.Operation;
using CodenamesGame.Domain.POCO;
using CodenamesGame.Domain.POCO.Match;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Media.Animation;
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
        private MediaPlayer _mediaPlayer = new MediaPlayer();

        public BoardPage(MatchDM match, Guid myID)
        {
            InitializeComponent();
            _viewModel = new BoardViewModel(match, myID);

            _viewModel.GoBackToMenu += OnGoBackToMenu;

            _viewModel.OnAgentFlipRequested += HandleAgentFlip;
            _viewModel.OnBystanderFlipRequested += HandleBystanderFlip;
            _viewModel.OnAssassinFlipRequested += HandleAssassinFlip;

            this.DataContext = _viewModel;
            DrawWords();
            DrawKeycard();
            _viewModel.StartChronometer();
            _viewModel.StartTimer();
        }

        private void HandleAgentFlip(BoardCoordinatesDM coordinates)
        {
            FlashAgentLight();
            FlipCardAt(coordinates, AGENT_CODE, false);
        }

        private void HandleBystanderFlip(BoardCoordinatesDM coordinates)
        {
            FlashBystanderLight();

            ToggleButton btn = GetButtonAt(coordinates.Row, coordinates.Column);
            bool iPickedIt = btn != null && (btn.IsChecked == true);

            FlipCardAt(coordinates, BYSTANDER_CODE, !iPickedIt);
        }

        private void HandleAssassinFlip(BoardCoordinatesDM coordinates)
        {
            FlipCardAt(coordinates, ASSASSIN_CODE, false);
            if (_viewModel.AmISpymaster)
            {
                TriggerAssassinSequence();
            }
            else
            {
                TriggerKilledSequence();
            }
        }

        private async void FlipCardAt(BoardCoordinatesDM coordinates, int code, bool keepInteractiveForSpymaster)
        {
            ToggleButton button = GetButtonAt(coordinates.Row, coordinates.Column);
            if (button == null)
            {
                return;
            }

            button.IsChecked = true;
            button.IsEnabled = false;

            ImageBrush newBackground;
            switch (code)
            {
                case AGENT_CODE:
                    newBackground = GetAgentCardImage();
                    break;
                case BYSTANDER_CODE:
                    newBackground = GetBystanderCardImage();
                    break;
                case ASSASSIN_CODE:
                    newBackground = GetAssassinCardImage();
                    break;
                default:
                    newBackground = new ImageBrush();
                    break;
            }

            await AnimateCardFlip(button, isFlippingAway: true);

            button.Background = newBackground;

            if (keepInteractiveForSpymaster && code == BYSTANDER_CODE)
            {
                button.IsEnabled = true;
                if (button.Content is string text && !string.IsNullOrEmpty(text))
                {
                    button.Content = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                        CornerRadius = new CornerRadius(5),
                        Padding = new Thickness(5),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Child = new TextBlock
                        {
                            Text = text,
                            Foreground = Brushes.White,
                            FontWeight = FontWeights.Bold,
                            FontSize = 14,
                            FontFamily = new FontFamily("Consolas"),
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center
                        }
                    };
                }
            }
            else
            {
                button.Content = string.Empty;
                button.IsEnabled = false;
            }

            await AnimateCardFlip(button, isFlippingAway: false);
        }

        private ToggleButton GetButtonAt(int row, int col)
        {
            return gridBoard.Children.OfType<ToggleButton>()
                .FirstOrDefault(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col);
        }

        private async void FlashAgentLight()
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Close();
            LightAgentOn.BeginAnimation(UIElement.OpacityProperty, null);
            LightAgentOn.Opacity = 0;

            LightAgentOn.Opacity = 1;
            AudioManager.Instance.PlaySoundEffect("Assets/AudioGame/friend.mp3");

            await Task.Delay(4000);

            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));

            fadeOut.Completed += (s, e) =>
            {
                LightAgentOn.BeginAnimation(UIElement.OpacityProperty, null);
                LightAgentOn.Opacity = 0;
            };
            LightAgentOn.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            _mediaPlayer.Stop();
        }

        private async void FlashBystanderLight()
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Close();
            LightBystanderOn.BeginAnimation(UIElement.OpacityProperty, null);
            LightBystanderOn.Opacity = 0;

            LightBystanderOn.Opacity = 1;
            AudioManager.Instance.PlaySoundEffect("Assets/AudioGame/cDown.mp3");

            await Task.Delay(5000);

            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(500));

            fadeOut.Completed += (s, e) =>
            {
                LightBystanderOn.BeginAnimation(UIElement.OpacityProperty, null);
                LightBystanderOn.Opacity = 0;
            };

            LightBystanderOn.BeginAnimation(UIElement.OpacityProperty, fadeOut);

            _mediaPlayer.Stop();
        }

        private async void TriggerAssassinSequence()
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Close();
            LightAssassinOn.BeginAnimation(UIElement.OpacityProperty, null);

            DoubleAnimation blink = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
            blink.AutoReverse = true;
            blink.RepeatBehavior = RepeatBehavior.Forever;

            LightAssassinOn.BeginAnimation(UIElement.OpacityProperty, blink);

            AudioManager.Instance.PlaySoundEffect("Assets/AudioGame/sos.mp3");

            await Task.Delay(4000);

            _mediaPlayer.Stop();
            LightAssassinOn.BeginAnimation(UIElement.OpacityProperty, null);
            LightAssassinOn.Opacity = 0;

            _viewModel.ShowGameOverScreen();
        }

        private async void TriggerKilledSequence()
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Close();
            AudioManager.Instance.PlaySoundEffect("Assets/AudioGame/shot.mp3");

            await Task.Delay(2800);

            if (BrokenScreenImage != null)
            {
                BrokenScreenImage.Visibility = Visibility.Visible;
            }

            await Task.Delay(1200);

            _viewModel.ShowGameOverScreen();
        }

        private void OnGoBackToMenu()
        {
            if (NavigationService != null)
            {
                NavigationService.GoBack();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Close();

            _viewModel.StopTimer();
            _viewModel.StopChronometer();
            _viewModel.Disconnect();

            _viewModel.GoBackToMenu -= OnGoBackToMenu;

            _viewModel.OnAgentFlipRequested -= HandleAgentFlip;
            _viewModel.OnBystanderFlipRequested -= HandleBystanderFlip;
            _viewModel.OnAssassinFlipRequested -= HandleAssassinFlip;
        }

        private void Click_QuitMatch(object sender, RoutedEventArgs e)
        {
            _viewModel.GoBackToMenu -= OnGoBackToMenu;
            NavigationService.GoBack();
        }

        private async void Click_Keyword(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton clickedButton)
            {
                int row = Grid.GetRow(clickedButton);
                int column = Grid.GetColumn(clickedButton);
                int code = _viewModel.AgentsMatrix[row, column];

                if (_viewModel.AmISpymaster && clickedButton.Content.ToString() != string.Empty)
                {
                    int trueCode = _viewModel.Keycard[row, column];

                    FlipCardAt(new BoardCoordinatesDM(row, column), trueCode, false);
                    return;
                }

                clickedButton.IsChecked = true;
                clickedButton.IsEnabled = false;

                ImageBrush newBackground;
                switch (code)
                {
                    case AGENT_CODE:
                        newBackground = GetAgentCardImage();
                        break;
                    case BYSTANDER_CODE:
                        newBackground = GetBystanderCardImage();
                        break;
                    case ASSASSIN_CODE:
                        newBackground = GetAssassinCardImage();
                        break;
                    default:
                        newBackground = new ImageBrush();
                        break;
                }

                await AnimateCardFlip(clickedButton, isFlippingAway: true);

                clickedButton.Background = newBackground;
                clickedButton.Content = string.Empty;

                await AnimateCardFlip(clickedButton, isFlippingAway: false);

                BoardCoordinatesDM coordinates = new BoardCoordinatesDM(row, column);
                switch (code)
                {
                    case AGENT_CODE:
                        _viewModel.HandleAgentSelection(coordinates);
                        break;
                    case BYSTANDER_CODE:
                        _viewModel.HandleBystanderSelection(coordinates);
                        break;
                    case ASSASSIN_CODE:
                        _viewModel.HandleAssassinSelection(coordinates);
                        break;
                }
            }
        }

        private ImageBrush GetAgentCardImage()
        {
            if (_viewModel.AgentNumbers.Count == 0)
            {
                return new ImageBrush();
            }

            int remainingAgents = _viewModel.AgentNumbers.Count;
            int random = _random.Next(remainingAgents);

            int agentToReveal = _viewModel.AgentNumbers[random];
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
            int columns = gridBoard.ColumnDefinitions.Count;
            foreach (KeyValuePair<int, string> word in _viewModel.Keywords)
            {
                int flatIndex = word.Key;
                int row = flatIndex / columns;
                int column = flatIndex % columns;
                ToggleButton toggleButton = gridBoard.Children.OfType<ToggleButton>()
                    .FirstOrDefault(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == column);
                if (toggleButton != null)
                {
                    toggleButton.Content = word.Value;
                }
            }
        }

        private void DrawKeycard()
        {
            SolidColorBrush agentBrush = new SolidColorBrush(Colors.Green);
            SolidColorBrush assasinBrush = new SolidColorBrush(Colors.Black);
            for (int i = 0; i < gridKeycard.RowDefinitions.Count; i++)
            {
                for (int j = 0; j < gridKeycard.ColumnDefinitions.Count; j++)
                {
                    if (_viewModel.Keycard[i, j] == 0)
                    {
                        DrawKeycardCell(i, j, agentBrush);
                    }
                    else if (_viewModel.Keycard[i, j] == 2)
                    {
                        DrawKeycardCell(i, j, assasinBrush);
                    }
                }
            }
        }

        private void DrawKeycardCell(int i, int j, SolidColorBrush brush)
        {
            Rectangle rectangle = gridKeycard.Children.OfType<Rectangle>()
                    .FirstOrDefault(e => Grid.GetRow(e) == i && Grid.GetColumn(e) == j);
            if (rectangle != null)
            {
                rectangle.Fill = brush;
            }
        }

        private void AnimateBoardCards()
        {
            int delay = 0;
            int delayIncrement = 20;
            int animationDuration = 600;

            double startTranslateX = -500;
            double startTranslateY = 300;
            double startRotation = -45;

            foreach (UIElement element in gridBoard.Children)
            {
                if (element is ToggleButton card)
                {
                    var translate = new TranslateTransform(startTranslateX, startTranslateY);
                    var rotate = new RotateTransform(startRotation);
                    var scale = new ScaleTransform(1.0, 1.0);

                    var group = new TransformGroup();
                    group.Children.Add(translate);
                    group.Children.Add(rotate);
                    group.Children.Add(scale);

                    card.RenderTransform = group;

                    card.RenderTransformOrigin = new Point(0.5, 0.5);

                    card.Opacity = 0;

                    var storyboard = new Storyboard();

                    var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

                    var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(animationDuration))
                    {
                        EasingFunction = ease,
                        BeginTime = TimeSpan.FromMilliseconds(delay)
                    };
                    Storyboard.SetTarget(opacityAnim, card);
                    Storyboard.SetTargetProperty(opacityAnim, new PropertyPath(UIElement.OpacityProperty));
                    storyboard.Children.Add(opacityAnim);

                    var slideXAnim = new DoubleAnimation(startTranslateX, 0, TimeSpan.FromMilliseconds(animationDuration))
                    {
                        EasingFunction = ease,
                        BeginTime = TimeSpan.FromMilliseconds(delay)
                    };
                    Storyboard.SetTarget(slideXAnim, card);
                    Storyboard.SetTargetProperty(slideXAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
                    storyboard.Children.Add(slideXAnim);

                    var slideYAnim = new DoubleAnimation(startTranslateY, 0, TimeSpan.FromMilliseconds(animationDuration))
                    {
                        EasingFunction = ease,
                        BeginTime = TimeSpan.FromMilliseconds(delay)
                    };
                    Storyboard.SetTarget(slideYAnim, card);
                    Storyboard.SetTargetProperty(slideYAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
                    storyboard.Children.Add(slideYAnim);

                    var rotateAnim = new DoubleAnimation(startRotation, 0, TimeSpan.FromMilliseconds(animationDuration))
                    {
                        EasingFunction = ease,
                        BeginTime = TimeSpan.FromMilliseconds(delay)
                    };
                    Storyboard.SetTarget(rotateAnim, card);
                    Storyboard.SetTargetProperty(rotateAnim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)"));
                    storyboard.Children.Add(rotateAnim);

                    storyboard.Begin();

                    delay += delayIncrement;
                }
            }
        }

        private Task AnimateCardFlip(ToggleButton card, bool isFlippingAway)
        {
            double durationMiliseconds = 150;
            var transformGroup = card.RenderTransform as TransformGroup;
            if (transformGroup == null || transformGroup.Children.Count <= 2 || !(transformGroup.Children[2] is ScaleTransform scaleTransform))
            {
                return Task.CompletedTask;
            }

            double fromValue = isFlippingAway ? 1.0 : 0.0;
            double toValue = isFlippingAway ? 0.0 : 1.0;

            var anim = new DoubleAnimation
            {
                From = fromValue,
                To = toValue,
                Duration = TimeSpan.FromMilliseconds(durationMiliseconds),
                EasingFunction = isFlippingAway ? (IEasingFunction)new BackEase { EasingMode = EasingMode.EaseIn, Amplitude = 0.3 }
                                                : new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
            };

            var tcs = new TaskCompletionSource<bool>();
            anim.Completed += (s, e) =>
            {
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                scaleTransform.ScaleX = toValue;
                tcs.SetResult(true);
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);

            return tcs.Task;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            AnimateBoardCards();
        }

        private void Click_ReportPlayer(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.IsEnabled = false;
                try
                {
                    _viewModel.ReportCompanion();
                }
                finally
                {
                    btn.IsEnabled = true;
                }
            }
        }

        private void Click_SkipTurn(object sender, RoutedEventArgs e)
        {
            _viewModel.SkipTurn();
        }

        private void Click_SendMessage(object sender, RoutedEventArgs e)
        {
            _viewModel.SendMessage();
        }

        private void KeyDown_ChatInput(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.SendMessage();
            }
        }
    }
}