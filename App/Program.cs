using System;
using System.Collections.Generic;
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
            var optionHelper = CreateOptionHelper();
            var items = optionHelper.GetNoOption(args).ToList();
            var options = optionHelper.GetOptions(args);

            if (options.ContainsKey("version"))
            {
                ShowVersion();
                return 0;
            }

            if (!TryGetNewLine(options, out var newline))
                return -1;

            if (!TryGetEncoding(options, out var encoding, out var encodingExitCode))
                return encodingExitCode;

            if (items.Count < 1 || options.ContainsKey("help") || !options.ContainsKey("output"))
            {
                ShowHelp(optionHelper);
                return options.ContainsKey("help") ? 0 : -1;
            }

            if (!HasAllInputFiles(items))
                return -2;

            SaveCombinedXml(items, options, newline, encoding);
            return 0;
        }

        private static OptionHelper CreateOptionHelper()
        {
            return new OptionHelper()
                .AddOption("o", "output", "FILE", Resources.HelpOptionsOutput)
                .AddOption("n", "newline", "LINECODE", Resources.HelpOptionsNewLine, string.Format(Resources.HelpOptionsNewLineEx, string.Join("|", Enum.GetNames(typeof(NewLineKind)))))
                .AddOption("es", "encodings", "", Resources.HelpOptionsEncodings)
                .AddOption("e", "encoding", "ENCODNG", Resources.HelpOptionsEncoding, Resources.HelpEncodingExt)
                .AddOption("i-", "no-indent", "", Resources.HelpOptionsNoIndent)
                .AddOption("i", "indent", "INDENTCHAR", Resources.HelpOptionsIndent)
                .AddOption("v", "version", "", Resources.HelpOptionsVersion)
                .AddOption("h", "help", "", Resources.HelpOptionsHelp);
        }

        private static bool TryGetNewLine(Dictionary<string, string> options, out string newLine)
        {
            newLine = Environment.NewLine;
            if (!options.ContainsKey("newline"))
                return true;

            newLine = NewLineResolver.Resolve(options["newline"]);
            if (newLine != null)
                return true;

            Console.WriteLine(Resources.ErrorInvalidLineCode, options["newline"]);
            return false;
        }

        private static bool TryGetEncoding(Dictionary<string, string> options, out Encoding encoding, out int exitCode)
        {
            encoding = null;
            exitCode = 0;
            if (!options.ContainsKey("encoding") && !options.ContainsKey("encodings"))
                return true;

            if (options.ContainsKey("encoding"))
            {
                var encodingName = options["encoding"];
                try
                {
                    encoding = EncodingResolver.Resolve(encodingName);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(Resources.ErrorInvalidEncoding, encodingName, ex.Message);
                    Console.WriteLine();
                }
            }

            if (encoding != null && !options.ContainsKey("encodings"))
                return true;

            ShowEncodings();
            exitCode = options.ContainsKey("encodings") ? 0 : -1;
            return false;
        }

        private static bool HasAllInputFiles(IEnumerable<string> items)
        {
            var hasAll = true;
            foreach (var item in items.Where(w => !File.Exists(w)))
            {
                Console.WriteLine(string.Format(Resources.ErrorFileNotFound, item));
                hasAll = false;
            }
            return hasAll;
        }

        private static void SaveCombinedXml(IList<string> items, Dictionary<string, string> options, string newline, Encoding encoding)
        {
            var baseDoc = XmlComplexer.Combine(items.First(), items.Skip(1).ToArray());
            if (!options.ContainsKey("encoding"))
            {
                baseDoc.Save(options["output"]);
                return;
            }

            var settings = new XmlWriterSettings
            {
                Encoding = encoding,
                Indent = !options.ContainsKey("no-indent"),
                IndentChars = options.ContainsKey("indent") ? options["indent"] : "  ",
                NewLineChars = newline
            };
            using (var stream = XmlWriter.Create(options["output"], settings))
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
        /// <summary>
        /// Show help
        /// </summary>
        /// <param name="helper"></param>
        private static void ShowHelp(OptionHelper helper)
        {
            var asm = Assembly.GetExecutingAssembly();
            var copyright = Attribute.GetCustomAttribute(asm, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
            var title = Attribute.GetCustomAttribute(asm, typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
            Console.WriteLine("{0} ({1})", title?.Title, copyright?.Copyright);
            var filename = Path.GetFileName(asm.Location);
            Console.WriteLine(Resources.HelpMessage, filename, Resources.HelpEncodingExt, string.Join("|", Enum.GetNames(typeof(NewLineKind))));
            foreach (var line in helper.GetHelp())
                Console.WriteLine(line);
        }
    }
}
