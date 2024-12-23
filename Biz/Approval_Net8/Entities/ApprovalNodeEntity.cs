using Approval.Models;

namespace Approval.Entities
{
  public class ApprovalNodeEntity
  {
    public int Id { get; set; }

    public long UserId { get; set; }

    public int ItemId { get; set; }

    public ApprovalItemEntity? Item { get; set; }


    public ApprovalActionType ActionType { get; set; }

    public int? PreviousId { get; set; }
    public ApprovalNodeEntity? PreviousNode { get; set; }

    public int? NextId { get; set; }

    public ApprovalNodeEntity? NextNode { get; set; }

    public ApprovalFlowNodeType NodeType { get; set; }

    public List<BriefComment> Comments { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime LastUpdatedTime { get; set; }

    public Dictionary<ApprovalActionType, List<string>>? Hooks { get; set; }

    public List<AttachFile> Attachments { get; set; }

    public bool? IsRead { get; set; }

    public int Seq { get; set; }

    public string? ResponseCode { get; set; }

    public ApprovalNodeEntity()
    {
      Attachments = new List<AttachFile>();
      Comments = new List<BriefComment>();
    }

  }
}
