using System.Collections.Generic;

namespace Game2048.Board
{
    public struct SlideStep
    {
        public int Id;
        public int Row;
        public int Col;
    }

    public struct MergeStep
    {
        public int SurvivingId;
        public int AbsorbedId;
        public int Row;
        public int Col;
        public int NewValue;
    }

    public struct SpawnStep
    {
        public int Id;
        public int Value;
        public int Row;
        public int Col;
    }

    public sealed class MoveResult
    {
        public bool Moved;
        public int GainedScore;
        public bool ReachedTarget;
        public readonly List<SlideStep> Slides = new List<SlideStep>();
        public readonly List<MergeStep> Merges = new List<MergeStep>();
        public bool HasSpawn;
        public SpawnStep Spawn;
    }
}
