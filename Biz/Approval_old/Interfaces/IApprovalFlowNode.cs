using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Interface
{
  public interface IApprovalFlowNode
  {    
    public long UserId { get; }
    public int ItemId { get; }
    public ApprovalFlowNodeType NodeType { get; set; }
    public ApprovalActionType ActionType { get; set; }

    public IApprovalFlowNode Previous { get; set; }
    public IApprovalFlowNode Next { get; set; }
    
  }

  public interface ILogicApprovalFlowNode: IApprovalFlowNode
  {
    public ICollection<IApprovalFlowNode> Children { get; set; }

  }
}
 