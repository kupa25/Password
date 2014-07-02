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

                    if (Helper.IsInternetAvailable)
                    {
                        if (passwordList.Count <= 0)
                        {
                            passwordList = GetPassword(cloudStorage);
                        }
                    }
                }
                
                return passwordList;
            }
            set
            {
                passwordList = value;
            }
        }

        public static Password tempPassword;

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
                try
                {
                    passwords.Add(JsonConvert.DeserializeObject<Password>(pair.Value.ToString()));
                }
                catch (Exception)
                { }
            }

            return passwords;
        }

        public static void sync()
        {
            Debug.WriteLine("Trying to Synchronize");

            if (Helper.IsInternetAvailable)
            {
                int cloudVersion = cloudStorage.Values.ContainsKey("Version") ? 
                    (int)cloudStorage.Values["Version"] : 0;

                int localVersion = localStorage.Values.ContainsKey("Version") ? 
                    (int)localStorage.Values["Version"] : 0;

                // (Version Check)
                Debug.WriteLine(
                    string.Format(@"--- Version Check ---
                                    Cloud Version :{0}
                                    Local Version :{1}", cloudVersion, localVersion));

                if (cloudVersion >= localVersion)
                {
                    // restore down from cloud

                    localStorage.Values.Clear();

                    foreach (var value in cloudStorage.Values)
                    {
                        localStorage.Values.Add(value);
                    }
                }
                else
                {
                    // Sync up
                    cloudStorage.Values.Clear();

                    foreach (var value in localStorage.Values)
                    {
                        cloudStorage.Values.Add(value);
                    }
                }

                if (cloudStorage.Values.ContainsKey("Version"))
                {
                    cloudStorage.Values.Remove("Version");
                }

                if (localStorage.Values.ContainsKey("Version"))
                {
                    localStorage.Values.Remove("Version");
                }
                
                cloudStorage.Values.Add("Version", cloudVersion + 1);
                localStorage.Values.Add("Version", cloudVersion + 1);
            }

            // update the cache from local
            cachedPasswordList = GetPassword(localStorage);
        }

        public static Results AddPassword(Password pwd)
        {
            Results results = null;
            if(pwd != null)
            {
                try
                {
                    //TODO: Have a rollback mechanism so that if any of the storage fails then we rollback.
                    var pair = CreateKeyValuePair(pwd);
                    bool foundValue = false;

                    if (tempPassword != null)
                    {
                        foundValue = localStorage.Values.ContainsKey(tempPassword.Key);
                    }

                    if (foundValue)
                    {
                        localStorage.Values.Remove(tempPassword.Key);
                    }

                    localStorage.Values.Add(pair);

                    if (Helper.IsInternetAvailable)
                    {
                        foundValue = false;

                        if (tempPassword != null)
                        {
                            foundValue = cloudStorage.Values.ContainsKey(tempPassword.Key);
                        }

                        if (foundValue)
                        {
                            cloudStorage.Values.Remove(tempPassword.Key);
                        }

                        cloudStorage.Values.Add(pair);

                        if (localStorage.Values.ContainsKey("Sync"))
                        {
                            localStorage.Values.Remove("Sync");   
                        }
                    }
                    else
                    {
                        if (!localStorage.Values.ContainsKey("Sync"))
                        {
                            int localversion = 0;
                            if (localStorage.Values.ContainsKey("Version"))
                            {
                                localversion = (int)localStorage.Values["Version"];
                                localStorage.Values.Remove("Version");
                            }

                            localStorage.Values.Add("Version", localversion + 1);
                            localStorage.Values.Add("Sync", true);
                        }

                    }

                    if (cachedPasswordList.Contains(tempPassword))
                    {
                        cachedPasswordList.Remove(tempPassword);
                    }

                    cachedPasswordList.Add(pwd);
                }
                catch(Exception exception)
                {
                    results = new Results{Message = "Error Adding the password: "+ exception.Message, ResultsType = ResultsType.Error};
                }
            }

            return results ?? new Results {Message = null, ResultsType = ResultsType.Success};
        }

        public static void DeletePassword(Password pwd)
        {
            if (pwd != null)
            {
                Debug.WriteLine("Delete is going to match on the following key : " + (pwd.KeyGuid.Equals(Guid.Empty) ? pwd.Title : pwd.KeyGuid.ToString()));

                cachedPasswordList.Remove(pwd);

                localStorage.Values.Remove(CreateKeyValuePair(pwd));
            }
            else
            {
                Debug.WriteLine("selected item was not a Password object");
            }
        }

        public static void RemovePasswordList()
        {
            cachedPasswordList = null;
            localStorage.Values.Clear();
            cloudStorage.Values.Clear();
        }

        public static void Convert()
        {
            List<Password> passwords = new List<Password>();

            //object converted;
            //localStorage.Values.TryGetValue("converted", out converted);

            //if (converted != null && (bool)converted)
            //{
            //    return;
            //}

            foreach (KeyValuePair<string, object> pair in localStorage.Values)
            {
                Guid result;
                Guid.TryParse(pair.Key, out result);

                if (result != Guid.Empty)
                {
                    try
                    {
                        var password = JsonConvert.DeserializeObject<Password>(pair.Value.ToString());
                        password.Key = password.Title;
                        password.KeyGuid = null;
                        passwords.Add(password);

                        localStorage.Values.Remove(pair);
                    }
                    catch (Exception)
                    {
                        // Ignore the bad ones, because it could be something else.
                    }
                }
            }

            foreach (KeyValuePair<string, object> pair in cloudStorage.Values)
            {
                Guid result;
                Guid.TryParse(pair.Key, out result);

                if (result != Guid.Empty)
                {
                    try
                    {
                        var password = JsonConvert.DeserializeObject<Password>(pair.Value.ToString());
                        password.Key = password.Title;
                        password.KeyGuid = null;

                        if (!passwords.Exists(pass => pass.Key == password.Key))
                        {
                            passwords.Add(password);
                            cloudStorage.Values.Remove(pair);
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore the bad ones, because it could be something else.
                    }
                }
            }

            var counter = 0;
            foreach (var password in passwords)
            {
                counter++;
                var keyValuePair = CreateKeyValuePair(password);

                localStorage.Values.Add(keyValuePair);
                cloudStorage.Values.Add(keyValuePair);
            }

            Debug.WriteLine("Fixed " + counter + " passwords to both storage");

            //localStorage.Values.Add("converted", true);
        }

        private static KeyValuePair<string, object> CreateKeyValuePair(Password password)
        {
            return new KeyValuePair<string, object>(password.Key, JsonConvert.SerializeObject(password));
        }
    }
}
