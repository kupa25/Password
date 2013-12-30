namespace PasswordManager.Domain
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Password
    {
        public string Title { get; set; }
        public string UserName { get; set; }
        public string PasswordText { get; set; }
        public Guid KeyGuid { get; set; }

        public override string ToString()
        {
            return this.UserName;
        }
    }
}
