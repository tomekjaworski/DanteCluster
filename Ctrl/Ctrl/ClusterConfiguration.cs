using Newtonsoft.Json;

public class ClusterConfiguration
{
    [JsonProperty("machines")]
    public Node[] Nodes { get; set; }
}