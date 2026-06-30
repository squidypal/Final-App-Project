using System;
using System.Collections.Generic;

namespace Game2048.Board
{
    public sealed class BoardModel
    {
        public const int Target = 2048;

        public int Size { get; }
        public int Score { get; private set; }
        public int HighestValue { get; private set; }
        public bool HasReachedTarget { get; private set; }

        private readonly Tile[,] grid;
        private readonly Random rng;
        private int nextId = 1;

        public BoardModel(int size, int seed)
        {
            Size = size;
            grid = new Tile[size, size];
            rng = new Random(seed);
        }

        public IEnumerable<Tile> Tiles()
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (grid[r, c] != null)
                    {
                        yield return grid[r, c];
                    }
                }
            }
        }

        public void StartNewGame()
        {
            Array.Clear(grid, 0, grid.Length);
            Score = 0;
            HighestValue = 0;
            HasReachedTarget = false;
            nextId = 1;
            SpawnRandomTile();
            SpawnRandomTile();
        }

        public MoveResult Move(Direction direction)
        {
            var result = new MoveResult();
            bool moved = false;

            for (int line = 0; line < Size; line++)
            {
                var coords = BuildLine(direction, line);
                var seq = new List<Tile>(Size);
                foreach (var coord in coords)
                {
                    var tile = grid[coord.r, coord.c];
                    if (tile != null)
                    {
                        seq.Add(tile);
                    }
                }

                var placements = new List<Placement>(seq.Count);
                int write = 0;
                int i = 0;
                while (i < seq.Count)
                {
                    if (i + 1 < seq.Count && seq[i].Value == seq[i + 1].Value)
                    {
                        placements.Add(new Placement { Tile = seq[i], Index = write, Merged = true, Absorbed = seq[i + 1] });
                        i += 2;
                    }
                    else
                    {
                        placements.Add(new Placement { Tile = seq[i], Index = write, Merged = false, Absorbed = null });
                        i += 1;
                    }
                    write++;
                }

                foreach (var coord in coords)
                {
                    grid[coord.r, coord.c] = null;
                }

                foreach (var placement in placements)
                {
                    var dest = coords[placement.Index];
                    var tile = placement.Tile;
                    bool positionChanged = tile.Row != dest.r || tile.Col != dest.c;

                    if (placement.Merged)
                    {
                        int newValue = tile.Value * 2;
                        tile.Value = newValue;
                        tile.Row = dest.r;
                        tile.Col = dest.c;
                        grid[dest.r, dest.c] = tile;

                        Score += newValue;
                        result.GainedScore += newValue;
                        moved = true;

                        if (newValue > HighestValue)
                        {
                            HighestValue = newValue;
                        }
                        if (newValue >= Target && !HasReachedTarget)
                        {
                            HasReachedTarget = true;
                            result.ReachedTarget = true;
                        }

                        result.Merges.Add(new MergeStep
                        {
                            SurvivingId = tile.Id,
                            AbsorbedId = placement.Absorbed.Id,
                            Row = dest.r,
                            Col = dest.c,
                            NewValue = newValue
                        });
                    }
                    else
                    {
                        tile.Row = dest.r;
                        tile.Col = dest.c;
                        grid[dest.r, dest.c] = tile;

                        if (positionChanged)
                        {
                            moved = true;
                            result.Slides.Add(new SlideStep { Id = tile.Id, Row = dest.r, Col = dest.c });
                        }
                    }
                }
            }

            result.Moved = moved;

            if (moved)
            {
                var spawned = SpawnRandomTile();
                if (spawned != null)
                {
                    result.HasSpawn = true;
                    result.Spawn = new SpawnStep
                    {
                        Id = spawned.Id,
                        Value = spawned.Value,
                        Row = spawned.Row,
                        Col = spawned.Col
                    };
                }
            }

            return result;
        }

        public bool CanMove()
        {
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (grid[r, c] == null)
                    {
                        return true;
                    }
                    int value = grid[r, c].Value;
                    if (c + 1 < Size && grid[r, c + 1] != null && grid[r, c + 1].Value == value)
                    {
                        return true;
                    }
                    if (r + 1 < Size && grid[r + 1, c] != null && grid[r + 1, c].Value == value)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public int[] ToValues()
        {
            var values = new int[Size * Size];
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    values[r * Size + c] = grid[r, c]?.Value ?? 0;
                }
            }
            return values;
        }

        public void LoadValues(int[] values, int score, int highestValue, bool hasReachedTarget)
        {
            Array.Clear(grid, 0, grid.Length);
            nextId = 1;
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    int value = values[r * Size + c];
                    if (value > 0)
                    {
                        grid[r, c] = new Tile(nextId++, value, r, c);
                    }
                }
            }
            Score = score;
            HighestValue = highestValue;
            HasReachedTarget = hasReachedTarget;
        }

        private Tile SpawnRandomTile()
        {
            var empties = new List<(int r, int c)>();
            for (int r = 0; r < Size; r++)
            {
                for (int c = 0; c < Size; c++)
                {
                    if (grid[r, c] == null)
                    {
                        empties.Add((r, c));
                    }
                }
            }
            if (empties.Count == 0)
            {
                return null;
            }

            var cell = empties[rng.Next(empties.Count)];
            int value = rng.NextDouble() < 0.9 ? 2 : 4;
            var tile = new Tile(nextId++, value, cell.r, cell.c);
            grid[cell.r, cell.c] = tile;
            if (value > HighestValue)
            {
                HighestValue = value;
            }
            return tile;
        }

        private List<(int r, int c)> BuildLine(Direction direction, int line)
        {
            var coords = new List<(int r, int c)>(Size);
            for (int k = 0; k < Size; k++)
            {
                switch (direction)
                {
                    case Direction.Left:
                        coords.Add((line, k));
                        break;
                    case Direction.Right:
                        coords.Add((line, Size - 1 - k));
                        break;
                    case Direction.Up:
                        coords.Add((k, line));
                        break;
                    case Direction.Down:
                        coords.Add((Size - 1 - k, line));
                        break;
                }
            }
            return coords;
        }

        private struct Placement
        {
            public Tile Tile;
            public int Index;
            public bool Merged;
            public Tile Absorbed;
        }
    }
}
