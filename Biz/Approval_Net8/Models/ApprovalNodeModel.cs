using CyberStone.Core.Models;
using System.Text.Json.Serialization;

namespace Approval.Models
{
  public class ApprovalNodeModel
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("userId")]
    public long UserId { get; set; }

    [JsonPropertyName("itemId")]
    public int ItemId { get; set; }
    //审批节点类型

    [JsonPropertyName("nodeType")]
    public ApprovalFlowNodeType NodeType { get; set; }

    [JsonPropertyName("actionType")]
    public ApprovalActionType ActionType { get; set; }

    [JsonPropertyName("createdTime")]
    public DateTime CreatedTime { get; set; }

    [JsonPropertyName("lastUpdatedTime")]
    public DateTime LastUpdatedTime { get; set; }


    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
    [JsonPropertyName("comments")]
    public List<BriefComment>? Comments { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile>? Attachments { get; set; }

    [JsonPropertyName("user")]
    public User? User { get; set; }

    [JsonPropertyName("nextApprovalUserIds")]
    public List<long>? NextApprovalUserIds { get; set; }
    [JsonPropertyName("children")]
    public List<ApprovalNodeModel>? Children { get; set; }

    public ApprovalNodeModel()
    {
      Children = new List<ApprovalNodeModel>();
    }
  }

  public class NodeUserCode
  {
    public long UserId { get; set; }
    public string ResponseCode { get; set; }
  }
}
