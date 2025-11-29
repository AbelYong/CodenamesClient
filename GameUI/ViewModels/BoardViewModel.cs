using CodenamesGame.Domain.POCO;
using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.Network;
using CodenamesGame.Network.EventArguments;
using CodenamesGame.MatchService;
using CodenamesClient.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using CodenamesClient.Properties.Langs;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace CodenamesClient.GameUI.ViewModels
{
    public class BoardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action GoBackToMenu;

        public event Action<BoardCoordinatesDM> OnAgentFlipRequested;
        public event Action<BoardCoordinatesDM> OnBystanderFlipRequested;
        public event Action<BoardCoordinatesDM> OnAssassinFlipRequested;

        public const int MAX_GLOBAL_AGENTS = 15;
        public const int MAX_GLOBAL_ASSASSINS = 3;
        public const int MAX_GLOBAL_BYSTANDERS = 7;
        public const int MAX_TURN_LENGTH = 60;

        private readonly MatchDM _match;
        private readonly ModerationOperation _moderationOperation;

        private PlayerDM _me;
        private PlayerDM _companion;

        private string _playerSTurn;
        private string _turnInstrucions;
        private string _myUsername;
        private string _companionUsername;
        private string _chatInput;
        private string _gameOverTitle;
        private string _gameOverMessage;
        private string _gameOverImageSource;
        private string _overlayColor;

        private DispatcherTimer _timer;
        private DispatcherTimer _chronometer;
        private TimeSpan _elapsedTime;

        private readonly int[,] _agentsMatrix;
        private readonly int[,] _keycardMatrix;
        private readonly Dictionary<int, string> _keywords = new Dictionary<int, string>();

        private int _turnTimer;
        private int _timerTokens;
        private int _bystanderTokens;
        private int _turnLength;

        private bool _isGameOverVisible;
        private bool _showAssassinImage;
        private bool _isChatEnabled;
        private bool _isBoardEnabled;
        private bool _isSkipButtonVisible;
        private bool _amISpymaster;

        public ObservableCollection<ChatMessageDM> ChatMessages { get; set; }
        public List<int> AgentNumbers { get; set; }

        public bool AmISpymaster => _amISpymaster;

        public string PlayerSTurn
        {
            get => _playerSTurn;
            set
            {
                _playerSTurn = value;
                OnPropertyChanged();
            }
        }

        public string TurnInstructions
        {
            get => _turnInstrucions;
            set
            {
                _turnInstrucions = value;
                OnPropertyChanged();
            }
        }

        public string MyUsername
        {
            get => _myUsername;
            set
            {
                _myUsername = value;
                OnPropertyChanged();
            }
        }

        public string CompanionUsername
        {
            get => _companionUsername;
            set
            {
                _companionUsername = value;
                OnPropertyChanged();
            }
        }

        public string ChatInput
        {
            get => _chatInput;
            set
            {
                _chatInput = value;
                OnPropertyChanged();
            }
        }

        public bool IsChatEnabled
        {
            get => _isChatEnabled;
            set
            {
                _isChatEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsBoardEnabled
        {
            get => _isBoardEnabled;
            set
            {
                _isBoardEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsSkipButtonVisible
        {
            get => _isSkipButtonVisible;
            set
            {
                _isSkipButtonVisible = value;
                OnPropertyChanged();
            }
        }

        public int[,] AgentsMatrix
        {
            get => _agentsMatrix;
        }

        public Dictionary<int, string> Keywords
        {
            get => _keywords;
        }

        public int[,] Keycard
        {
            get => _keycardMatrix;
        }

        public int TurnLength
        {
            get => _turnLength;
        }

        public int TurnTimer
        {
            get => _turnTimer;
            set
            {
                _turnTimer = value;
                OnPropertyChanged();
            }
        }

        public int TimerTokens
        {
            get => _timerTokens;
            set
            {
                _timerTokens = value;
                OnPropertyChanged();
            }
        }

        public int BystanderTokens
        {
            get => _bystanderTokens;
            set
            {
                _bystanderTokens = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan ElapsedTime
        {
            get => _elapsedTime;
            set
            {
                _elapsedTime = value;
                OnPropertyChanged();
            }
        }

        public bool IsGameOverVisible
        {
            get => _isGameOverVisible;
            set
            {
                _isGameOverVisible = value;
                OnPropertyChanged();
            }
        }

        public string GameOverTitle
        {
            get => _gameOverTitle;
            set
            {
                _gameOverTitle = value;
                OnPropertyChanged();
            }
        }

        public string GameOverMessage
        {
            get => _gameOverMessage;
            set
            {
                _gameOverMessage = value;
                OnPropertyChanged();
            }
        }

        public string GameOverImageSource
        {
            get => _gameOverImageSource;
            set
            {
                _gameOverImageSource = value;
                OnPropertyChanged();
            }
        }

        public bool ShowAssassinImage
        {
            get => _showAssassinImage;
            set
            {
                _showAssassinImage = value;
                OnPropertyChanged();
            }
        }

        public string OverlayColor
        {
            get => _overlayColor;
            set
            {
                _overlayColor = value;
                OnPropertyChanged();
            }
        }

        public BoardViewModel(MatchDM match, Guid myID)
        {
            _match = match;
            MatchOperation.Instance.Initialize(myID);
            MatchOperation.Instance.JoinMatch(match);
            _moderationOperation = new ModerationOperation();
            ChatMessages = new ObservableCollection<ChatMessageDM>();

            SetPlayers(myID);
            InitializeAgentNumbers();
            _agentsMatrix = match.Board;
            SetKeywords(match.SelectedWords);
            _keycardMatrix = match.Keycard;
            InitializeMatchData(match);
            InitializeChronometer();
            InitializeTimer();
            SubscribeToCallbacks();

            SetInitialTurn();
        }

        private void SubscribeToCallbacks()
        {
            MatchCallbackHandler.OnCompanionDisconnect += HandleCompanionDisconnect;
            MatchCallbackHandler.OnClueReceived += HandleClueReceived;
            MatchCallbackHandler.OnTurnChange += HandleTurnChange;
            MatchCallbackHandler.OnRolesChanged += HandleRolesChanged;
            MatchCallbackHandler.OnGuesserTurnTimeout += HandleGuesserTurnTimeout;
            MatchCallbackHandler.OnAgentPicked += HandleAgentPicked;
            MatchCallbackHandler.OnBystanderPicked += HandleBystanderPicked;
            MatchCallbackHandler.OnMatchWon += HandleMatchWon;
            MatchCallbackHandler.OnMatchTimeout += HandleMatchTimeout;
            MatchCallbackHandler.OnAssassinPicked += HandleAssassinPicked;
            MatchCallbackHandler.OnScoreNotSaved += HandleScoreNotSaved;
        }

        private void UnsubscribeCallbacks()
        {
            MatchCallbackHandler.OnCompanionDisconnect -= HandleCompanionDisconnect;
            MatchCallbackHandler.OnClueReceived -= HandleClueReceived;
            MatchCallbackHandler.OnTurnChange -= HandleTurnChange;
            MatchCallbackHandler.OnGuesserTurnTimeout -= HandleGuesserTurnTimeout;
            MatchCallbackHandler.OnRolesChanged -= HandleRolesChanged;
            MatchCallbackHandler.OnAgentPicked -= HandleAgentPicked;
            MatchCallbackHandler.OnBystanderPicked -= HandleBystanderPicked;
            MatchCallbackHandler.OnMatchWon -= HandleMatchWon;
            MatchCallbackHandler.OnMatchTimeout -= HandleMatchTimeout;
            MatchCallbackHandler.OnAssassinPicked -= HandleAssassinPicked;
            MatchCallbackHandler.OnScoreNotSaved -= HandleScoreNotSaved;
        }

        private void SetPlayers(Guid myID)
        {
            if (_match != null)
            {
                bool isPlayerRequester = _match.Requester.PlayerID == myID;
                if (isPlayerRequester)
                {
                    MyUsername = _match.Requester.Username;
                    _me = _match.Requester;
                    CompanionUsername = _match.Companion.Username;
                    _companion = _match.Companion;
                }
                else
                {
                    MyUsername = _match.Companion.Username;
                    _me = _match.Companion;
                    CompanionUsername = _match.Requester.Username;
                    _companion = _match.Requester;
                }
            }
        }

        private void SetInitialTurn()
        {
            _amISpymaster = _me.PlayerID == _match.Requester.PlayerID;
            UpdateUIState();
        }

        private void UpdateUIState()
        {
            if (_amISpymaster)
            {
                PlayerSTurn = string.Format(Lang.matchYouAreSpymaster, CompanionUsername);
                TurnInstructions = string.Format(Lang.matchTypeAClue, CompanionUsername);
                IsChatEnabled = true;
                IsBoardEnabled = false;
                IsSkipButtonVisible = false;
            }
            else
            {
                PlayerSTurn = string.Format(Lang.matchCompanionIsSpymaster, CompanionUsername);
                TurnInstructions = string.Format(Lang.matchWaitForClue, CompanionUsername);
                IsChatEnabled = false;
                IsBoardEnabled = false;
                IsSkipButtonVisible = false;
            }
        }

        private void HandleCompanionDisconnect()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StopTimer();
                StopChronometer();
                MessageBox.Show(Lang.errorCompanionLostConnection, Lang.globalInformationTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                GoBackToMenu?.Invoke();
            });
        }

        private void HandleClueReceived(object sender, string clue)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AddMessageToChat(clue, false);

                if (!_amISpymaster)
                {
                    if (!IsBoardEnabled)
                    {
                        ResetTimer();
                    }

                    IsBoardEnabled = true;
                    IsSkipButtonVisible = true;
                    IsChatEnabled = false;
                    TurnInstructions = string.Format(Lang.matchPickKeywords, CompanionUsername);
                }
            });
        }

        private void HandleTurnChange()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!_amISpymaster)
                {
                    if (!IsBoardEnabled)
                    {
                        ResetTimer();
                    }
                    IsBoardEnabled = true;
                    IsSkipButtonVisible = true;
                    IsChatEnabled = false;
                    TurnInstructions = string.Format(Lang.matchPickKeywords, CompanionUsername);
                }
                else
                {
                    IsChatEnabled = false;
                    IsBoardEnabled = false;
                    TurnInstructions = string.Format(Lang.matchWaitForCompanion, CompanionUsername);
                    StopTimer();
                }
            });
        }

        private void HandleGuesserTurnTimeout(object sender, int e)
        {
            TimerTokens = e;
        }

        private void HandleRolesChanged()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _amISpymaster = !_amISpymaster;
                UpdateUIState();
                ResetTimer();
            });
        }

        private void HandleAgentPicked(object sender, AgentPickedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnAgentFlipRequested?.Invoke(e.Coordinates);
                if (_amISpymaster)
                {
                    TurnTimer = e.NewTurnLength;
                    StartTimer();
                }
            });
        }

        private void HandleBystanderPicked(object sender, BystanderPickedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnBystanderFlipRequested?.Invoke(e.Coordinates);
                if (e.TokenToUpdate == TokenType.TIMER)
                {
                    TimerTokens = e.RemainingTokens;
                }
                else if (e.TokenToUpdate == TokenType.BYSTANDER)
                {
                    BystanderTokens = e.RemainingTokens;
                }
            });
        }

        private void HandleAssassinPicked(object sender, AssassinPickedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StopTimer();
                StopChronometer();

                GameOverTitle = Lang.matchDefeat;
                GameOverMessage = _amISpymaster ? string.Format(Lang.matchDefeatAssassinCompanion, CompanionUsername) : Lang.matchDefeatAssassinMessage;
                OverlayColor = "#CC000000";
                GameOverImageSource = "/Assets/BoardUI/Assassins/assassin01.png";
                ShowAssassinImage = true;

                OnAssassinFlipRequested?.Invoke(e.Coordinates);

                if (!_amISpymaster)
                {
                    ShowGameOverScreen();
                }
            });
        }

        private static void HandleScoreNotSaved()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show("La partida ha terminado pero hubo un error al guardar las estadísticas.",
                    Lang.globalWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }

        public void Disconnect()
        {
            UnsubscribeCallbacks();
            MatchOperation.Instance.Disconnect();
        }

        public void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(ChatInput) || !IsChatEnabled)
            {
                return;
            }

            string trimmedMessage = ChatInput.Trim();

            if (!IsValidClueFormat(trimmedMessage))
            {
                MessageBox.Show(Lang.errorInvalidClueFormat, Lang.globalWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string wordPart = trimmedMessage.Split(' ')[0];

            if (IsMessageIllegal(wordPart))
            {
                MessageBox.Show(Lang.errorIllegalClue, Lang.globalWarningTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MatchOperation.Instance.SendClue(trimmedMessage);

            AddMessageToChat(trimmedMessage, true);
            ChatInput = string.Empty;

            if (_amISpymaster)
            {
                IsChatEnabled = false;
                IsBoardEnabled = false;
                TurnInstructions = string.Format(Lang.matchWaitForCompanion, CompanionUsername);

                StopTimer();
            }
        }

        private static bool IsValidClueFormat(string message)
        {
            string[] parts = message.Split(' ');
            if (parts.Length != 2)
            {
                return false;
            }

            string word = parts[0];
            string number = parts[1];

            if (!Regex.IsMatch(word, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ]+$"))
            {
                return false;
            }

            if (!int.TryParse(number, out _))
            {
                return false;
            }

            return true;
        }

        private bool IsMessageIllegal(string clueWord)
        {
            List<string> englishWords = GetWordsForCulture(new CultureInfo("en"));
            List<string> spanishWords = GetWordsForCulture(new CultureInfo("es"));

            foreach (int index in _keywords.Keys)
            {
                string boardWordCurrent = _keywords[index];
                string boardWordEn = (index < englishWords.Count) ? englishWords[index] : string.Empty;
                string boardWordEs = (index < spanishWords.Count) ? spanishWords[index] : string.Empty;

                if (IsIllegalMatch(clueWord, boardWordCurrent) ||
                    IsIllegalMatch(clueWord, boardWordEn) ||
                    IsIllegalMatch(clueWord, boardWordEs))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsIllegalMatch(string clue, string boardWord)
        {
            if (string.IsNullOrEmpty(boardWord))
            {
                return false;
            }

            string c = clue.Trim().ToLowerInvariant();
            string b = boardWord.Trim().ToLowerInvariant();

            return c.Equals(b) || b.Contains(c) || c.Contains(b);
        }

        private void AddMessageToChat(string text, bool isMine)
        {
            ChatMessages.Add(new ChatMessageDM
            {
                Message = text,
                Author = isMine ? MyUsername : CompanionUsername,
                IsMine = isMine,
                Timestamp = DateTime.Now
            });
        }

        public void SkipTurn()
        {
            var currentRole = _amISpymaster ? MatchRoleType.SPYMASTER : MatchRoleType.GUESSER;

            MatchOperation.Instance.NotifyTurnTimeout(currentRole);
            TimerTokens--;
            IsBoardEnabled = false;
            IsSkipButtonVisible = false;
            IsChatEnabled = false;
        }

        public void HandleAgentSelection(BoardCoordinatesDM coordinates)
        {
            AddTime(TurnLength);
            MatchOperation.Instance.NotifyPickedAgent(coordinates, TurnTimer);
        }

        public void HandleBystanderSelection(BoardCoordinatesDM coordinates)
        {
            StopTimer();
            TurnTimer = 0;
            if (_match.Rules.gamemode != GamemodeDM.CUSTOM)
            {
                if (TimerTokens >= 0)
                {
                    TimerTokens--;
                    MatchOperation.Instance.NotifyPickedBystander(coordinates);
                }
            }
            else
            {
                if (BystanderTokens > 0)
                {
                    BystanderTokens--;
                    MatchOperation.Instance.NotifyPickedBystander(coordinates);
                }
                else
                {
                    if (TimerTokens >= 0)
                    {
                        int auxTimerTokens = TimerTokens - MatchRulesDM.TIMER_TOKENS_TO_TAKE_CUSTOM;
                        TimerTokens = auxTimerTokens >= 0 ? auxTimerTokens : 0;
                        MatchOperation.Instance.NotifyPickedBystander(coordinates);
                    }
                }
            }

            IsBoardEnabled = false;
            IsSkipButtonVisible = false;
        }

        public void HandleAssassinSelection(BoardCoordinatesDM coordinates)
        {
            StopTimer();
            MatchOperation.Instance.NotifyPickedAssassin(coordinates);
        }


        private void InitializeAgentNumbers()
        {
            AgentNumbers = new List<int>();
            for (int i = 0; i < MAX_GLOBAL_AGENTS; i++)
            {
                AgentNumbers.Add(i);
            }
        }

        private void SetKeywords(List<int> wordlist)
        {
            const int TOTAL_KEYCARDS = 25;
            _keywords.Clear();

            List<string> allWords = GetCurrentCultureWords();
            for (int i = 0; i < TOTAL_KEYCARDS; i++)
            {
                if (wordlist[i] < allWords.Count)
                {
                    _keywords.Add(i, allWords[wordlist[i]]);
                }
            }
        }

        private static List<string> GetCurrentCultureWords()
        {
            return GetWordsForCulture(CultureInfo.CurrentUICulture);
        }

        private static List<string> GetWordsForCulture(CultureInfo culture)
        {
            ResourceManager manager = Properties.Langs.GameWords.ResourceManager;
            ResourceSet resourceSet = manager.GetResourceSet(culture, true, true);

            if (resourceSet == null)
            {
                return new List<string>();
            }
            List<string> allWords = resourceSet.OfType<DictionaryEntry>()
                .Where(entry => entry.Value is string).Select(entry => (string)entry.Value).ToList();
            return allWords;
        }

        private void InitializeMatchData(MatchDM match)
        {
            _turnLength = match.Rules.TurnTimer;
            _timerTokens = match.Rules.TimerTokens;
            _bystanderTokens = match.Rules.BystanderTokens;
        }

        private void InitializeTimer()
        {
            _turnTimer = _turnLength;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += TimerTick;
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (TurnTimer > 0)
            {
                TurnTimer--;
            }
            else
            {
                _timer.Stop();

                if (_amISpymaster)
                {
                    MatchOperation.Instance.NotifyTurnTimeout(MatchRoleType.SPYMASTER);
                }
                else
                {
                    if (_isBoardEnabled)
                    {
                        MatchOperation.Instance.NotifyTurnTimeout(MatchRoleType.GUESSER);
                        TimerTokens--;
                    }
                }
            }
        }

        private void ResetTimer()
        {
            TurnTimer = _turnLength;
            StartTimer();
        }

        public void StartTimer()
        {
            _timer.Start();
        }

        public void AddTime(int seconds)
        {
            int turnLength = TurnTimer + seconds;
            TurnTimer = turnLength < MatchRulesDM.MAX_TURN_TIMER ? turnLength : MatchRulesDM.MAX_TURN_TIMER;
        }

        public void StopTimer()
        {
            _timer.Stop();
        }

        private void InitializeChronometer()
        {
            ElapsedTime = TimeSpan.Zero;
            _chronometer = new DispatcherTimer();
            _chronometer.Interval = TimeSpan.FromSeconds(1);
            _chronometer.Tick += ChronometerTick;
        }

        private void ChronometerTick(object sender, EventArgs e)
        {
            ElapsedTime = ElapsedTime.Add(TimeSpan.FromSeconds(1));
        }

        public void StartChronometer()
        {
            _chronometer.Start();
        }

        public void StopChronometer()
        {
            _chronometer.Stop();
            ElapsedTime = TimeSpan.Zero;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void ReportCompanion()
        {
            if (_companion == null)
            {
                return;
            }

            var result = MessageBox.Show(
                string.Format(Lang.confirmReportMessage, _companion.Username),
                Lang.globalWarningTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                string reason = "Conducta antideportiva";

                var serverResponse = _moderationOperation.ReportPlayer(
                    _me.PlayerID.Value,
                    _companion.PlayerID.Value,
                    reason
                );

                string feedbackMessage = StatusToMessageMapper.GetModerationMessage(serverResponse.StatusCode);

                MessageBox.Show(feedbackMessage, Lang.globalInformationTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void HandleMatchWon(object sender, string finalTime)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StopTimer();
                StopChronometer();

                GameOverTitle = Lang.matchVictory;
                GameOverMessage = string.Format(Lang.matchVictoryMessage);
                OverlayColor = "#CC228B22";
                ShowAssassinImage = false;

                IsGameOverVisible = true;
            });
        }

        private void HandleMatchTimeout(object sender, string finalTime)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StopTimer();
                StopChronometer();

                GameOverTitle = Lang.matchDefeat;
                GameOverMessage = Lang.matchDefeatTimeoutMessage;
                OverlayColor = "#CC8B0000";
                ShowAssassinImage = false;

                IsGameOverVisible = true;
            });
        }

        public void ShowGameOverScreen()
        {
            IsGameOverVisible = true;
        }
    }
}