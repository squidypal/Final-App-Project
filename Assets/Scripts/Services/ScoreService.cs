using System;

namespace Game2048.Services
{
    public sealed class ScoreService
    {
        public int Current { get; private set; }
        public int Best { get; private set; }

        public event Action<int> CurrentChanged;
        public event Action<int> BestChanged;
        public event Action<int> Gained;

        public void Initialize(int current, int best)
        {
            Current = current;
            Best = best;
            CurrentChanged?.Invoke(Current);
            BestChanged?.Invoke(Best);
        }

        public void SetCurrent(int value)
        {
            Current = value;
            CurrentChanged?.Invoke(Current);
            if (Current > Best)
            {
                Best = Current;
                BestChanged?.Invoke(Best);
            }
        }

        public void ReportGain(int amount)
        {
            if (amount > 0)
            {
                Gained?.Invoke(amount);
            }
        }
    }
}
