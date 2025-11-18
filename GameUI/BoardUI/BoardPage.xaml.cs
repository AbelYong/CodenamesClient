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

        public BoardPage(MatchDM match, Guid myID)
        {
            InitializeComponent();
            _viewModel = new BoardViewModel(match, myID);
            this.DataContext = _viewModel;
            DrawWords();
            DrawKeycard();
            _viewModel.StartChronometer();
            _viewModel.StartTimer();
        }

        private void Click_QuitMatch(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private async void Click_Keyword(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton clickedButton)
            {
                clickedButton.IsEnabled = false;

                int row = Grid.GetRow(clickedButton);
                int column = Grid.GetColumn(clickedButton);
                int code = _viewModel.AgentsMatrix[row, column];
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

                await AnimateCardFlip(clickedButton, isFlippingAway: true, durationMs: 150);

                clickedButton.Background = newBackground;
                clickedButton.Content = string.Empty;

                await AnimateCardFlip(clickedButton, isFlippingAway: false, durationMs: 200);

                switch (code)
                {
                    case AGENT_CODE:
                        _viewModel.StopTimer();
                        _viewModel.AddTime(_viewModel.TurnLength);
                        _viewModel.StartTimer();
                        break;
                    case BYSTANDER_CODE:
                        _viewModel.TurnTimer = 0;
                        _viewModel.StopTimer();
                        MessageBox.Show("Le diste a un civil, se ha terminado tu turno");
                        MessageBox.Show("Ahora es tu turno nuevamente");
                        _viewModel.TurnTimer = _viewModel.TurnLength;
                        _viewModel.StartTimer();
                        break;
                    case ASSASSIN_CODE:
                        _viewModel.TurnTimer = 0;
                        _viewModel.StopTimer();
                        MessageBox.Show("Te encontraste con un asesino, fin del juego");
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
                        Rectangle rectangle = gridKeycard.Children.OfType<Rectangle>()
                            .FirstOrDefault(e => Grid.GetRow(e) == i && Grid.GetColumn(e) == j);
                        rectangle.Fill = agentBrush;
                    }
                    if (_viewModel.Keycard[i, j] == 2)
                    {
                        Rectangle rectangle = gridKeycard.Children.OfType<Rectangle>()
                            .FirstOrDefault(e => Grid.GetRow(e) == i && Grid.GetColumn(e) == j);
                        rectangle.Fill = assasinBrush;
                    }
                }
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
                    Storyboard.SetTargetProperty(opacityAnim, new PropertyPath("Opacity"));
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

        /// <summary>
        /// Animates the card to “flip” using its ScaleTransform (at index 2 of the TransformGroup).
        /// </summary>
        /// <param name="card">The ToggleButton to be animated. </param>
        /// <param name="isFlippingAway">True if it will shrink (1 to 0), False if it will expand (0 to 1). </param>
        /// <param name="durationMs">Duration of the animation in milliseconds.</param>
        /// <returns>A Task that completes when the animation ends.</returns>
        private Task AnimateCardFlip(ToggleButton card, bool isFlippingAway, double durationMs = 150)
        {
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
                Duration = TimeSpan.FromMilliseconds(durationMs),
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

        private async void Click_ReportPlayer(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.IsEnabled = false; // Deshabilitar botón para evitar doble clic
                try
                {
                    // Ejecutar la lógica (que puede tardar unos milisegundos en ir al server)
                    // Nota: Como ReportOpponent en el VM no es asíncrono (void), esto bloqueará
                    // un poco la UI, pero evitará el spam inmediato.
                    _viewModel.ReportCompanion();
                }
                finally
                {
                    btn.IsEnabled = true; // Rehabilitar (opcional, si quieres permitir reportar de nuevo)
                }
            }
        }
    }
}
