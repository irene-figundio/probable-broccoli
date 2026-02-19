using BCrypt.Net;

namespace WorkBotAI.API.Services
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword)) return false;
            Console.WriteLine($"HASH:[{hashedPassword}] LEN:{hashedPassword?.Length}");

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch (Exception)
            {
                // In case the password in the database is not a valid BCrypt hash (e.g. legacy plain text)
                // we return false to prevent crashing.
                // For hardening, we should not allow plain text login.
                return false;
            }
        }
    }
}
