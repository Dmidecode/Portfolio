namespace Portfolio.Models
{
    public class FileConfigNoFourInARow
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public List<GameData> Grids { get; set; } = new List<GameData>();
    }

    public class GameData
    {
        public int Index { get; set; }

        public List<int> Grid { get; set; } = new List<int>();

        public int Seconds { get; set; }

        public bool Done { get; set; }
    }
}
