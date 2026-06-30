using System;

namespace Game2048.Persistence
{
    [Serializable]
    public sealed class StatsData
    {
        public int gamesPlayed;
        public int gamesWon;
        public int highestTile;
        public long totalScore;
        public int totalMoves;
    }

    [Serializable]
    public sealed class GameData
    {
        public int version = 1;
        public int boardSize = 4;
        public int[] cells = new int[0];
        public int score;
        public int bestScore;
        public int highestValue;
        public bool reachedTarget;
        public bool hasActiveGame;
        public StatsData stats = new StatsData();
    }
}
