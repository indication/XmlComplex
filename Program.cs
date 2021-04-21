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
    class Program
    {
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        /// <returns>return code</returns>
        [STAThread]
        static int Main(string[] args)
        {
            var _items = GetNoOption(args).ToList();
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
            var options = GetOptions(args, optionParams);

            var newline = Environment.NewLine;
            do
            {
                if (options.ContainsKey("newline"))
                {
                    NewLineKind kind;
                    if (Enum.TryParse(options["newline"], true, out kind))
                    {
                        switch (kind)
                        {
                            case NewLineKind.CR:
                                newline = "\r";
                                break;
                            case NewLineKind.LF:
                                newline = "\n";
                                break;
                            case NewLineKind.CRLF:
                                newline = "\r\n";
                                break;
                        }
                        break;

                    }
                    Console.WriteLine(Resources.ErrorInvalidLineCode, options["newline"]);
                    return -1;
                }
            } while (false);
            if (options.ContainsKey("version"))
            {
                var asm = Assembly.GetExecutingAssembly();
                var version = Attribute.GetCustomAttribute(asm, typeof(AssemblyFileVersionAttribute)) as AssemblyFileVersionAttribute;
                Console.WriteLine(Resources.MessageVersion, Path.GetFileName(asm.Location), version?.Version);
                return 0;
            }
            Encoding encode = null;
            do
            {
                if (options.ContainsKey("encoding") || options.ContainsKey("encodings"))
                {
                    if (options.ContainsKey("encoding"))
                    {
                        var str = options["encoding"];
                        var hasBe = str.ToLower().Contains("be");
                        var hasBom = str.ToLower().Contains("bom") && !str.ToLower().Contains("nobom");
                        if (str.StartsWith("utf-8", StringComparison.CurrentCultureIgnoreCase) || str.StartsWith("utf8", StringComparison.CurrentCultureIgnoreCase))
                        {
                            encode = new UTF8Encoding(hasBom);
                            break;
                        }
                        else if (str.StartsWith("utf-32", StringComparison.CurrentCultureIgnoreCase) || str.StartsWith("utf32", StringComparison.CurrentCultureIgnoreCase))
                        {
                            encode = new UTF32Encoding(hasBe, hasBom);
                            break;
                        }
                        else if (str.StartsWith("utf-16", StringComparison.CurrentCultureIgnoreCase) || str.StartsWith("utf16", StringComparison.CurrentCultureIgnoreCase))
                        {
                            encode = new UnicodeEncoding(hasBe, hasBom);
                            break;
                        }

                        try
                        {
                            encode = Encoding.GetEncoding(str);
                            break;
                        }
                        catch (ArgumentException ex)
                        {
                            Console.WriteLine(Resources.ErrorInvalidEncoding, str, ex.Message);
                        }
                    }
                    Console.WriteLine();
                    Console.WriteLine(Resources.HelpEncoding);
                    foreach (var enc in Encoding.GetEncodings().OrderBy(w => w.Name))
                    {
                        Console.WriteLine("\t{0}\t{1}", enc.Name, enc.DisplayName);
                    }
                    Console.WriteLine("\t{0}",Resources.HelpEncodingExt);
                    return options.ContainsKey("encodings")  ? 0 : -1;
                }
            } while (false);
            if (_items.Count < 1 || options.ContainsKey("help") || !options.ContainsKey("output"))
            {
                var asm = Assembly.GetExecutingAssembly();
                var copyright = Attribute.GetCustomAttribute(asm, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
                var title = Attribute.GetCustomAttribute(asm, typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
                Console.WriteLine("{0} ({1})", title?.Title, copyright?.Copyright);
                var filename = Path.GetFileName(asm.Location);
                Console.WriteLine(Resources.HelpMessage, filename, Resources.HelpEncodingExt, string.Join("|", Enum.GetNames(typeof(NewLineKind))));
                foreach (var opts in optionParams)
                {
                    Console.Write("\t");
                    if (!string.IsNullOrEmpty(opts[0]))
                    {
                        if (!string.IsNullOrEmpty(opts[2]))
                            Console.Write("-{0}={2} ", opts);
                        else
                            Console.Write("-{0} ", opts);
                    }
                    if (!string.IsNullOrEmpty(opts[1]))
                    {
                        if (!string.IsNullOrEmpty(opts[2]))
                            Console.Write("[--{1}={2}]", opts);
                        else
                            Console.Write("[--{1}]", opts);
                    }
                    Console.WriteLine();
                    foreach(var line in opts.Skip(3))
                        Console.WriteLine("\t\t{0}", line);
                    Console.WriteLine();
                }

                return options.ContainsKey("help")  ? 0 : -1;
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

            var _ex = new XmlComplexer();
            var _basedoc = _ex.Combine(_items.First(), _items.Skip(1).ToArray());
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
        /// Get option strings from argument array
        /// </summary>
        /// <param name="args">argument array</param>
        /// <returns>Option pair</returns>
        /// <remarks>
        /// Start with - value is option and value is separated by =.
        /// </remarks>
        static Dictionary<string, string> GetOptions(string[] args, string[][] transcodes)
        {
            return args.Where(w => w.StartsWith("-"))
                .Select((item, i) => item.TrimStart('-').Split(new[] { '=' }, 2))
                .Select((k, i) =>
                {
                    var key = k[0].ToLower();
                    var trans = transcodes?.FirstOrDefault(w => w[0] == key);
                    return new
                    {
                        Key = trans != null ? trans[1] : key,
                        Value = k.Length > 1 ? k[1] : string.Empty,
                    };
                })
                .GroupBy(w => w.Key)
                .ToDictionary(k => k.Key, v => v.First().Value);
        }
        class OptionValues
        {
            public string Short { get; set; }
            public string Long { get; set; }

        }
        /// <summary>
        /// Get arguments without options
        /// </summary>
        /// <param name="args">Argument array</param>
        /// <returns>String array</returns>
        static IEnumerable<string> GetNoOption(string[] args)
        {
            return args.Where(w => !w.StartsWith("-"));
        }
    }
}
