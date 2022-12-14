using System.Text.RegularExpressions;
using FluentValidation;

namespace SiteWatcher.Application.Common.Validation;

public static class FluentValidationRules
{
    public static IRuleBuilderOptions<T, string> HasOnlyLetters<T>(this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder.Matches("^[A-Za-z ]*$");

    public static IRuleBuilderOptions<T, string?> IsValidRegex<T>(this IRuleBuilder<T, string?> ruleBuilder) =>
        ruleBuilder.Must(s =>
        {
            if(s == null)
                return false;

            try
            {
                Regex.Match("", s);
            }
            catch
            {
                return false;
            }

            return true;
        });
}