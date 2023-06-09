namespace Portfolio.Models
{
    public enum StatusCell
    {
        Empty,
        Cross,
        Circle
    }

    public enum ActionStatusCell
    {
      Nothing,
      Cross,
      Circle,
      Erease
    }

  public enum Difficulty
    {
        Novice = 1,
        Easy,
        Standard,
        Hard,
        Advanced,
        Expert,
        Laborious
    }

    public static class ExtensionsEnum
    {
        public static string GetDifficultyPath(this Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => "easy",
                Difficulty.Standard => "standard",
                Difficulty.Hard => "hard",
                Difficulty.Advanced => "advanced",
                Difficulty.Expert => "expert",
                Difficulty.Laborious => "laborious",
                _ => "novice",
            };
        }
    }
}
