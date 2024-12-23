using Approval.Models;

namespace Approval.Entities
{
  public class ApprovalItemEntity
  {
    public int Id { get; set; }

    public string Code { get; set; }

    public string Title { get; set; }

    public long TemplateId { get; set; }



    //表单提交内容
    public Dictionary<string, string> Content { get; set; }

    public string? Summary { get;set; }

    public DateTime CreatedTime { get; set; }

    public long CreatorId { get; set; }

    public DateTime LastUpdatedTime { get; set; }

    public ApprovalItemStatus Status { get; set; }

    public ICollection<ApprovalNodeEntity> Nodes { get; set; }

    public ApprovalTemplateEntity Template { get; set; }

    public List<AttachFile> FinalFiles { get; set; }

    public List<AttachFile> VerifiedFiles { get; set; }

    public bool IsPublished { get; set; }

    public string? PublishType { get; set; }

    public List<string> Purview { get; set; }

    public bool IsUpdate { get; set; }
    public string? PublishDepartment { get; set; }
    public string? PublishTitle { get; set; }
    public DateTime? PublishTime { get; set; }

    public ApprovalItemEntity()
    {
      Nodes = new List<ApprovalNodeEntity>();
      FinalFiles = new List<AttachFile>();
      VerifiedFiles = new List<AttachFile>();
    }


  }
}
