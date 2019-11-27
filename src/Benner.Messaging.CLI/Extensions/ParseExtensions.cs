using Benner.Messaging.CLI.Attributes;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Benner.Messaging.CLI.Extensions
{
    public static class ParseExtensions
    {
        public static ParserResult<object> ParseVerbs(this Parser parser, IEnumerable<string> args, params Type[] types)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            var argsArray = args as string[] ?? args.ToArray();
            if (argsArray.Length == 0 || argsArray[0].StartsWith("-"))
                return parser.ParseArguments(argsArray, types);

            var verb = argsArray[0];
            Type finalType = null;
            foreach (Type type in types)
            {
                var verbAttribute = type.GetCustomAttribute<VerbAttribute>();
                if (verbAttribute == null || verbAttribute.Name != verb)
                    continue;

                finalType = type;
                var subVerbsAttribute = type.GetCustomAttribute<SubVerbsAttribute>();
                if (subVerbsAttribute != null)
                    return ParseVerbs(parser, argsArray.Skip(1).ToArray(), subVerbsAttribute.Types);

                break;
            }

            return parser.ParseArguments(argsArray, finalType);
        }
    }
}
