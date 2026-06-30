using UnityEngine;
using Game2048.Board;
using Game2048.Persistence;

namespace Game2048.Core
{
    public sealed class GameController : MonoBehaviour
    {
        private GameManager gm;
        private BoardModel model;
        private GameData data;

        private int[] undoCells;
        private int undoScore;
        private int undoHighest;
        private bool undoReached;
        private bool canUndo;

        private bool pendingWin;
        private bool continueAfterWin;
        private bool subscribed;

        public void Begin(GameData gameData)
        {
            gm = GameManager.Instance;
            data = gameData;

            int size = data.boardSize > 0 ? data.boardSize : 4;
            model = new BoardModel(size, unchecked(System.Environment.TickCount));

            gm.BoardView.Initialize(size, gm.Theme, gm.Settings);

            if (!subscribed)
            {
                gm.Theme.Changed += gm.BoardView.ApplyTheme;
                gm.Swipe.Swiped += OnSwipe;
                subscribed = true;
            }

            continueAfterWin = data.reachedTarget;
            canUndo = false;
            pendingWin = false;

            bool hasValidSave = data.hasActiveGame && data.cells != null && data.cells.Length == size * size;
            if (hasValidSave)
            {
                model.LoadValues(data.cells, data.score, data.highestValue, data.reachedTarget);
                gm.Score.Initialize(data.score, data.bestScore);
                gm.BoardView.BuildInstant(model.Tiles());
            }
            else
            {
                NewGame();
            }
        }

        public void NewGame()
        {
            model.StartNewGame();
            gm.Stats.RegisterGameStarted();
            gm.Score.Initialize(0, gm.Score.Best);

            pendingWin = false;
            continueAfterWin = false;
            canUndo = false;

            gm.GameOverPanel.SetVisible(false);
            gm.WinPanel.SetVisible(false);

            gm.BoardView.BuildInstant(model.Tiles());
            WriteData();
            gm.Persist();
        }

        public void Undo()
        {
            if (!canUndo || gm.BoardView.IsAnimating || undoCells == null)
            {
                return;
            }

            model.LoadValues(undoCells, undoScore, undoHighest, undoReached);
            gm.Score.SetCurrent(undoScore);
            canUndo = false;
            pendingWin = false;

            gm.GameOverPanel.SetVisible(false);
            gm.BoardView.BuildInstant(model.Tiles());
            WriteData();
            gm.Persist();
        }

        public void ContinueAfterWin()
        {
            continueAfterWin = true;
            gm.WinPanel.SetVisible(false);
        }

        public void FlushSave()
        {
            if (model == null)
            {
                return;
            }
            WriteData();
            gm.Persist();
        }

        private void OnSwipe(Direction direction)
        {
            if (gm.BoardView.IsAnimating)
            {
                return;
            }

            int[] preCells = model.ToValues();
            int preScore = model.Score;
            int preHighest = model.HighestValue;
            bool preReached = model.HasReachedTarget;

            MoveResult result = model.Move(direction);
            if (!result.Moved)
            {
                return;
            }

            undoCells = preCells;
            undoScore = preScore;
            undoHighest = preHighest;
            undoReached = preReached;
            canUndo = true;

            gm.Score.SetCurrent(model.Score);
            gm.Score.ReportGain(result.GainedScore);
            gm.Stats.RegisterMove(result.GainedScore, model.HighestValue);

            PlayMoveAudio(result);

            pendingWin = result.ReachedTarget && !continueAfterWin;

            WriteData();
            gm.Persist();

            gm.BoardView.ApplyMove(result, OnAnimationComplete);
        }

        private void OnAnimationComplete()
        {
            if (pendingWin)
            {
                pendingWin = false;
                continueAfterWin = true;
                gm.Stats.RegisterWin();
                WriteData();
                gm.Persist();
                gm.Audio.PlayWin();
                gm.WinPanel.Show();
                return;
            }

            if (!model.CanMove())
            {
                data.hasActiveGame = false;
                gm.Persist();
                gm.Audio.PlayLose();
                gm.GameOverPanel.Show(model.Score);
            }
        }

        private void PlayMoveAudio(MoveResult result)
        {
            if (result.Merges.Count > 0)
            {
                int maxMerge = 0;
                foreach (var merge in result.Merges)
                {
                    if (merge.NewValue > maxMerge)
                    {
                        maxMerge = merge.NewValue;
                    }
                }
                gm.Audio.PlayMerge(maxMerge);
                TriggerHaptics();
            }
            else
            {
                gm.Audio.PlayMove();
            }
        }

        private void TriggerHaptics()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (gm.Settings.HapticsEnabled)
            {
                Handheld.Vibrate();
            }
#endif
        }

        private void WriteData()
        {
            data.boardSize = model.Size;
            data.cells = model.ToValues();
            data.score = model.Score;
            data.bestScore = gm.Score.Best;
            data.highestValue = model.HighestValue;
            data.reachedTarget = model.HasReachedTarget;
            data.hasActiveGame = true;
        }
    }
}
