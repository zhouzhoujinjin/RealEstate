using System.Text.Json.Serialization;

namespace Approval.Models
{
  public class ApprovalTemplateModel
  {
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("iconUrl")]
    public string IconUrl { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("groupCode")]
    public TemplateGroup GroupCode { get; set; }
    [JsonPropertyName("groupTitle")]
    public string GroupTitle { get; set; }

    [JsonPropertyName("fields")]
    public IEnumerable<FormField> Fields { get; set; }

    [JsonPropertyName("summaryFields")]
    public List<string> SummaryFields { get; set; }

    [JsonPropertyName("isCustomFlow")]
    public bool IsCustomFlow { get; set; }
    [JsonPropertyName("conditionFields")]
    public List<ConditionField> ConditionFields { get; set; }

    [JsonPropertyName("javascriptHooks")]
    public Dictionary<string, string> JavascriptHooks { get; set; } = new Dictionary<string, string>();

    [JsonPropertyName("workflow")]
    public Workflow Workflow { get; set; }
    [JsonPropertyName("departmentIds")]
    public List<int> DepartmentIds { get; set; } = new List<int>();
  }
}
