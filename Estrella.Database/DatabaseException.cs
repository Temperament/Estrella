using System;

namespace Estrella.Database
{
        [Serializable()]
        public class DatabaseException : Exception
        {
            internal DatabaseException(string sMessage) : base(sMessage) { }
        }
}