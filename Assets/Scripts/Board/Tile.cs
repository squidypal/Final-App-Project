namespace Game2048.Board
{
    public sealed class Tile
    {
        public int Id;
        public int Value;
        public int Row;
        public int Col;

        public Tile(int id, int value, int row, int col)
        {
            Id = id;
            Value = value;
            Row = row;
            Col = col;
        }
    }
}
