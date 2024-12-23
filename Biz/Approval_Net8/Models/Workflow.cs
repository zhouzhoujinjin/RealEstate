using Approval_Net8.Utils;
using CyberStone.Core.Models;
using System.Text.Json.Serialization;

namespace Approval.Models
{
  public class ConditionField
  {

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("defaultChecked")]
    public bool DefaultChecked { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
  }

  /// <summary>
  /// 审批模板中的各个节点
  /// </summary>
  public abstract class Node
  {
    [JsonPropertyName("type")]
    public virtual string Type { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("nextNode")]
    public FlowNode NextNode { get; set; }
    [JsonPropertyName("userIds")]
    public virtual List<long> UserIds { get; set; }
    public Node()
    {
      UserIds = new List<long>();
    }
  }

  [JsonConverter(typeof(NodeJsonConverter))]
  public class ConditionNode : Node
  {
    public override string Type => Consts.FlowNodeTypeCondition;

    [JsonPropertyName("conditions")]
    public Dictionary<string, string> Conditions { get; set; }
  }

  [JsonConverter(typeof(NodeJsonConverter))]
  public abstract class FlowNode : Node
  {

    [JsonPropertyName("conditionNodes")]
    public List<ConditionNode>? ConditionNodes { get; set; }

    [JsonPropertyName("hooks")]
    public Dictionary<ApprovalActionType, List<string>>? Hooks { get; set; }
  }

  [JsonConverter(typeof(NodeJsonConverter))]
  public class ApprovalNode : FlowNode
  {
    public override string Type => Consts.FlowNodeTypeApproval;
    [JsonPropertyName("assigneeType")]
    public string AssigneeType { get; set; }
    [JsonPropertyName("users")]
    public List<User> Users { get; set; }
    [JsonPropertyName("userIds")]
    public override List<long> UserIds
    {
      get
      {
        return Users?.Select(x => x.Id).ToList();
      }
    }

    [JsonPropertyName("counterSign")]
    public bool CounterSign { get; set; }
  }

  [JsonConverter(typeof(NodeJsonConverter))]
  public class StartNode : FlowNode
  {
    [JsonPropertyName("applicants")]
    public DepartmentsAndUsers Applicants { get; set; }
    public override string Type => "start";
  }

  [JsonConverter(typeof(NodeJsonConverter))]
  public class CarbonCopyNode : FlowNode
  {
    public override string Type => Consts.FlowNodeTypeCarbonCopy;

    [JsonPropertyName("users")]
    public List<User> Users { get; set; }
    [JsonPropertyName("userIds")]
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
    [JsonPropertyName("startNode")]
    public StartNode StartNode { get; set; }
  }
}
