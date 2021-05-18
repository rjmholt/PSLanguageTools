using System;

namespace PSLanguageTools.CodeAnalysis
{
    public class NonConstantValueException : Exception
    {
        public NonConstantValueException() : base()
        {
        }

        public NonConstantValueException(string message) : base(message)
        {
        }
    }
}