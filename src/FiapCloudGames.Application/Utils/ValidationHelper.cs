using System.Text.RegularExpressions;

namespace FiapCloudGames.Application.Utils
{
    public static class ValidationHelper
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static List<string> ValidateRegisterEntries(string password, string email)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add("O email é obrigatório.");
            }
            else if (!IsValidEmail(email))
            {
                errors.Add("O email fornecido não é válido.");
            }
            var passwordErrors = ValidatePassword(password);
            if (passwordErrors.Count > 0)
            {
                errors.AddRange(passwordErrors);
            }
            return errors;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return EmailRegex.IsMatch(email);
        }

        public static List<string> ValidatePassword(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("A senha é obrigatória.");
                return errors;
            }

            if (password.Length < 8)
                errors.Add("A senha deve ter no mínimo 8 caracteres.");

            if (!password.Any(char.IsLower))
                errors.Add("A senha deve conter pelo menos uma letra minúscula.");

            if (!password.Any(char.IsUpper))
                errors.Add("A senha deve conter pelo menos uma letra maiúscula.");

            if (!password.Any(char.IsDigit))
                errors.Add("A senha deve conter pelo menos um número.");

            if (!password.Any(c => "@$!%*?&#+\\-_.=".Contains(c)))
                errors.Add("A senha deve conter pelo menos um caractere especial (@$!%*?&#+\\-_.=).");

            return errors;
        }
    }
}