using Newtonsoft.Json;
using PureCode.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Models
{
  public class ApprovalNodeModel
  {
    public int Id { get; set; }
    public long UserId { get; set; }

    public int ItemId { get; set; }
    //审批节点类型
    public ApprovalFlowNodeType NodeType { get; set; }

    public ApprovalActionType ActionType { get; set; }

    public DateTime CreatedTime { get; set; }

    public DateTime LastUpdatedTime { get; set; }

    public string Comment { get; set; }
    public List<BriefComment> Comments { get; set; }

    public List<AttachFile> Attachments { get; set; }

    public BriefUser User { get; set; }

    public List<long> NextApprovalUserIds { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<ApprovalNodeModel> Children { get; set; }

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
