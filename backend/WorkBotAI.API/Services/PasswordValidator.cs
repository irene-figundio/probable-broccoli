using System.Text.RegularExpressions;

namespace WorkBotAI.API.Services
{
    public static class PasswordValidator
    {
        public static (bool IsValid, string? ErrorMessage) Validate(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return (false, "La password è obbligatoria.");
            }

            if (password.Length < 6)
            {
                return (false, "La password deve contenere almeno 6 caratteri.");
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return (false, "La password deve contenere almeno una lettera maiuscola.");
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return (false, "La password deve contenere almeno un numero.");
            }

            if (!Regex.IsMatch(password, @"[\W_]"))
            {
                return (false, "La password deve contenere almeno un carattere speciale.");
            }

            return (true, null);
        }
    }
}
