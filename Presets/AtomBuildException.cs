using System;
using System.Runtime.Serialization;

namespace EsotericDevZone.RuleBasedParser.Presets
{
    [Serializable]
    internal class AtomBuildException : Exception
    {
        public AtomBuildException()
        {
        }

        public AtomBuildException(string message) : base(message)
        {
        }

        public AtomBuildException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AtomBuildException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}