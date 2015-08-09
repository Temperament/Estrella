using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zepheus.Database.Storage
{
   public class User
    {
        public long ID { get;  set; }
        public string Username { get;  set; }
        public string Password { get;  set; }
        public byte Admin { get; set; }
        public bool Banned { get;  set; }
        public bool Logged { get; set; }
    }
}
