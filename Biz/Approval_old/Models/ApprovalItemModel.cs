using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Approval.Entities;
using Approval.Utils;
using Newtonsoft.Json;
using PureCode.Models;

namespace Approval.Models
{
  public class ApprovalItemModel
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? Id { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Code { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Title { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int TemplateId { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string TemplateTitle { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string TemplateName { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public long? CreatorId { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? CreatedTime { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime LastUpdateTime { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ApprovalItemStatus Status { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TemplateGroup? TemplateGroup { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ICollection<ApprovalFlowNode> Nodes { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public BriefUser Creator { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, string> Content { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public ApprovalTemplateModel Template { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool IsFinal { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<AttachFile> FinalFiles { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<AttachFile> VerifiedFiles { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool IsPublished { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string PublishType { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Purview { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool IsUpdate { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string PublishDepartment { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string PublishTitle { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public DateTime? PublishTime { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<Department> Departments { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ApprovalMsg { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool IsRead { get; set; }


    public ApprovalItemModel()
    {
      Nodes = new List<ApprovalFlowNode>();
      FinalFiles = new List<AttachFile>();
      VerifiedFiles = new List<AttachFile>();
    }
  }
}
