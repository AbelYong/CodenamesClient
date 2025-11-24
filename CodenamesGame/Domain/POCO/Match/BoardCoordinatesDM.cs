
namespace CodenamesGame.Domain.POCO.Match
{
    public class BoardCoordinatesDM
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public BoardCoordinatesDM(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public static BoardCoordinatesDM AssembleBoardCoordinates(MatchService.BoardCoordinates svCoordinates)
        {
            return new BoardCoordinatesDM(svCoordinates.Row, svCoordinates.Column);
        }

        public static MatchService.BoardCoordinates AssembleMatchSvBoardCoordinates(BoardCoordinatesDM coordinates)
        {
            MatchService.BoardCoordinates svCoordinates = new MatchService.BoardCoordinates();
            svCoordinates.Row = coordinates.Row;
            svCoordinates.Column = coordinates.Column;
            return svCoordinates;
        }
    }
}
