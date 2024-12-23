using Approval.Interface;
using CyberStone.Core.Models;
using System.Text.Json.Serialization;

namespace Approval.Models
{
  /// <summary>
  /// 根据数据库的 ApprovalNode 生成的节点，用来显示具体审批的信息，不是用于模板相关处理的
  /// </summary>
  public class ApprovalFlowNode : IApprovalFlowNode
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
    public virtual ApprovalActionType ActionType { get; set; }

    [JsonIgnore]
    public IApprovalFlowNode Previous { get; set; }
    [JsonIgnore]
    public IApprovalFlowNode Next { get; set; }

    public Dictionary<ApprovalActionType, List<string>>? Hooks { get; set; }

    [JsonPropertyName("previousId")]
    public int? PreviousId { get; set; }
    [JsonPropertyName("nextId")]
    public int? NextId { get; set; }
    [JsonPropertyName("lastUpdatedTime")]
    public DateTime? LastUpdatedTime { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }
    [JsonPropertyName("isCurrentPendingNode")]
    public bool IsCurrentPendingNode { get; set; }
    [JsonPropertyName("comments")]
    public List<BriefComment> Comments { get; set; }
  }

  public class LogicApprovalFlowNode : ApprovalFlowNode, ILogicApprovalFlowNode
  {

    private ApprovalActionType approvalActionType;
    [JsonPropertyName("actionType")]
    public override ApprovalActionType ActionType
    {
      get
      {
        if (approvalActionType != ApprovalActionType.Created) return approvalActionType;

        if (NodeType == ApprovalFlowNodeType.And && Children.Any(x => x.ActionType == ApprovalActionType.Rejected))
          return ApprovalActionType.Rejected;
        if (NodeType == ApprovalFlowNodeType.And && Children.Any(x => x.ActionType == ApprovalActionType.Pending))
          return ApprovalActionType.Pending;
        if (NodeType == ApprovalFlowNodeType.And && Children.Any(x => x.ActionType == ApprovalActionType.Created))
          return ApprovalActionType.Created;
        if (NodeType == ApprovalFlowNodeType.Or && Children.Any(x => x.ActionType == ApprovalActionType.Approved))
        {
          return ApprovalActionType.Approved;
        }
        //或签全部为拒绝时操作类型为拒绝
        if (NodeType == ApprovalFlowNodeType.Or && Children.Count(x => x.ActionType == ApprovalActionType.Rejected) == Children.Count)
        {
          return ApprovalActionType.Rejected;
        }
        //或签全部为待审时操作类型为待审
        if (NodeType == ApprovalFlowNodeType.Or && Children.Count(x => x.ActionType == ApprovalActionType.Pending) == Children.Count)
        {
          return ApprovalActionType.Pending;
        }
        return ApprovalActionType.Created;
      }
      set
      {
        approvalActionType = value;
      }
    }
    [JsonPropertyName("children")]
    public ICollection<IApprovalFlowNode> Children { get; set; }
  }
}
