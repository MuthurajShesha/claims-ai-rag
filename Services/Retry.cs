namespace ClaimsAi.Api.Services
{
    public class Retry
    {
        public static async Task<T?> WithRetryAsync<T>(Func<Task<T?>> action, int attempts = 3)
        {
            for (var i = 1; i <= attempts; i++)
            {
                try
                {
                    var result = await action();
                    if (result is not null) return result;
                }
                catch (HttpRequestException) when (i < attempts) { }
                if (i < attempts) await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
            }
            return default;
        }
    }
}
