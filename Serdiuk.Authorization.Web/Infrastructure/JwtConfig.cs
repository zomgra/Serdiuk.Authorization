namespace Serdiuk.Authorization.Web.Infrastructure
{
    public class JwtConfig
    {
        public static string SecretKey { get; set; } = "JwtRandomSecretKey"; // for fasted develop
    }
}
