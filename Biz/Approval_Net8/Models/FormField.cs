using System.Text.Json.Serialization;

namespace Approval.Models
{
  public class FormField
  {
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("valueType")]
    public string ValueType { get; set; }
    [JsonPropertyName("controlType")]
    public string ControlType { get; set; }
    [JsonPropertyName("required")]
    public bool Required { get; set; }
    [JsonPropertyName("controlOptions")]
    public Dictionary<string, object> ControlOptions { get; set; }
  }

  public class BlockField: FormField
  {
    [JsonPropertyName("minCount")]
    public int MinCount { get; set; }
    [JsonPropertyName("maxCount")]
    public int MaxCount { get; set; }

    [JsonPropertyName("children")]
    public ICollection<FormField> Children { get; set; }
  }

  public class BackTimeForm
  {
    [JsonPropertyName("backTime")]
    public DateTime BackTime { get; set; }
  }

  public class OvertimeFinishDate
  {
    [JsonPropertyName("itemId")]
    public int ItemId { get; set; }
    [JsonPropertyName("finishDate")]
    public DateTime FinishDate { get; set; }
  }
}
