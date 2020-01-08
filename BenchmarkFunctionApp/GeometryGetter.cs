using Newtonsoft.Json;

namespace BenchmarkFunctionApp
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GeometryGetter
    {
        [JsonProperty("value")] public int CurrentOffset { get; set; }

        public void Reset()
        {
            CurrentOffset = 0;
        }
    }
}