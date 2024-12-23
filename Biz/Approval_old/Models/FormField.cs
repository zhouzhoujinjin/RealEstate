using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Models
{
  public class FormField
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Title { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ValueType { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ControlType { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool Required { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, object> ControlOptions { get; set; }
  }

  public class BlockField: FormField
  {
    public int MinCount { get; set; }
    public int MaxCount { get; set; }

    public ICollection<FormField> Children { get; set; }
  }

  public class BackTimeForm
  {
    public DateTime BackTime { get; set; }
  }

  public class OvertimeFinishDate
  {
    public int ItemId { get; set; }
    public DateTime FinishDate { get; set; }
  }
}
