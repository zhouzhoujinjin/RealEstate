using CyberStone.Core.Models;
using System.Text.Json.Serialization;

namespace RealEstate.Models
{
  public class Config : GlobalSettings
  {
    [JsonPropertyName("openStartDate")]
    public DateTime? OpenStartDate { get; set; }
    [JsonPropertyName("openEndDate")]
    public DateTime? OpenEndDate { get; set; }
  }
}
