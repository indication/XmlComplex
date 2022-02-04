namespace XmlComplex
{
    partial class OptionHelper
    {
        /// <summary>
        /// Option value
        /// </summary>
        protected internal class OptionValue
        {
            /// <summary>
            /// Option value
            /// </summary>
            /// <param name="shortName">Short key</param>
            /// <param name="longName">Long key</param>
            /// <param name="optName">Option name for help</param>
            /// <param name="messages">Help descriptions</param>
            public OptionValue(string shortName, string longName, string optName, params string[] messages)
            {
                Short = shortName;
                Long = longName;
                HelpOptionName = optName;
                HelpMessages = messages;
            }
            /// <summary>
            /// Short key
            /// </summary>
            public string Short { get; set; }
            /// <summary>
            /// Long key
            /// </summary>
            public string Long { get; set; }
            /// <summary>
            /// Option name for help
            /// </summary>
            public string HelpOptionName { get; set; }
            /// <summary>
            /// Help description
            /// </summary>
            public string[] HelpMessages { get; set; }

        }
    }
}
