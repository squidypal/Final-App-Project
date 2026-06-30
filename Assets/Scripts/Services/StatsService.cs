using Game2048.Persistence;

namespace Game2048.Services
{
    public sealed class StatsService
    {
        private StatsData data;

        public int GamesPlayed => data.gamesPlayed;
        public int GamesWon => data.gamesWon;
        public int HighestTile => data.highestTile;
        public long TotalScore => data.totalScore;
        public int TotalMoves => data.totalMoves;

        public void Bind(StatsData statsData)
        {
            data = statsData;
        }

        public void RegisterGameStarted()
        {
            data.gamesPlayed++;
        }

        public void RegisterWin()
        {
            data.gamesWon++;
        }

        public void RegisterMove(int gainedScore, int highestValueOnBoard)
        {
            data.totalMoves++;
            data.totalScore += gainedScore;
            if (highestValueOnBoard > data.highestTile)
            {
                data.highestTile = highestValueOnBoard;
            }
        }
    }
}
