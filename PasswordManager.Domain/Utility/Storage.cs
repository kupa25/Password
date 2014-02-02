using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Storage;
using Newtonsoft.Json;
using PasswordManager.Helper.Domain;

namespace PasswordManager.Helper.Utility
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
                    passwordList = GetPassword(localStorage);

                    if (Helper.IsInternet)
                    {
                        if (passwordList.Count <= 0)
                        {
                            passwordList = GetPassword(cloudStorage);
                        }
                    }
                }

                return passwordList;
            }
            set { passwordList = value; }
        }

        public static List<Password> RetreivePassword()
        {
            cachedPasswordList.Sort();

            return cachedPasswordList;
        }

        private static List<Password> GetPassword(ApplicationDataContainer storage)
        {
            List<Password> passwords = new List<Password>();

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

                passwords.Add(deserializedObj);
            }

            return passwords;
        }

        public static void sync()
        {
            Debug.WriteLine("Trying to Synchronize");

            if (Helper.IsInternet)
            {
                // sync from up from cache to cloud
                foreach (Password pwd in cachedPasswordList)
                {
                    if (!cloudStorage.Values.Contains(new KeyValuePair<string, object>(pwd.KeyGuid.ToString(), JsonConvert.SerializeObject(pwd))))
                    {
                        Debug.WriteLine("SYNC UP: "+ pwd);
                        cloudStorage.Values.Add(pwd.KeyGuid.ToString(), JsonConvert.SerializeObject(pwd));
                    }
                }

                // Sync stuff down from cloud to local
                foreach (Password pwd in GetPassword(cloudStorage))
                {
                    if (!localStorage.Values.Contains(new KeyValuePair<string, object>(pwd.KeyGuid.ToString(), JsonConvert.SerializeObject(pwd))))
                    {
                        Debug.WriteLine("SYNC Down: " + pwd);
                        localStorage.Values.Add(pwd.KeyGuid.ToString(), JsonConvert.SerializeObject(pwd));
                    }
                }

                // update the cache from local
                cachedPasswordList = GetPassword(localStorage);
            }
        }

        public static void AddPassword(Password pwd)
        {
            if(pwd != null)
            {
                cachedPasswordList.Add(pwd);
                localStorage.Values.Add(pwd.KeyGuid.ToString(), JsonConvert.SerializeObject(pwd));

                if (Helper.IsInternet)
                {
                    cloudStorage.Values.Add(pwd.KeyGuid.ToString(), JsonConvert.SerializeObject(pwd));
                }
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
            cloudStorage.Values.Clear();
        }
    }
}
