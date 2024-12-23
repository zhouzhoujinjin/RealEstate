
using System.Text.Json.Serialization;

namespace RealEstate.Models
{
  public class UploadFile
  {
    [JsonPropertyName("uid")]
    public string Uid { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("size")]
    public long Size { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
  }
}
