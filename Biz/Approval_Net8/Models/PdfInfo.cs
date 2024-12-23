using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Approval.Models
{
  public class PdfInfo
  {
    [JsonPropertyName("approvalTitle")]
    public string ApprovalTitle { get; set; }
    [JsonPropertyName("companyName")]
    public string CompanyName { get; set; }
    [JsonPropertyName("dateTime")]
    public string DateTime { get; set; }
    [JsonPropertyName("userName")]
    public string UserName { get; set; }
    [JsonPropertyName("departmentName")]
    public string DepartmentName { get; set; }
    [JsonPropertyName("content")]
    public Dictionary<string, string> Content { get; set; }
    [JsonPropertyName("nodes")]
    public List<FlowInfo> Nodes { get; set; }
    [JsonPropertyName("templateId")]
    public long TemplateId { get; set; }
  }

  public class FlowInfo
  {
    [JsonPropertyName("userName")]
    public string UserName { get; set; }
    [JsonPropertyName("actionType")]
    public string ActionType { get; set; }
    [JsonPropertyName("dateTime")]
    public string DateTime { get; set; }
    [JsonPropertyName("comments")]
    public string Comments { get; set; }
  }
}
