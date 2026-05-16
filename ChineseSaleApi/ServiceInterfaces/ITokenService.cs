namespace ChineseSaleApi.ServiceInterfaces
{
    public interface ITokenService
    {
        string GenerateToken(int userId, string email, string firstName, string lastName, bool? isAdmin);
    }
}