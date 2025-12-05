namespace Commun.Helpers
{
    public class JwtSettings
    {
        public const string Jwt_Issuer = "JWTAuthenticationServer";
        public const string Jwt_Audience = "JWTServicePostmanClient";
        public const string Jwt_Subject = "JWTServiceAccessToken";
        public const string Jwt_Expire = "1440";
        public const string Jwt_SecretPassword = "txMlrFxTTqDc6uEp29rQ07n598Tv3ve3fQl5ZnRmX8P8fHtIyRSzSK2tA2tY";
        public const String Jwt_AuthCookieName = "AccessToken";
    }
}
