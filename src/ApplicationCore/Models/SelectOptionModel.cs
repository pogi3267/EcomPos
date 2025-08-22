using Newtonsoft.Json;

namespace ApplicationCore.Models
{
    public class SelectOptionModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
