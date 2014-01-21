using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using PasswordManager.Domain;
using Newtonsoft.Json;
using System.Diagnostics;
using Windows.UI.Popups;

namespace PasswordManager.Utility
{
    public class Storage
    {
        /// <summary>
        /// The settings.
        /// </summary>
        private static ApplicationDataContainer cloudStorage = ApplicationData.Current.RoamingSettings;
        private static ApplicationDataContainer localStorage = ApplicationData.Current.LocalSettings;

        private static List<Password> passwordList;

        private static List<Password> cachedPasswordList
        {
            get
            {
                if (passwordList == null)
                {
                    passwordList = new List<Password>();
                }

                return passwordList;
            }

            set
            {
                passwordList = value;
            }
        }

        public static List<Password> RetreivePassword()
        {
            if (cachedPasswordList.Count <= 0 && localStorage != null)
            {
                GetPassword(localStorage);
            }
            else if (cachedPasswordList == null && localStorage == null)
            {
                GetPassword(cloudStorage);
            }

            cachedPasswordList.Sort();

            return cachedPasswordList;
        }

        private static void GetPassword(ApplicationDataContainer storage)
        {
            foreach (KeyValuePair<string, object> pair in storage.Values)
            {
                Password deserializedObj;
                try
                {
                    deserializedObj = JsonConvert.DeserializeObject<Password>(pair.Value.ToString());
                }
                catch (Exception)
                {
                    deserializedObj = new Password { Title = pair.Key, UserName = pair.Value.ToString() };
                }

                cachedPasswordList.Add(deserializedObj);
            }
        }

        private void sync()
        {
            if (Helper.IsInternet)
            {
                //download the cloud storage
                ApplicationDataContainer tempStorage = cloudStorage;

                //if there is any difference between local and cloud storage then store them in the downloaded copy
                foreach (Password pwd in cachedPasswordList)
                {
                    if (!tempStorage.Values.Contains(new KeyValuePair<string, object>(pwd.KeyGuid.ToString(), JsonConvert.SerializeObject(pwd))))
                    {
                        tempStorage.Values.Add(pwd.KeyGuid.ToString(), JsonConvert.SerializeObject(pwd));
                    }
                }
            }
        }

        public static void AddPassword(Password pwd)
        {
            if(pwd != null)
            {
                cachedPasswordList.Add(pwd);
                localStorage.Values.Add(pwd.KeyGuid.ToString(), JsonConvert.SerializeObject(pwd));
            }
        }

        public static void DeletePassword(Password pwd)
        {
            if (pwd != null)
            {
                Debug.WriteLine("Delete is going to match on the following key : " + (pwd.KeyGuid.Equals(Guid.Empty) ? pwd.Title : pwd.KeyGuid.ToString()));

                cachedPasswordList.Remove(pwd);

                localStorage.Values.Remove(pwd.KeyGuid.ToString());
            }
            else
            {
                Debug.WriteLine("selected item was not a Password object");
            }
        }

        public static void RemovePasswordList()
        {
            cachedPasswordList.Clear();
            localStorage.Values.Clear();

            //we will not sync here as, the user may want to undo.
        }
    }
}
