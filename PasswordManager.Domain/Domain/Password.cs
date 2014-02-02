using System;

namespace PasswordManager.Helper.Domain
{
    public class Password : IComparable
    {
        public string Title { get; set; }
        public string UserName { get; set; }
        public string PasswordText { get; set; }
        public Guid KeyGuid { get; set; }
        public string AlphaIcon
        {
            get
            {
                string alphaIcon = string.Empty;

                string [] words = Title.Split(new string [] { " "}, StringSplitOptions.RemoveEmptyEntries);

                //If there is only 1 word then we abbrv to first three letters
                if (words.Length == 1)
                    return words[0].Substring(0,1).ToUpperInvariant();

                //Abbrv using the first and the last word
                return words[0].Substring(0, 1).ToUpper() + " " + words[words.Length - 1].Substring(0, 1).ToUpper();
            }
        }

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