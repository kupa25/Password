using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager.Domain
{
    public class Password
    {
        public string Title { get; set; }
        public string UserName { get; set; }
        public string PasswordText { get; set; }

        public override string ToString()
        {
            return this.UserName;
        }
    }
}
