using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodenamesGame.Domain.POCO.Match
{
    public class MatchDM
    {
        public Guid MatchID { get; set; }
        public PlayerDM Requester { get; set; }
        public PlayerDM Companion { get; set; }
        public MatchRulesDM Rules { get; set; }
        public int[,] Board { get; set; }
        public int[,] Keycard { get; set; }
        public List<int> SelectedWords { get; set; }

        public static MatchDM AssembleMatch(MatchmakingService.Match incomingMatch, Guid myID)
        {
            MatchDM match = new MatchDM();
            match.MatchID = incomingMatch.MatchID;
            match.Requester = PlayerDM.AssemblePlayer(incomingMatch.Requester);
            match.Companion = PlayerDM.AssemblePlayer(incomingMatch.Companion);
            match.Rules = MatchRulesDM.AssembleRules(incomingMatch.Rules);
            match.SelectedWords = DeserializeIncomingWords(incomingMatch.SelectedWords);
            if (match.Requester.PlayerID == myID)
            {
                match.Board = ConvertToBidimensional(incomingMatch.BoardPlayerOne);
                match.Keycard = ConvertToBidimensional(incomingMatch.BoardPlayerTwo);
            }
            else
            {
                match.Board = ConvertToBidimensional(incomingMatch.BoardPlayerTwo);
                match.Keycard = ConvertToBidimensional(incomingMatch.BoardPlayerOne);
            }
            return match;
        }

        private static List<int> DeserializeIncomingWords(int[] incomingWords)
        {
            List<int> selectedWords = new List<int>();
            foreach (int word in incomingWords)
            {
                selectedWords.Add(word);
            }
            return selectedWords;
        }

        private static int[,] ConvertToBidimensional(int[][] jaggedArray)
        {
            if (jaggedArray == null || jaggedArray.Length == 0)
            {
                return new int[0, 0];
            }

            int rows = jaggedArray.Length;
            int cols = jaggedArray[0].Length;

            int[,] multiArray = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    multiArray[i, j] = jaggedArray[i][j];
                }
            }
            return multiArray;
        }
    }
}
