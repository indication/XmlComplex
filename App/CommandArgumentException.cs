using System;

namespace XmlComplex
{
    internal abstract class CommandArgumentException : ArgumentException
    {
        protected CommandArgumentException(string message = null)
            : base(message)
        {
        }

        public abstract int ExitCode { get; }
    }

    internal sealed class CommandArgumentSuccessException : CommandArgumentException
    {
        public const int ExitCodeValue = 0;

        public CommandArgumentSuccessException(string message = null)
            : base(message)
        {
        }

        public override int ExitCode => ExitCodeValue;
    }

    internal sealed class InvalidCommandArgumentException : CommandArgumentException
    {
        public const int ExitCodeValue = -1;

        public InvalidCommandArgumentException(string message = null)
            : base(message)
        {
        }

        public override int ExitCode => ExitCodeValue;
    }

    internal sealed class MissingInputFileCommandArgumentException : CommandArgumentException
    {
        public const int ExitCodeValue = -2;

        public MissingInputFileCommandArgumentException(string message = null)
            : base(message)
        {
        }

        public override int ExitCode => ExitCodeValue;
    }
}
