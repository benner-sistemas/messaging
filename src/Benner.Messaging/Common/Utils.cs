using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/*
    REGRAS QUEUE NAME AZURE
    https://docs.microsoft.com/pt-br/rest/api/storageservices/Naming-Queues-and-Metadata?redirectedfrom=MSDN
    1. A queue name must start with a letter or number, and can only contain letters, numbers, and the dash (-) character.
    2. The first and last letters in the queue name must be alphanumeric. The dash (-) character cannot be the first or last character. Consecutive dash characters are not permitted in the queue name.
    3. All letters in a queue name must be lowercase.
    4. A queue name must be from 3 through 63 characters long.
*/

namespace Benner.Messaging
{
    internal static class Utils
    {
        private static readonly int _queueMinLength = 3;
        private static readonly int _queueMaxLength = 63;
        private static readonly RegexOptions _regexOptions = RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant;
        private static readonly Regex _queueNameRegex = new Regex("^[a-z0-9]+(-[a-z0-9]+)*$", _regexOptions);

        /// <summary>
        /// Valida o nome de uma fila de acordo com as regras do AzureQueue. 
        /// </summary>
        /// <param name="queueName">O nome a ser validado.</param>
        /// <param name="throwOnError">Se deve-se lançar exceção para um nome inválido.</param>
        /// <returns>Se é válido ou não. Exceção caso <paramref name="throwOnError"/> seja <see cref="true"/>.</returns>
        internal static bool ValidateQueueName(string queueName, bool throwOnError = false)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                return !throwOnError ? false : throw new ArgumentException("Invalid queue name. The queue name may not be null, empty, or whitespace only.");

            if (queueName.Length < _queueMinLength || queueName.Length > _queueMaxLength)
                return !throwOnError ? false : throw new ArgumentException($"Invalid queue name length. The queue name must be between {_queueMinLength} and {_queueMaxLength} characters long.");

            if (!_queueNameRegex.IsMatch(queueName))
                return !throwOnError ? false : throw new ArgumentException("Invalid queue name.");

            return true;
        }

        /// <summary>
        /// Obtém o valor do dicionário de acordo com a chave. 
        /// Caso seja required e não exista o valor no dicionário para a chave, lança um exceção.
        /// Caso não seja required retorna o valor encontrado. Se não for encontrado retorna string vazia.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key">A chave.</param>
        /// <param name="isRequired">Se é required.</param>
        /// <returns></returns>
        internal static string GetValue(this Dictionary<string, string> dictionary, string key, bool isRequired)
        {
            bool containsKey = dictionary.TryGetValue(key, out string value);
            if (isRequired)
            {
                if (containsKey)
                    return value;
                throw new ArgumentException($"The parameter '{key}' is mandatory for the broker configuration.");
            }
            else
                return value ?? "";
        }

        /// <summary>
        /// Obtém o valor do dicionário de acordo com a chave. 
        /// Caso seja required e não exista o valor no dicionário para a chave, lança um exceção.
        /// Caso não seja required e não exista o valor, retorna o valor informado como default.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key">A chave.</param>
        /// <param name="isRequired">Se é required.</param>
        /// <param name="defaultValue">O valor padrão.</param>
        /// <returns></returns>
        internal static string GetValue(this Dictionary<string, string> dictionary, string key, bool isRequired, string defaultValue)
        {
            var value = GetValue(dictionary, key, isRequired);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        internal static T DeserializeObject<T>(string content)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(content, new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    TypeNameHandling = TypeNameHandling.All
                });
            }
            catch (Exception e)
            {
                throw new InvalidCastException("Error parsing the object.", e);
            }
        }

        internal static string SerializeObject(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
                {
                    ObjectCreationHandling = ObjectCreationHandling.Replace,
                    TypeNameHandling = TypeNameHandling.All
                });
            }
            catch (Exception e)
            {
                throw new InvalidCastException("Error parsing the object.", e);
            }
        }
    }
}
