using CodenamesGame.Domain.POCO;
using CodenamesGame.Domain.POCO.Match;
using CodenamesGame.Network;
using CodenamesClient.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CodenamesClient.Properties.Langs;

namespace CodenamesClient.GameUI.ViewModels
{
    public class BoardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand ReportPlayerCommand { get; set; }
        //The MAX_GLOBAL constants are used for selecting random images from the assets by setting limits for number-indexing
        public const int MAX_GLOBAL_AGENTS = 15;
        public const int MAX_GLOBAL_ASSASSINS = 3;
        public const int MAX_GLOBAL_BYSTANDERS = 7;
        private readonly MatchDM _match;
        private string _playerSTurn;
        private string _turnInstrucions;
        private PlayerDM _me;
        private string _myUsername;
        private PlayerDM _companion;
        private string _companionUsername;
        private DispatcherTimer _timer;
        private DispatcherTimer _chronometer;
        private TimeSpan _elapsedTime;
        private readonly int[,] _agentsMatrix;
        private readonly int[,] _keycardMatrix;
        private readonly Dictionary<int, string> _keywords = new Dictionary<int, string>();
        private int _turnTimer;
        private int _timerTokens;
        private int _bystanderTokens;
        private int _turnLenght;
        private ModerationOperation _moderationOperation;
        
        public string PlayerSTurn
        {
            get => _playerSTurn;
            set
            {
                _playerSTurn = value;
                OnPropertyChanged(nameof(PlayerSTurn));
            }
        }

        public string TurnInstructions
        {
            get => _turnInstrucions;
            set
            {
                _turnInstrucions = value;
                OnPropertyChanged(nameof(TurnInstructions));
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
                OnPropertyChanged(nameof(MyUsername));
            }
        }

        public List<int> AgentNumbers { get; set; }

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
            get => _turnLenght;
        }

        public int TurnTimer
        {
            get => _turnTimer;
            set
            {
                _turnTimer = value;
                OnPropertyChanged(nameof(TurnTimer));
            }
        }

        public int TimerTokens
        {
            get => _timerTokens;
            set
            {
                _timerTokens = value;
                OnPropertyChanged(nameof(TimerTokens));
            }
        }

        public int BystanderTokens
        {
            get => _bystanderTokens;
            set
            {
                _bystanderTokens = value;
                OnPropertyChanged(nameof(BystanderTokens));
            }
        }

        public TimeSpan ElapsedTime
        {
            get => _elapsedTime;
            set
            {
                _elapsedTime = value;
                OnPropertyChanged(nameof(ElapsedTime));
            }
        }

        public BoardViewModel(MatchDM match, Guid myID)
        {
            _match = match;
            SetPlayers(myID);
            InitializeAgentNumbers();
            _agentsMatrix = match.Board;
            SetKeywords(match.SelectedWords);
            _keycardMatrix = match.Keycard;
            InitializeMatchData(match);
            InitializeChronometer();
            InitializeTimer();
            _moderationOperation = new ModerationOperation();
            SetInitialTurn();
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
                    MyUsername = _match.Requester.Username;
                    _me = _match.Companion;
                    CompanionUsername = _match.Requester.Username;
                    _companion = _match.Requester;
                }
            }
        }

        private void SetInitialTurn()
        {
            if (_me.PlayerID == _match.Requester.PlayerID)
            {
                PlayerSTurn = string.Format(Lang.matchYouAreSpymaster, _match.Companion.Username);
                TurnInstructions = string.Format(Lang.matchTypeAClue, _match.Companion.Username);
            }
            else
            {
                PlayerSTurn = string.Format(Lang.matchCompanionIsSpymaster, _match.Companion.Username);
                TurnInstructions = string.Format(Lang.matchPickKeywords, _match.Companion.Username);
            }
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
                _keywords.Add(i, allWords[wordlist[i]]);
            }
        }

        private static List<string> GetCurrentCultureWords()
        {
            ResourceManager manager = Properties.Langs.GameWords.ResourceManager;
            CultureInfo currentCulture = CultureInfo.CurrentUICulture;
            ResourceSet resourceSet = manager.GetResourceSet(currentCulture, true, true);

            if (resourceSet == null)
            {
                return new List<string>();
                //TODO: Notify world list could not be found, end match
            }
            List<string> allWords = resourceSet.OfType<DictionaryEntry>()
                .Where(entry => entry.Value is string).Select(entry => (string)entry.Value).ToList();
            return allWords;
        }

        private void InitializeMatchData(MatchDM match)
        {
            _turnLenght = match.Rules.TurnTimer;
            _timerTokens = match.Rules.TimerTokens;
            _bystanderTokens = match.Rules.BystanderTokens;
        }

        private void InitializeTimer()
        {
            _turnTimer = _turnLenght;
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
            }
        }

        public void StartTimer()
        {
            _timer.Start();
        }

        public void AddTime(int seconds)
        {
            const int MAX_TURN_LENGTH = 60;
            int turnLength = TurnTimer + seconds;
            TurnTimer = turnLength < MAX_TURN_LENGTH ? turnLength : MAX_TURN_LENGTH;
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
            ElapsedTime =TimeSpan.Zero;
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
                string.Format(Properties.Langs.Lang.confirmReportMessage, _companion.Username),
                Properties.Langs.Lang.globalWarningTitle,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                string reason = "Conducta antideportiva";

                // --- AQUÍ ESTÁ EL CAMBIO ---
                // Debes pasar TRES argumentos:
                // 1. Tu ID (_me.PlayerID.Value) <- Esto es lo que faltaba
                // 2. El ID del compañero (_companion.PlayerID.Value)
                // 3. La razón
                var serverResponse = _moderationOperation.ReportPlayer(
                    _me.PlayerID.Value,           // <--- AGREGADO: Tu ID (Reporter)
                    _companion.PlayerID.Value,    // El ID del reportado (Target)
                    reason
                );

                string feedbackMessage = StatusToMessageMapper.GetModerationMessage(serverResponse.StatusCode);

                MessageBox.Show(feedbackMessage, Properties.Langs.Lang.globalInformationTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
