namespace ClaimsAi.Api.Services
{
    public class PromptStore
    {
        public static string Load(string name) =>
       File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Prompts", name));  //Path.Combine(AppContext.BaseDirectory, "Prompts", name)
    }
}
