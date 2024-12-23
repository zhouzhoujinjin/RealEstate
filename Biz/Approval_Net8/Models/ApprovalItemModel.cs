using CyberStone.Core.Models;
using System.Text.Json.Serialization;

namespace Approval.Models
{
  public class ApprovalItemModel
  {
    [JsonPropertyName("id")]
    public int? Id { get; set; }
    [JsonPropertyName("code")]
    public string Code { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("templateId")]
    public long TemplateId { get; set; }
    [JsonPropertyName("templateTitle")]
    public string TemplateTitle { get; set; }
    [JsonPropertyName("templateName")]
    public string TemplateName { get; set; }
    [JsonPropertyName("creatorId")]
    public long? CreatorId { get; set; }
    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }
    [JsonPropertyName("lastUpdateTime")]
    public DateTime LastUpdateTime { get; set; }
    [JsonPropertyName("status")]
    public ApprovalItemStatus Status { get; set; }
    [JsonPropertyName("templateGroup")]
    public TemplateGroup? TemplateGroup { get; set; }
    [JsonPropertyName("nodes")]
    public ICollection<ApprovalFlowNode> Nodes { get; set; }
    [JsonPropertyName("creator")]
    public User Creator { get; set; }
    [JsonPropertyName("content")]
    public Dictionary<string, string> Content { get; set; }
    [JsonPropertyName("template")]
    public ApprovalTemplateModel Template { get; set; }
    [JsonPropertyName("isFinal")]
    public bool IsFinal { get; set; }
    [JsonPropertyName("finalFiles")]
    public List<AttachFile> FinalFiles { get; set; }
    [JsonPropertyName("verifiedFiles")]
    public List<AttachFile> VerifiedFiles { get; set; }
    [JsonPropertyName("isPublished")]
    public bool IsPublished { get; set; }
    [JsonPropertyName("publishType")]
    public string PublishType { get; set; }
    [JsonPropertyName("purview")]
    public List<string> Purview { get; set; }
    [JsonPropertyName("isUpdate")]
    public bool IsUpdate { get; set; }
    [JsonPropertyName("publishDepartment")]
    public string PublishDepartment { get; set; }
    [JsonPropertyName("publishTitle")]
    public string PublishTitle { get; set; }
    [JsonPropertyName("publishTime")]
    public DateTime? PublishTime { get; set; }
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }
    [JsonPropertyName("approvalMsg")]
    public string ApprovalMsg { get; set; }
    [JsonPropertyName("isRead")]
    public bool IsRead { get; set; }


    public ApprovalItemModel()
    {
      Nodes = new List<ApprovalFlowNode>();
      FinalFiles = new List<AttachFile>();
      VerifiedFiles = new List<AttachFile>();
    }
  }
}
