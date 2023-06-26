namespace Serdiuk.Authorization.Web.Models.DTO
{
    public class RefreshRequestDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
