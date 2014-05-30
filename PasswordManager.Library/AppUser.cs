using Newtonsoft.Json;

namespace PasswordManager.Library
{
    public class AppUser
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "DeviceId")]
        public string DeviceId { get; set; }

        [JsonProperty(PropertyName = "SerializedPassword")]
        public string SerializedPassword { get; set; }
    }
}
