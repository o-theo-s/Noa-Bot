using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Noa.Helpers
{
    [Serializable]
    public class CardMetaData
    {
        [JsonProperty]
        public CardInfo CardInfo { get; set; }
        [JsonProperty]
        public string Course { get; set; }
        [JsonProperty]
        public DateTime? ScheduleDate { get; set; }
        [JsonProperty]
        public string Chapter { get; set; }
        [JsonProperty]
        public int? Exercise { get; set; }
    }

    [JsonObject]
    public class CardInfo
    {
        [JsonProperty]
        public string Card { get; set; }
        [JsonProperty]
        public string Action { get; set; }
        [JsonProperty]
        public List<string> ChildCards { get; set; }
    }
}
