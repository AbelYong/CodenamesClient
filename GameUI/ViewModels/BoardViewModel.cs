using CodenamesGame.Domain.POCO;
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
using System.Windows.Threading;

namespace CodenamesClient.GameUI.ViewModels
{
    public class BoardViewModel : INotifyPropertyChanged
    {
        public const int MAX_GLOBAL_AGENTS = 15;
        public const int MAX_GLOBAL_ASSASSINS = 3;
        public const int MAX_GLOBAL_BYSTANDERS = 7;
        public const int MAX_LOCAL_BYSTANDERS = 13;
        public event PropertyChangedEventHandler PropertyChanged;
        public string PlayerUsername { get; set; }
        private readonly Random random = new Random();
        private DispatcherTimer _timer;
        private DispatcherTimer _chronometer;
        private TimeSpan _elapsedTime;
        private const int MAX_ROWS = 5;
        private const int MAX_COLUMNS = 5;
        private readonly int[,] _agentsMatrix;
        private readonly Dictionary<int, string> _keywords = new Dictionary<int, string>();
        private readonly int[,] _keycardMatrix;
        
        private int _turnTimer;
        private int _timerTokens;
        private int _bystanderTokens;
        private int _turnLenght;

        public List<int> AgentNumbers { get; set; }

        public BoardViewModel(MatchDM match)
        {
            InitializeAgentNumbers();
            _agentsMatrix = GenerateMockBoard();
            GenerateMockWordList();
            _keycardMatrix = GenerateMockBoard();
            InitializeMatchData(match);
            InitializeChronometer();
            InitializeTimer();
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

        private void InitializeAgentNumbers()
        {
            AgentNumbers = new List<int>();
            for (int i = 0; i < MAX_GLOBAL_AGENTS; i++)
            {
                AgentNumbers.Add(i);
            }
        }

        private int[,] GenerateMockBoard()
        {
            int[,] board = new int[MAX_ROWS, MAX_COLUMNS];
            int numberOfRows = board.GetLength(0);
            int numberOfColumns = board.GetLength(1);
            int totalSpots = numberOfRows * numberOfColumns;

            Array.Clear(board, 0, board.Length);
            List<int> positions = Enumerable.Range(0, totalSpots).ToList();

            Shuffle(positions);
            SetAssassins(positions, board);
            SetBystanders(positions, board);
            return board;
        }

        private void SetAssassins(List<int> positions, int[,] board)
        {
            int numberOfColumns = board.GetLength(1);
            const int ASSASSIN_CODE = 2;
            for (int i = 0; i < MAX_GLOBAL_ASSASSINS; i++)
            {
                int flatIndex = positions[i];
                int row = flatIndex / numberOfColumns;
                int column = flatIndex % numberOfColumns;
                board[row, column] = ASSASSIN_CODE;
            }
        }

        private void SetBystanders(List<int> positions, int[,] board)
        {
            int numberOfColumns = board.GetLength(1);
            const int BYSTANDER_CODE = 1;
            int startIndex = MAX_GLOBAL_ASSASSINS;
            int endIndex = startIndex + MAX_LOCAL_BYSTANDERS;
            for (int i = startIndex; i < endIndex; i++)
            {
                int flatIndex = positions[i];
                int row = flatIndex / numberOfColumns;
                int column = flatIndex % numberOfColumns;
                board[row, column] = BYSTANDER_CODE;
            }
        }

        private void Shuffle(List<int> positions)
        {
            //Shuffle the positions using Fisher-Yates algorithm
            int n = positions.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);

                int value = positions[k];
                positions[k] = positions[n];
                positions[n] = value;
            }
        }

        private void GenerateMockWordList()
        {
            _keywords.Clear();
            const int numberOfWords = 400;
            List<int> selectedWords = Enumerable.Range(0, numberOfWords).ToList();
            Shuffle(selectedWords);

            List<string> allWords = GetCurrentCultureWords();
            int totalKeycards = MAX_GLOBAL_AGENTS + MAX_GLOBAL_BYSTANDERS + MAX_GLOBAL_ASSASSINS;
            for (int i = 0; i < totalKeycards; i++)
            {
                _keywords.Add(i, allWords[selectedWords[i]]);
            }
        }

        private List<string> GetCurrentCultureWords()
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
            _turnLenght = match.Rules.turnTimer;
            _timerTokens = match.Rules.timerTokens;
            _bystanderTokens = match.Rules.bystanderTokens;
            PlayerUsername = match.Player.Username;
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
            int turnLength = TurnTimer + seconds;
            if (turnLength >= 60)
            {
                const int MAX_TURN_LENGTH = 60;
                TurnTimer = MAX_TURN_LENGTH;
            }
            else
            {
                TurnTimer = turnLength;
            }
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
    }
}
