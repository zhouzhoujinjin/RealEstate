using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Approval.Entities;
using Approval.Utils;
using CyberStone.Core.Models;
using Newtonsoft.Json;


namespace Approval.Models
{
  public class ApprovalItemModel
  {
    
    public int? Id { get; set; }
    
    public string Code { get; set; }
    
    public string Title { get; set; }
    
    public int TemplateId { get; set; }
    
    public string TemplateTitle { get; set; }
    
    public string TemplateName { get; set; }
    
    public long? CreatorId { get; set; }
    
    public DateTime? CreatedTime { get; set; }
    
    public DateTime LastUpdateTime { get; set; }
    
    public ApprovalItemStatus Status { get; set; }
    
    public TemplateGroup? TemplateGroup { get; set; }
    
    public ICollection<ApprovalFlowNode> Nodes { get; set; }
    
    public User Creator { get; set; }
    
    public Dictionary<string, string> Content { get; set; }
    
    public ApprovalTemplateModel Template { get; set; }
    
    public bool IsFinal { get; set; }
    
    public List<AttachFile> FinalFiles { get; set; }
    
    public List<AttachFile> VerifiedFiles { get; set; }
    
    public bool IsPublished { get; set; }
    
    public string PublishType { get; set; }
    
    public List<string> Purview { get; set; }
    
    public bool IsUpdate { get; set; }
    
    public string PublishDepartment { get; set; }
    
    public string PublishTitle { get; set; }
    
    public DateTime? PublishTime { get; set; }
    
    public List<Department> Departments { get; set; }
    
    public string ApprovalMsg { get; set; }
    
    public bool IsRead { get; set; }


    public ApprovalItemModel()
    {
      Nodes = new List<ApprovalFlowNode>();
      FinalFiles = new List<AttachFile>();
      VerifiedFiles = new List<AttachFile>();
    }
  }
}
