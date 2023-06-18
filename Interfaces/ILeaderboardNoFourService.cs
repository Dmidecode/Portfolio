namespace Portfolio.Interfaces
{
    public interface ILeaderboardNoFourService
    {
        Task<TValue?> GetFromJsonAsync<TValue>(string requestURI);

        Task<HttpResponseMessage> PostAsJsonAsync<TValue>(string requestURI, TValue value);
    }
}
