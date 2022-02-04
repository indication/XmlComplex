using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlComplex
{
    partial class OptionHelper
    {
        /// <summary>
        /// Options param
        /// </summary>
        protected internal List<OptionValue> OptionValues { get; } = new List<OptionValue>();
        /// <summary>
        /// Add option
        /// </summary>
        /// <param name="shortName">short name</param>
        /// <param name="longName">long name</param>
        /// <param name="optName">option name for help message</param>
        /// <param name="messages">help messages</param>
        /// <returns>this instance</returns>
        public OptionHelper AddOption(string shortName, string longName, string optName, params string[] messages)
        {
            OptionValues.Add(new OptionValue(shortName, longName, optName, messages));
            return this;
        }
        /// <summary>
        /// Get option strings from argument array
        /// </summary>
        /// <param name="args">argument array</param>
        /// <returns>Option pair</returns>
        /// <remarks>
        /// Start with - value is option and value is separated by =.
        /// </remarks>
        public Dictionary<string, string> GetOptions(string[] args)
        {
            return args.Where(w => w.StartsWith("-"))
                .Select((item, i) => item.TrimStart('-').Split(new[] { '=' }, 2))
                .Select((k, i) =>
                {
                    var key = k[0].ToLower();
                    var trans = OptionValues?.FirstOrDefault(w => w.Short == key);
                    return new
                    {
                        Key = trans != null ? trans.Long : key,
                        Value = k.Length > 1 ? k[1] : string.Empty,
                    };
                })
                .GroupBy(w => w.Key)
                .ToDictionary(k => k.Key, v => v.First().Value);
        }
        /// <summary>
        /// Get arguments without options
        /// </summary>
        /// <param name="args">Argument array</param>
        /// <returns>String array</returns>
        public IEnumerable<string> GetNoOption(string[] args)
        {
            return args.Where(w => !w.StartsWith("-"));
        }

        /// <summary>
        /// Get argument helps
        /// </summary>
        /// <param name="args">Argument array</param>
        /// <returns></returns>
        public IEnumerable<string> GetHelp()
        {
            var sb = new StringBuilder();
            foreach (var opts in OptionValues)
            {
                sb.Clear();
                sb.Append("\t");
                if (!string.IsNullOrEmpty(opts.Short))
                {
                    if (!string.IsNullOrEmpty(opts.HelpOptionName))
                        sb.AppendFormat("-{0}={2} ", opts.Short, opts.Long, opts.HelpOptionName);
                    else
                        sb.AppendFormat("-{0} ", opts.Short, opts.Long, opts.HelpOptionName);
                }
                if (!string.IsNullOrEmpty(opts.Long))
                {
                    if (!string.IsNullOrEmpty(opts.HelpOptionName))
                        sb.AppendFormat("[--{1}={2}]", opts.Short, opts.Long, opts.HelpOptionName);
                    else
                        sb.AppendFormat("[--{1}]", opts.Short, opts.Long, opts.HelpOptionName);
                }
                yield return sb.ToString();
                foreach (var line in opts.HelpMessages)
                    yield return string.Format("\t\t{0}", line.Replace("\r\n", "\r\n\t\t"));
                yield return string.Empty;
            }
        }
    }
}
