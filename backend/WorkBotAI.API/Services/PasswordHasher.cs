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
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(password)) return false;

            // If it's a BCrypt hash, it should start with $2
            if (hashedPassword.StartsWith("$2"))
            {
                try
                {
                    // Try regular Verify first as it's more compatible with various BCrypt implementations
                    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                }
                catch
                {
                    try
                    {
                        // Fallback to EnhancedVerify if regular Verify fails (in case it was hashed with EnhancedHashPassword)
                        return BCrypt.Net.BCrypt.EnhancedVerify(password, hashedPassword);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            // Fallback for legacy plain-text passwords during migration phase.
            // This is likely why some users report "password correct but login fails".
            // For a strictly production-ready system after migration, this should be removed.
            return password == hashedPassword;
        }
    }
}
