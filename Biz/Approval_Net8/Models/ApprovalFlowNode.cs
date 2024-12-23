using Approval.Interface;
using CyberStone.Core.Models;

namespace Approval.Models
{
  /// <summary>
  /// 根据数据库的 ApprovalNode 生成的节点，用来显示具体审批的信息，不是用于模板相关处理的
  /// </summary>
  public class ApprovalFlowNode : IApprovalFlowNode
  {
    public int Id { get; set; }
    public long UserId { get; set; }

    public int ItemId { get; set; }
    //审批节点类型
    public ApprovalFlowNodeType NodeType { get; set; }

    public virtual ApprovalActionType ActionType { get; set; }

    public IApprovalFlowNode Previous { get; set; }
    public IApprovalFlowNode Next { get; set; }

    public int? PreviousId { get; set; }

    public int? NextId { get; set; }

    public DateTime? LastUpdatedTime { get; set; }

    public User User { get; set; }

    public bool IsCurrentPendingNode { get; set; }

    public List<BriefComment> Comments { get; set; }
  }

  public class LogicApprovalFlowNode : ApprovalFlowNode, ILogicApprovalFlowNode
  {

    private ApprovalActionType approvalActionType;

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
        if (NodeType == ApprovalFlowNodeType.Or && Children.Count(x => x.ActionType == ApprovalActionType.Rejected) == Children.Count())
        {
          return ApprovalActionType.Rejected;
        }
        //或签全部为待审时操作类型为待审
        if (NodeType == ApprovalFlowNodeType.Or && Children.Count(x => x.ActionType == ApprovalActionType.Pending) == Children.Count())
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

    public ICollection<IApprovalFlowNode> Children { get; set; }
  }
}
