namespace Portfolio.Models.Aws
{
    public class LeaderboardGet
    {
        public LeaderboardGet() 
        {
            this.Data = new Dictionary<int, List<LeaderboardData>>();
        }

        public Dictionary<int, List<LeaderboardData>> Data { get; set; }
    }
}
