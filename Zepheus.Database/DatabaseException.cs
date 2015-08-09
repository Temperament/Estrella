using System;

namespace Zepheus.Database
{
        [Serializable()]
        public class DatabaseException : Exception
        {
            internal DatabaseException(string sMessage) : base(sMessage) { }
        }
}