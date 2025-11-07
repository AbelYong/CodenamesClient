using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CodenamesClient.GameUI.ViewModels
{
    public class BoardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private readonly Random random = new Random();
        private DispatcherTimer _timer;
        private readonly int[,] _agentsMatrix = new int[5, 5];
        private readonly Dictionary<int, string> _keywords = new Dictionary<int, string>();
        public const int MAX_GLOBAL_AGENTS = 15;
        public const int MAX_GLOBAL_ASSASSINS = 3;
        public const int MAX_GLOBAL_BYSTANDERS = 7;
        public const int MAX_LOCAL_BYSTANDERS = 13;
        private int _turnTimer;
        private int _timerTokens;
        private int _bystanderTokens;
        
        public List<int> AgentNumbers { get; set; }

        public BoardViewModel()
        {
            GenerateMockBoard();
            GenerateMockWordList();
            InitializeAgents();
        }

        public int[,] AgentsMatrix { get; set; }

        public Dictionary<int, string> Keywords
        {
            get
            {
                return _keywords;
            }
        }

        public int TurnTimer
        {
            get
            { 
                return _turnTimer;
            }
            set
            {
                _turnTimer = value;
                OnPropertyChanged(nameof(TurnTimer));
            }
        }

        public int TimerTokens
        {
            get
            {
                return _timerTokens;
            }
            set
            {
                _timerTokens = value;
                OnPropertyChanged(nameof(TimerTokens));
            }
        }

        public int BystanderTokens
        {
            get
            {
                return _bystanderTokens;
            }
            set
            {
                _bystanderTokens = value;
                OnPropertyChanged(nameof(BystanderTokens));
            }
        }

        private void GenerateMockBoard()
        {
            int numberOfRows = _agentsMatrix.GetLength(0);
            int numberOfColumns = _agentsMatrix.GetLength(1);
            int totalSpots = numberOfRows * numberOfColumns;

            Array.Clear(_agentsMatrix, 0, _agentsMatrix.Length);
            List<int> positions = Enumerable.Range(0, totalSpots).ToList();

            Shuffle(positions);
            SetAssassins(positions, numberOfColumns);
            SetBystanders(positions, numberOfColumns);
            AgentsMatrix = _agentsMatrix;
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

        private void SetAssassins(List<int> positions, int numberOfColumns)
        {
            const int ASSASSIN_CODE = 2;
            for (int i = 0; i < MAX_GLOBAL_ASSASSINS; i++)
            {
                int flatIndex = positions[i];
                int row = flatIndex / numberOfColumns;
                int column = flatIndex % numberOfColumns;
                _agentsMatrix[row, column] = ASSASSIN_CODE;
            }
        }

        private void SetBystanders(List<int> positions, int numberOfColumns)
        {
            const int BYSTANDER_CODE = 1;
            int startIndex = MAX_GLOBAL_ASSASSINS;
            int endIndex = startIndex + MAX_LOCAL_BYSTANDERS;
            for (int i = startIndex; i < endIndex; i++)
            {
                int flatIndex = positions[i];
                int row = flatIndex / numberOfColumns;
                int column = flatIndex % numberOfColumns;
                _agentsMatrix[row, column] = BYSTANDER_CODE;
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

        private void InitializeAgents()
        {
            AgentNumbers = new List<int>();
            for (int i = 0; i < MAX_GLOBAL_AGENTS; i++)
            {
                AgentNumbers.Add(i);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
