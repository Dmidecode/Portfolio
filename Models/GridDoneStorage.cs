namespace Portfolio.Models
{
    public class GridsDoneStorage
    {
        public List<GridDoneStorage> Grids { get; set; }
        public GridsDoneStorage() {
            Grids = new List<GridDoneStorage>();
        }
    }

    public class GridDoneStorage
    {
        public int Index { get; set; }
        public int Seconds { get; set; }
    }
}
