using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Models
{
  public class ApprovalTemplateModel
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int Id { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Name { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Title { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string IconUrl { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Description { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TemplateGroup GroupCode { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string GroupTitle { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IEnumerable<FormField> Fields { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool IsCustomFlow { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<ConditionField> ConditionFields { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Workflow Workflow { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<int> DepartmentIds { get; set; } = new List<int>();
  }
}
