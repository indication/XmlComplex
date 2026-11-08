using System;
using System.Linq;
using System.Xml;

namespace XmlComplex
{
    partial class Program
    {
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        /// <returns>return code</returns>
        [STAThread]
        static int Main(string[] args)
        {
            try
            {
                var commandArguments = CommandArguments.Parse(args);
                SaveCombinedXml(commandArguments);
                return CommandArgumentSuccessException.ExitCodeValue;
            }
            catch (CommandArgumentException ex)
            {
                if (!string.IsNullOrEmpty(ex.Message))
                    Console.WriteLine(ex.Message);

                return ex.ExitCode;
            }
        }

        private static void SaveCombinedXml(CommandArguments commandArguments)
        {
            var baseDoc = XmlComplexer.Combine(commandArguments.InputFiles.First(), commandArguments.InputFiles.Skip(1).ToArray());
            if (!commandArguments.HasEncodingOption)
            {
                baseDoc.Save(commandArguments.OutputPath);
                return;
            }

            var settings = new XmlWriterSettings
            {
                Encoding = commandArguments.Encoding,
                Indent = commandArguments.Indent,
                IndentChars = commandArguments.IndentChars,
                NewLineChars = commandArguments.NewLineChars
            };
            using (var stream = XmlWriter.Create(commandArguments.OutputPath, settings))
            {
                baseDoc.Save(stream);
            }
        }

    }
}
