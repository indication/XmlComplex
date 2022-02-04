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
            var _items = OptionHelper.GetNoOption(args).ToList();
            //Console.WriteLine(Resources.MessageArguments, string.Join(",", _items));
            var optionParams = new[] {
                //from, to
                new [] { "o", "output", "FILE", Resources.HelpOptionsOutput },
                new [] { "n", "newline", "LINECODE", Resources.HelpOptionsNewLine, string.Format(Resources.HelpOptionsNewLineEx, string.Join("|", Enum.GetNames(typeof(NewLineKind))))},
                new [] { "es", "encodings", "", Resources.HelpOptionsEncodings },
                new [] { "e", "encoding", "ENCODNG", Resources.HelpOptionsEncoding, Resources.HelpEncodingExt },
                new [] { "i-", "no-indent", "", Resources.HelpOptionsNoIndent },
                new [] { "i", "indent", "INDENTCHAR", Resources.HelpOptionsIndent },
                new [] { "v", "version", "", Resources.HelpOptionsVersion },
                new [] { "h", "help", "", Resources.HelpOptionsHelp },
            };
            var options = OptionHelper.GetOptions(args, optionParams);

            var newline = Environment.NewLine;
            if (options.ContainsKey("newline"))
            {
                newline = GetNewLineCode(options["newline"]);
                if (newline == null)
                {
                    Console.WriteLine(Resources.ErrorInvalidLineCode, options["newline"]);
                    return -1;
                }
            }
            if (options.ContainsKey("version"))
            {
                ShowVersion();
                return 0;
            }
            Encoding encode = null;
            if (options.ContainsKey("encoding") || options.ContainsKey("encodings"))
            {
                if (options.ContainsKey("encoding"))
                {
                    var str = options["encoding"];
                    try
                    {
                        encode = GetEncoding(str);
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine(Resources.ErrorInvalidEncoding, str, ex.Message);
                        Console.WriteLine();
                    }
                }
                if (encode == null || options.ContainsKey("encodings"))
                {
                    ShowEncodings();
                    return options.ContainsKey("encodings") ? 0 : -1;
                }
            }
            if (_items.Count < 1 || options.ContainsKey("help") || !options.ContainsKey("output"))
            {
                ShowHelp(optionParams);
                return options.ContainsKey("help") ? 0 : -1;
            }

            bool findAll = true;
            foreach (var _item in _items.Where(w => !File.Exists(w)))
            {
                Console.WriteLine(string.Format(Resources.ErrorFileNotFound, _item));
                findAll = false;
            }
            if (!findAll)
            {
                return -2;
            }

            var _basedoc = XmlComplexer.Combine(_items.First(), _items.Skip(1).ToArray());
            if (options.ContainsKey("encoding"))
            {
                var settings = new XmlWriterSettings();
                settings.Encoding = encode;
                settings.Indent = !options.ContainsKey("no-indent");
                settings.IndentChars = options.ContainsKey("indent") ? options["indent"] : "  ";
                if (newline != null)
                    settings.NewLineChars = newline;
                using (var stream = XmlWriter.Create(options["output"], settings))
                {
                    _basedoc.Save(stream);
                }
            }
            else
                _basedoc.Save(options["output"]);
            return 0;
        }
        /// <summary>
        /// Get new line code
        /// </summary>
        /// <param name="newLine">name for new line</param>
        /// <returns>string from names</returns>
        private static string GetNewLineCode(string newLine)
        {
            NewLineKind kind;
            if (Enum.TryParse(newLine, true, out kind))
            {
                switch (kind)
                {
                    case NewLineKind.CR:
                        return "\r";
                    case NewLineKind.LF:
                        return "\n";
                    case NewLineKind.CRLF:
                        return "\r\n";
                }
            }
            return null;
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
        /// Get encoding from param
        /// </summary>
        /// <param name="str">encoding name</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">encoding is not found</exception>
        private static Encoding GetEncoding(string str)
        {
            var hasBe = str.ToLower().Contains("be");
            var hasBom = str.ToLower().Contains("bom") && !str.ToLower().Contains("nobom");
            if (str.StartsWith("utf-8", StringComparison.CurrentCultureIgnoreCase) || str.StartsWith("utf8", StringComparison.CurrentCultureIgnoreCase))
            {
                return new UTF8Encoding(hasBom);
            }
            else if (str.StartsWith("utf-32", StringComparison.CurrentCultureIgnoreCase) || str.StartsWith("utf32", StringComparison.CurrentCultureIgnoreCase))
            {
                return new UTF32Encoding(hasBe, hasBom);
            }
            else if (str.StartsWith("utf-16", StringComparison.CurrentCultureIgnoreCase) || str.StartsWith("utf16", StringComparison.CurrentCultureIgnoreCase))
            {
                return new UnicodeEncoding(hasBe, hasBom);
            }

            return Encoding.GetEncoding(str);
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
        /// <param name="optionParams"></param>
        private static void ShowHelp(string[][] optionParams)
        {
            var asm = Assembly.GetExecutingAssembly();
            var copyright = Attribute.GetCustomAttribute(asm, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
            var title = Attribute.GetCustomAttribute(asm, typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
            Console.WriteLine("{0} ({1})", title?.Title, copyright?.Copyright);
            var filename = Path.GetFileName(asm.Location);
            Console.WriteLine(Resources.HelpMessage, filename, Resources.HelpEncodingExt, string.Join("|", Enum.GetNames(typeof(NewLineKind))));
            foreach (var line in OptionHelper.GetHelp(optionParams))
                Console.WriteLine(line);
        }
    }
}
