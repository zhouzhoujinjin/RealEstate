using Approval.Utils;
using Newtonsoft.Json;
using PureCode.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Approval.Models
{
  public class ConditionField
  {

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("defaultChecked")]
    public bool DefaultChecked { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
  }

  /// <summary>
  /// 审批模板中的各个节点
  /// </summary>
  public abstract class Node
  {
    [JsonProperty("type")]
    public virtual string Type { get; set; }
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("content")]
    public string Content { get; set; }
    [JsonProperty("nextNode")]
    public FlowNode NextNode { get; set; }
    [JsonProperty("userIds")]
    public virtual List<long> UserIds { get; set; }
    public Node()
    {
      UserIds = new List<long>();
    }
  }

  public class ConditionNode : Node
  {
    public override string Type => Consts.FlowNodeTypeCondition;

    [JsonProperty("conditions")]
    public Dictionary<string, string> Conditions { get; set; }
  }

  [JsonConverter(typeof(FlowNodeJsonConverter))]
  public abstract class FlowNode : Node
  {

    [JsonProperty("conditionNodes")]
    public IEnumerable<ConditionNode> ConditionNodes { get; set; }
  }

  public class ApprovalNode : FlowNode
  {
    public override string Type => Consts.FlowNodeTypeApproval;
    [JsonProperty("assigneeType")]
    public string AssigneeType { get; set; }
    [JsonProperty("users")]
    public List<BriefUser> Users { get; set; }
    [JsonProperty("userIds")]
    public override List<long> UserIds
    {
      get
      {
        return Users?.Select(x => x.Id).ToList();
      }
    }

    [JsonProperty("counterSign")]
    public bool CounterSign { get; set; }
  }

  public class StartNode : FlowNode
  {
    [JsonProperty("applicants")]
    public DepartmentsAndUsers Applicants { get; set; }
    public override string Type => "start";
  }

  public class CarbonCopyNode : FlowNode
  {
    public override string Type => Consts.FlowNodeTypeCarbonCopy;

    [JsonProperty("users")]
    public List<BriefUser> Users { get; set; }
    [JsonProperty("userIds")]
    public override List<long> UserIds
    {
      get
      {
        return Users?.Select(x => x.Id).ToList();
      }
    }
  }

  public class Workflow
  {
    [JsonProperty("startNode")]
    public StartNode StartNode { get; set; }
  }
}
