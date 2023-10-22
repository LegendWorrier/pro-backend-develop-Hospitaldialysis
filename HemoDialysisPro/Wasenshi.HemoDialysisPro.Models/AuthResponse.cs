namespace Wasenshi.HemoDialysisPro.Models
{
    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public double Expire { get; set; }
    }
}
