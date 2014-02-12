using Windows.Storage.Search;

namespace PasswordManager.Helper.Domain
{
    public enum ResultsType
    {
        Success = 1,
        Error = 2
    }

    public class Results
    {
        private ResultsType resultType;

        public ResultsType ResultsType
        {
            get
            {
                return resultType;
            }

            set
            {
                resultType = value;
            }
        }

        public string Message { get; set; }
    }
}
