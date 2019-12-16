using System;

namespace Benner.Messaging.CLI.Verbs
{
    internal class OptionValidator
    {
        internal static void ValidateOption(string parName, object value)
        {
            if (value is string valor && string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException($"O parâmetro '{parName}' deve ser informado.");

            if (value is int num && num <= 0)
                throw new ArgumentException($"O parâmetro '{parName}' deve ser maior que 0.");
        }
    }
}
