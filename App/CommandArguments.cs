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
        /// <exception cref="CommandArgumentException">引数が無効、または表示専用オプションで処理終了する場合。</exception>
        public static CommandArguments Parse(string[] args)
        {
            var optionHelper = CreateOptionHelper();
            var inputFiles = optionHelper.GetNoOption(args).ToList();
            var options = optionHelper.GetOptions(args);
            var commandArguments = new CommandArguments(inputFiles, options, optionHelper);

            commandArguments.ResolveNewLineOrThrow();
            commandArguments.ResolveEncodingOrThrow();
            commandArguments.EnsureRequiredArgumentsOrThrow();
            commandArguments.EnsureAllInputFilesOrThrow();
            commandArguments.ThrowIfHelp();
            commandArguments.ThrowIfVersion();
            return commandArguments;
        }

        /// <summary>
        /// 改行コードオプションを解決し、無効な場合は例外を送出します。
        /// </summary>
        /// <exception cref="InvalidCommandArgumentException">改行コードが不正な場合。</exception>
        private void ResolveNewLineOrThrow()
        {
            NewLineChars = Environment.NewLine;
            if (!Options.ContainsKey("newline"))
                return;

            var resolvedNewLine = NewLineResolver.Resolve(Options["newline"]);
            if (resolvedNewLine != null)
            {
                NewLineChars = resolvedNewLine;
                return;
            }

            Console.WriteLine(Resources.ErrorInvalidLineCode, Options["newline"]);
            throw new InvalidCommandArgumentException();
        }

        /// <summary>
        /// エンコーディング関連オプションを解決し、継続不可の場合は例外を送出します。
        /// </summary>
        /// <exception cref="CommandArgumentSuccessException">一覧表示のみで正常終了する場合。</exception>
        /// <exception cref="InvalidCommandArgumentException">エンコーディング指定が不正な場合。</exception>
        private void ResolveEncodingOrThrow()
        {
            Encoding = null;
            if (!HasEncodingOption && !ShowEncodings)
                return;

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
                return;

            ShowEncodingsHelp();
            if (ShowEncodings)
                throw new CommandArgumentSuccessException();

            throw new InvalidCommandArgumentException();
        }

        /// <summary>
        /// 必須引数の有無を検証し、不足時はヘルプを表示して例外を送出します。
        /// </summary>
        /// <exception cref="CommandArgumentSuccessException">ヘルプ表示指定で正常終了する場合。</exception>
        /// <exception cref="InvalidCommandArgumentException">必須引数が不足している場合。</exception>
        private void EnsureRequiredArgumentsOrThrow()
        {
            if (InputFiles.Count > 0 && HasOutput && !HasHelpOption)
                return;

            ShowHelp();
            if (HasHelpOption)
                throw new CommandArgumentSuccessException();

            throw new InvalidCommandArgumentException();
        }

        /// <summary>
        /// 入力ファイルの存在を検証し、見つからないファイルがある場合は例外を送出します。
        /// </summary>
        /// <exception cref="MissingInputFileCommandArgumentException">入力ファイルが存在しない場合。</exception>
        private void EnsureAllInputFilesOrThrow()
        {
            var hasAll = true;
            foreach (var item in InputFiles.Where(w => !File.Exists(w)))
            {
                Console.WriteLine(string.Format(Resources.ErrorFileNotFound, item));
                hasAll = false;
            }

            if (!hasAll)
                throw new MissingInputFileCommandArgumentException();
        }

        /// <summary>
        /// バージョン情報を表示します。
        /// </summary>
        /// <exception cref="CommandArgumentSuccessException">成功</exception>
        private void ThrowIfVersion()
        {
            if (!ShowVersion)
                return;

            ShowVersionMessage();
            throw new CommandArgumentSuccessException();
        }

        /// <summary>
        /// ヘルプ オプションが指定されている場合にヘルプを表示し、正常終了を通知する例外をスローします。
        /// </summary>
        /// <remarks>ヘルプ オプションが指定されていない場合は何も実行しません。</remarks>
        /// <exception cref="CommandArgumentSuccessException">ヘルプ オプションが指定されている場合に、ヘルプ表示後の正常終了を通知するためにスローされます。</exception>
        private void ThrowIfHelp ()
        {
            if (!HasHelpOption)
                return;

            ShowHelp();
            throw new CommandArgumentSuccessException();
        }

        /// <summary>
        /// 利用可能なエンコーディング一覧を標準出力へ表示します。
        /// </summary>
        private void ShowEncodingsHelp()
        {
            Console.WriteLine(Resources.HelpEncoding);
            foreach (var enc in Encoding.GetEncodings().OrderBy(w => w.Name))
            {
                Console.WriteLine("\t{0}\t{1}", enc.Name, enc.DisplayName);
            }
            Console.WriteLine("\t{0}", Resources.HelpEncodingExt);
        }

        /// <summary>
        /// バージョン情報を標準出力へ表示します。
        /// </summary>
        public void ShowVersionMessage()
        {
            var asm = Assembly.GetExecutingAssembly();
            var version = Attribute.GetCustomAttribute(asm, typeof(AssemblyFileVersionAttribute)) as AssemblyFileVersionAttribute;
            Console.WriteLine(Resources.MessageVersion, Path.GetFileName(asm.Location), version?.Version);
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
