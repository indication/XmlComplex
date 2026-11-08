using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using XmlComplex.Properties;

namespace XmlComplex
{
    internal class CommandArguments
    {
        private readonly OptionHelper optionHelper;

        private CommandArguments(List<string> inputFiles, Dictionary<string, string> options, OptionHelper optionHelper)
        {
            InputFiles = inputFiles;
            Options = options;
            this.optionHelper = optionHelper;
            NewLineChars = Environment.NewLine;
            Encoding = null;
        }

        /// <summary>
        /// オプション以外で指定された入力ファイル一覧を取得します。
        /// </summary>
        public IList<string> InputFiles { get; }

        /// <summary>
        /// 解析済みのオプション辞書を取得します。
        /// </summary>
        public Dictionary<string, string> Options { get; }

        /// <summary>
        /// 解決済みの改行コードを取得します。
        /// </summary>
        public string NewLineChars { get; private set; }

        /// <summary>
        /// 解決済みの出力エンコーディングを取得します。
        /// </summary>
        public Encoding Encoding { get; private set; }

        /// <summary>
        /// バージョン表示オプションが指定されているか取得します。
        /// </summary>
        public bool ShowVersion => Options.ContainsKey("version");

        /// <summary>
        /// ヘルプ表示オプションが指定されているか取得します。
        /// </summary>
        public bool HasHelpOption => Options.ContainsKey("help");

        /// <summary>
        /// エンコーディング一覧表示オプションが指定されているか取得します。
        /// </summary>
        public bool ShowEncodings => Options.ContainsKey("encodings");

        /// <summary>
        /// 出力ファイルオプションが指定されているか取得します。
        /// </summary>
        public bool HasOutput => Options.ContainsKey("output");

        /// <summary>
        /// エンコーディングオプションが指定されているか取得します。
        /// </summary>
        public bool HasEncodingOption => Options.ContainsKey("encoding");

        /// <summary>
        /// インデントを有効にするか取得します。
        /// </summary>
        public bool Indent => !Options.ContainsKey("no-indent");

        /// <summary>
        /// インデント文字列を取得します。
        /// </summary>
        public string IndentChars => Options.ContainsKey("indent") ? Options["indent"] : "  ";

        /// <summary>
        /// 出力ファイルパスを取得します。
        /// </summary>
        public string OutputPath => Options["output"];

        /// <summary>
        /// コマンドライン引数の定義に使用するオプションヘルパーを生成します。
        /// </summary>
        /// <returns>オプション定義済みの <see cref="OptionHelper"/>。</returns>
        public static OptionHelper CreateOptionHelper()
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

        /// <summary>
        /// コマンドライン引数を解析し、<see cref="CommandArguments"/> インスタンスを生成します。
        /// </summary>
        /// <param name="args">解析対象のコマンドライン引数。</param>
        /// <returns>解析結果を保持する <see cref="CommandArguments"/>。</returns>
        public static CommandArguments Parse(string[] args)
        {
            var optionHelper = CreateOptionHelper();
            var inputFiles = optionHelper.GetNoOption(args).ToList();
            var options = optionHelper.GetOptions(args);
            return new CommandArguments(inputFiles, options, optionHelper);
        }

        /// <summary>
        /// 実行に必要な必須引数が揃っているか判定します。
        /// </summary>
        /// <returns>入力ファイルと出力指定があり、ヘルプ指定がない場合は <c>true</c>。</returns>
        public bool HasRequiredArguments()
        {
            return InputFiles.Count > 0 && HasOutput && !HasHelpOption;
        }

        /// <summary>
        /// 改行オプションを解決し、<see cref="NewLineChars"/> に設定します。
        /// </summary>
        /// <returns>改行コードを解決できた場合は <c>true</c>。</returns>
        public bool TryGetNewLine()
        {
            NewLineChars = Environment.NewLine;
            if (!Options.ContainsKey("newline"))
                return true;

            var resolvedNewLine = NewLineResolver.Resolve(Options["newline"]);
            if (resolvedNewLine != null)
            {
                NewLineChars = resolvedNewLine;
                return true;
            }

            Console.WriteLine(Resources.ErrorInvalidLineCode, Options["newline"]);
            return false;
        }

        /// <summary>
        /// エンコーディングオプションを解決し、<see cref="Encoding"/> に設定します。
        /// </summary>
        /// <param name="exitCode">処理終了時に返す終了コード。</param>
        /// <param name="showEncodings">エンコーディング一覧表示が必要な場合は <c>true</c>。</param>
        /// <returns>エンコーディング指定が有効で処理継続可能な場合は <c>true</c>。</returns>
        public bool TryGetEncoding(out int exitCode, out bool showEncodings)
        {
            Encoding = null;
            exitCode = 0;
            showEncodings = false;
            if (!HasEncodingOption && !ShowEncodings)
                return true;

            if (HasEncodingOption)
            {
                var encodingName = Options["encoding"];
                try
                {
                    Encoding = EncodingResolver.Resolve(encodingName);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(Resources.ErrorInvalidEncoding, encodingName, ex.Message);
                    Console.WriteLine();
                }
            }

            if (Encoding != null && !ShowEncodings)
                return true;

            showEncodings = true;
            exitCode = ShowEncodings ? 0 : -1;
            return false;
        }

        /// <summary>
        /// 指定された入力ファイルがすべて存在するか確認します。
        /// </summary>
        /// <returns>すべての入力ファイルが存在する場合は <c>true</c>。</returns>
        public bool HasAllInputFiles()
        {
            var hasAll = true;
            foreach (var item in InputFiles.Where(w => !File.Exists(w)))
            {
                Console.WriteLine(string.Format(Resources.ErrorFileNotFound, item));
                hasAll = false;
            }
            return hasAll;
        }

        /// <summary>
        /// アプリケーションのヘルプメッセージを標準出力へ表示します。
        /// </summary>
        public void ShowHelp()
        {
            var asm = Assembly.GetExecutingAssembly();
            var copyright = Attribute.GetCustomAttribute(asm, typeof(AssemblyCopyrightAttribute)) as AssemblyCopyrightAttribute;
            var title = Attribute.GetCustomAttribute(asm, typeof(AssemblyTitleAttribute)) as AssemblyTitleAttribute;
            Console.WriteLine("{0} ({1})", title?.Title, copyright?.Copyright);
            var filename = Path.GetFileName(asm.Location);
            Console.WriteLine(Resources.HelpMessage, filename, Resources.HelpEncodingExt, string.Join("|", Enum.GetNames(typeof(NewLineKind))));
            foreach (var line in optionHelper.GetHelp())
                Console.WriteLine(line);
        }
    }
}
