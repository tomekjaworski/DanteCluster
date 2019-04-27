using System.Net;
using Newtonsoft.Json;

public class Node
{
    [JsonProperty("hardware")]
    public HardwareAddress Hardware { get; set; }

    [JsonProperty("ip")]
    public IPAddress IP { get; set; }

    [JsonProperty("hostname")]
    public string Hostname { get; set; }

    [JsonProperty("location")]
    public string Location { get; set; }

}