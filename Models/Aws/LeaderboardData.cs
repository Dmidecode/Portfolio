namespace Portfolio.Models.Aws
{
    public class LeaderboardData
    {
        public LeaderboardData() { Name = string.Empty; }

        public string Name { get; set; }

        public int Seconds { get; set; }
    }
}
