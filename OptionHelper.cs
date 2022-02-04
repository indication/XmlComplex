using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlComplex
{
    static class OptionHelper
    {

        /// <summary>
        /// Get option strings from argument array
        /// </summary>
        /// <param name="args">argument array</param>
        /// <returns>Option pair</returns>
        /// <remarks>
        /// Start with - value is option and value is separated by =.
        /// </remarks>
        static public Dictionary<string, string> GetOptions(string[] args, string[][] transcodes)
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
        /// <summary>
        /// Get arguments without options
        /// </summary>
        /// <param name="args">Argument array</param>
        /// <returns>String array</returns>
        static public IEnumerable<string> GetNoOption(string[] args)
        {
            return args.Where(w => !w.StartsWith("-"));
        }

        /// <summary>
        /// Get argument helps
        /// </summary>
        /// <param name="args">Argument array</param>
        /// <returns></returns>
        static public IEnumerable<string> GetHelp(string[][] args)
        {
            var sb = new StringBuilder();
            foreach (var opts in args)
            {
                sb.Clear();
                sb.Append("\t");
                if (!string.IsNullOrEmpty(opts[0]))
                {
                    if (!string.IsNullOrEmpty(opts[2]))
                        sb.AppendFormat("-{0}={2} ", opts);
                    else
                        sb.AppendFormat("-{0} ", opts);
                }
                if (!string.IsNullOrEmpty(opts[1]))
                {
                    if (!string.IsNullOrEmpty(opts[2]))
                        sb.AppendFormat("[--{1}={2}]", opts);
                    else
                        sb.AppendFormat("[--{1}]", opts);
                }
                yield return sb.ToString();
                foreach (var line in opts.Skip(3))
                    yield return string.Format("\t\t{0}", line.Replace("\r\n", "\r\n\t\t"));
                yield return string.Empty;
            }
        }
    }
}
