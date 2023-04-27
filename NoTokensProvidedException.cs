using System;
using System.Runtime.Serialization;

namespace EsotericDevZone.RuleBasedParser
{
    [Serializable]
    public class NoTokensProvidedException : Exception
    {
        public NoTokensProvidedException()
        {
        }

        public NoTokensProvidedException(string message) : base(message)
        {
        }

        public NoTokensProvidedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoTokensProvidedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}