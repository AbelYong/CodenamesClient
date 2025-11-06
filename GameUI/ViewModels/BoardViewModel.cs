using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesClient.GameUI.ViewModels
{
    public class BoardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public const int MAX_AGENTS = 15;
        public const int MAX_ASSASSINS = 3;
        public const int MAX_BYSTANDERS = 7;
        private int _turnTimer;
        private int _timerTokens;
        private int _bystanderTokens;
        private readonly int[,] _agentsMatrix = new int[5, 5];
        private readonly Random random = new Random();
        
        public List<int> AgentNumbers { get; set; }
        public List<int> BystanderNumbers { get; set; }

        public BoardViewModel()
        {
            GenerateMockBoard();
            InitializeAgents();
            InitializeBystanders();
        }

        public int[,] AgentsMatrix { get; set; }

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

        public string TimerTokens
        {
            get
            {
                return string.Format("x{0}", _timerTokens);
            }
            set
            {
                _timerTokens = int.Parse(value);
                OnPropertyChanged(nameof(TimerTokens));
            }
        }

        public string BystanderTokens
        {
            get
            {
                return string.Format("x{0}", _bystanderTokens);
            }
            set
            {
                _bystanderTokens = int.Parse(value);
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

        private void SetAssassins(List<int> positions, int numberOfColumns)
        {
            const int ASSASSIN_CODE = 2;
            for (int i = 0; i < MAX_ASSASSINS; i++)
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
            int startIndex = MAX_ASSASSINS;
            int endIndex = startIndex + MAX_BYSTANDERS;
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
            for (int i = 0; i < MAX_AGENTS; i++)
            {
                AgentNumbers.Add(i);
            }
        }

        private void InitializeBystanders()
        {
            BystanderNumbers = new List<int>();
            for (int i = 0; i < MAX_BYSTANDERS; i++)
            {
                BystanderNumbers.Add(i);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
