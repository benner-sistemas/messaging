using System;
using System.Collections.Generic;
using System.Text;
using Benner.Messaging.CLI.Verbs;
using CommandLine;
using CommandLine.Text;

namespace Benner.Messaging.CLI
{
    public class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(h => h.HelpWriter = null);
            var result = parser.ParseArguments<ConsumerVerb, ControllersVerb, OidcVerb, LogsSettingsVerb, ActiveVerb, AmazonVerb, AzureVerb, RabbitVerb, BrokerBase>(args)
                .WithParsed<IVerb>(a =>
                {
                    if (a.HasNoInformedParams())
                    {
                        Console.WriteLine("Nenhum parâmetro foi informado. Utilize '--help' para visualizar o menu de opções.");
                        return;
                    }
                    a.Configure();
                    if (!(a is BrokerBase) || (a is BrokerBase broker && broker.Success))
                        Console.WriteLine("Configurações aplicadas com sucesso.");
                });
            result.WithNotParsed(errors =>
                {
                    var helpText = HelpText.AutoBuild(result, h =>
                        {
                            h.AddEnumValuesToHelpText = true;
                            h.AdditionalNewLineAfterOption = false;
                            h.AutoVersion = false;
                            return h;
                        }, 170);
                    Console.WriteLine(helpText);
                });
        }
    }
}
