namespace PasswordManager.Domain
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Password : IComparable
    {
        public string Title { get; set; }
        public string UserName { get; set; }
        public string PasswordText { get; set; }
        public Guid KeyGuid { get; set; }

        public override string ToString()
        {
            return this.UserName;
        }

        public int CompareTo(object obj)
        {
            var passwordToCompare = (Password)obj;

            return passwordToCompare == null ? 0 : this.Title.CompareTo(passwordToCompare.Title);
        }
    }
}