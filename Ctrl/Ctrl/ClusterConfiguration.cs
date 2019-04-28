using Newtonsoft.Json;

public class ClusterConfiguration
{
    [JsonProperty("machines")]
    public NodeDescriptor[] NodesDescriptor { get; set; }
}