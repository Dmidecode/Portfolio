namespace Portfolio.Models.Aws
{
    public class LeaderboardSaveRequest
    {
        public int Difficulty { get; set; }

        public int Level { get; set; }

        public string Name { get; set; }

        public int Seconds { get; set; }
    }
}
