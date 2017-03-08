using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XmlComplex
{
    class Program
    {
        
        [STAThread]
        static int Main(string[] args)
        {
            var _items = GetNoOption(args).ToList();
            Console.WriteLine("args: {0}", string.Join(",", _items));
            var options = GetOptions(args, new[] {
                //from, to
                new [] { "o", "output" },
                new [] { "n", "newline" },
                new [] { "es", "encodings" },
                new [] { "e", "encoding" },
                new [] { "v", "version" },
                new [] { "h", "help" },
            });

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
                    Console.WriteLine("Line code is invalid: {0}", options["newline"]);
                    return -1;
                }
            } while (false);
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
                            Console.WriteLine("Encoding {0} is invalid: {1}", str, ex.Message);
                        }
                    }
                    Console.WriteLine();
                    Console.WriteLine("Supported encodings:");
                    foreach (var enc in Encoding.GetEncodings().OrderBy(w => w.Name))
                    {
                        Console.WriteLine("\t{0}\t{1}", enc.Name, enc.DisplayName);
                    }
                    Console.WriteLine("\tUTF-8BOM/UTF-16BOM/UTF-16BEBOM/UTF-32BOM/UTF-32BEBOM is supported.");
                    return -1;
                }
            } while (false);
            if (_items.Count < 1 || options.ContainsKey("help") || !options.ContainsKey("output"))
            {
                var filename = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
                Console.WriteLine("{0} -o=exportfile.xml inputfile1.xml inputfile2.xml", filename);
                Console.WriteLine("Options: ");
                Console.WriteLine("\t-o=FILE [--output=FILE]");
                Console.WriteLine("\t\tOutput file. Specify export file. It is able to set input file");
                Console.WriteLine();
                Console.WriteLine("\t-e=ENCODNG [--encoding=ENCODING]");
                Console.WriteLine("\t\tOutput encoding. default: utf8 (without BOM)");
                Console.WriteLine("\t\tUTF-8BOM/UTF-16BOM/UTF-16BEBOM/UTF-32BOM/UTF-32BEBOM is supported.");
                Console.WriteLine();
                Console.WriteLine("\t-n=LINECODE [--newline=LINECODE]");
                Console.WriteLine("\t\tOutput line break charactor(s). default: CRLF");
                Console.WriteLine("\t\tLINECODE: {0}", string.Join("|", Enum.GetNames(typeof(NewLineKind))));
                Console.WriteLine();
                Console.WriteLine("\t-es [--encodings]");
                Console.WriteLine("\t\tShow supported encodings.");
                Console.WriteLine();
                Console.WriteLine("\t-h --help");
                Console.WriteLine("\t\tShow this message");
                return -1;
            }
            
            bool findAll = true;
            foreach (var _item in _items.Where(w => !File.Exists(w)))
            {
                Console.WriteLine(string.Format("{0} is not Found.", _item));
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
                using (var stream = new StreamWriter(options["output"], false, encode))
                {
                    if (newline != null)
                        stream.NewLine = newline;
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
