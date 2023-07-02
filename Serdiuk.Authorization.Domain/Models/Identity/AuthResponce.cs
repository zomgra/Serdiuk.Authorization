namespace Serdiuk.Authorization.Domain.Models.Identity
{
    public class AuthResponce
    {
        public string Token { get; set; }
        public string Refresh { get; set; }
        public bool Result { get; set; }
        public List<string> Errors { get; set; }

    }
}
