using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using XmlComplex.Properties;

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
            var commandArguments = CommandArguments.Parse(args);

            if (commandArguments.ShowVersion)
            {
                ShowVersion();
                return 0;
            }

            if (!commandArguments.TryGetNewLine())
                return -1;

            if (!commandArguments.TryGetEncoding(out var encodingExitCode, out var showEncodings))
            {
                if (showEncodings)
                    ShowEncodings();
                return encodingExitCode;
            }

            if (!commandArguments.HasRequiredArguments())
            {
                commandArguments.ShowHelp();
                return commandArguments.HasHelpOption ? 0 : -1;
            }

            if (!commandArguments.HasAllInputFiles())
                return -2;

            SaveCombinedXml(commandArguments);
            return 0;
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

        /// <summary>
        /// Show encodings
        /// </summary>
        private static void ShowEncodings()
        {
            Console.WriteLine(Resources.HelpEncoding);
            foreach (var enc in Encoding.GetEncodings().OrderBy(w => w.Name))
            {
                Console.WriteLine("\t{0}\t{1}", enc.Name, enc.DisplayName);
            }
            Console.WriteLine("\t{0}", Resources.HelpEncodingExt);
        }

        /// <summary>
        /// Show version
        /// </summary>
        private static void ShowVersion()
        {
            var asm = Assembly.GetExecutingAssembly();
            var version = Attribute.GetCustomAttribute(asm, typeof(AssemblyFileVersionAttribute)) as AssemblyFileVersionAttribute;
            Console.WriteLine(Resources.MessageVersion, Path.GetFileName(asm.Location), version?.Version);
        }
    }
}
