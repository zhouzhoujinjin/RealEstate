using Approval;
using Approval.Models;
using CyberStone.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Approval_Net8.Utils
{
  public class NodeJsonConverter : JsonConverter<Node>
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsAssignableTo(typeof(Node));
    }

    public override Node Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      using var jsonDocument = JsonDocument.ParseValue(ref reader);
      var jsonObject = jsonDocument.RootElement;

      var r = jsonObject.TryGetProperty("type", out var type);

      r = jsonObject.TryGetProperty("content", out var contentJson);
      var content = r ? contentJson.GetString() : null;
      r = jsonObject.TryGetProperty("userIds", out var userIds);
      var userIdsStr = default(string);
      if (r)
      {
        userIdsStr = userIds.GetRawText();
      }
      var userIdsObj = userIdsStr != null ? userIds.Deserialize<List<long>>(options) : new List<long>();


      r = jsonObject.TryGetProperty("title", out var title);
      var titleStr = default(string);
      if (r)
      {
        titleStr = title.GetString();
      }

      r = jsonObject.TryGetProperty("nextNode", out var nextNode);

      var nextNodeStr = r ? nextNode.GetRawText() : null;

      r = jsonObject.TryGetProperty("hooks", out var hooks);
      var hooksJson = r ? hooks.GetRawText() : null;

      switch (type.ToString())
      {
        case Consts.FlowNodeTypeApproval:
        case "approver":
          var approvalNode = new ApprovalNode
          {
            UserIds = userIdsObj,
            Content = content,
            Title = titleStr
          };
          r = jsonObject.TryGetProperty("assigneeType", out var assignType);
          approvalNode.AssigneeType = r ? assignType.GetString() : null;
          r = jsonObject.TryGetProperty("counterSign", out var counterSign);
          approvalNode.CounterSign = r ? counterSign.GetBoolean() : false;
          r = jsonObject.TryGetProperty("users", out var users);
          var usersStr = r ? users.GetRawText() : null;
          approvalNode.Users = usersStr != null ? JsonSerializer.Deserialize<List<User>>(usersStr, options) : new List<User>();
          r = jsonObject.TryGetProperty("conditionNodes", out var conditionNodes);
          var conditionNodesJson = r ? conditionNodes.GetRawText() : null;
          approvalNode.ConditionNodes = conditionNodesJson != null ? JsonSerializer.Deserialize<List<ConditionNode>>(conditionNodesJson, options) : new List<ConditionNode>();
          approvalNode.NextNode = nextNodeStr != null ? JsonSerializer.Deserialize<FlowNode>(nextNodeStr, options) : null;
          approvalNode.Hooks = hooksJson != null ? JsonSerializer.Deserialize<Dictionary<ApprovalActionType, List<string>>>(hooksJson, options) : null;

          return approvalNode;
        case Consts.FlowNodeTypeCarbonCopy:
          var ccNode = new CarbonCopyNode
          {
            Content = content,
            Title = titleStr,
          };
          r = jsonObject.TryGetProperty("users", out users);
          usersStr = r ? users.GetRawText() : null;
          ccNode.Users = usersStr != null ? JsonSerializer.Deserialize<List<User>>(usersStr, options) : new List<User>();
          r = jsonObject.TryGetProperty("conditionNodes", out conditionNodes);
          conditionNodesJson = r ? conditionNodes.GetRawText() : null;
          ccNode.ConditionNodes = conditionNodesJson != null ? JsonSerializer.Deserialize<List<ConditionNode>>(conditionNodesJson, options) : new List<ConditionNode>();
          ccNode.NextNode = nextNodeStr != null ? JsonSerializer.Deserialize<FlowNode>(nextNodeStr, options) : null;
          ccNode.Hooks = hooksJson != null ? JsonSerializer.Deserialize<Dictionary<ApprovalActionType, List<string>>>(hooksJson, options) : null;

          return ccNode;
        case Consts.FlowNodeTypeStart:
          var startNode = new StartNode
          {
            Content = content,
            Title = titleStr,

          };
          r = jsonObject.TryGetProperty("applicants", out var applicantJson);
          var applicants = r ? applicantJson.GetRawText() : null;
          startNode.Applicants = applicants != null ? JsonSerializer.Deserialize<DepartmentsAndUsers>(applicants, options) : new DepartmentsAndUsers();
          r = jsonObject.TryGetProperty("conditionNodes", out conditionNodes);
          conditionNodesJson = r ? conditionNodes.GetRawText() : null;
          startNode.ConditionNodes = conditionNodesJson != null ? JsonSerializer.Deserialize<List<ConditionNode>>(conditionNodesJson, options) : new List<ConditionNode>();
          startNode.NextNode = nextNodeStr != null ? JsonSerializer.Deserialize<FlowNode>(nextNodeStr, options) : null;

          startNode.Hooks = hooksJson != null ? JsonSerializer.Deserialize<Dictionary<ApprovalActionType, List<string>>>(hooksJson, options) : null;
          return startNode;
        case Consts.FlowNodeTypeCondition:
          var conditionNode = new ConditionNode
          {
            Title = titleStr,
            Content = content,
          }; r = jsonObject.TryGetProperty("conditionNodes", out conditionNodes);
          conditionNodesJson = r ? conditionNodes.GetRawText() : null;
          conditionNode.Conditions = conditionNodesJson != null ? JsonSerializer.Deserialize<Dictionary<string, string>>(conditionNodesJson, options) : new Dictionary<string, string>();
          conditionNode.NextNode = nextNodeStr != null ? JsonSerializer.Deserialize<FlowNode>(nextNodeStr, options) : null;
          return conditionNode;
        default:
          throw new NotImplementedException("不支持的 Node 类型");
      }
    }

    public override void Write(Utf8JsonWriter writer, Node value, JsonSerializerOptions options)
    {
      writer.WriteStartObject();
      writer.WriteString("type", value.Type);
      writer.WriteString("title", value.Title);
      writer.WriteString("content", value.Content);
      if (value.NextNode != null)
      {
        writer.WritePropertyName("nextNode");
        writer.WriteRawValue(JsonSerializer.Serialize(value.NextNode, options));
      }
      writer.WritePropertyName("userIds");
      writer.WriteRawValue(JsonSerializer.Serialize(value.UserIds, options));

      switch (value.Type)
      {
        case Consts.FlowNodeTypeCondition:
          writer.WritePropertyName("conditions");
          writer.WriteRawValue(JsonSerializer.Serialize((value as ConditionNode).Conditions, options));
          break;
        case Consts.FlowNodeTypeApproval:
          var approval = value as ApprovalNode;
          writer.WriteString("assigneeType", approval.AssigneeType);
          writer.WritePropertyName("users");
          writer.WriteRawValue(JsonSerializer.Serialize(approval.Users, options));
          writer.WriteBoolean("counterSign", approval.CounterSign);
          if (approval.ConditionNodes != null && approval.ConditionNodes.Count > 0)
          {
            writer.WritePropertyName("conditionNodes");
            writer.WriteRawValue(JsonSerializer.Serialize(approval.ConditionNodes, options));
          }
          break;
        case Consts.FlowNodeTypeCarbonCopy:
          var cc = value as CarbonCopyNode;
          writer.WritePropertyName("users");
          writer.WriteRawValue(JsonSerializer.Serialize(cc.Users, options));
          if (cc.ConditionNodes != null && cc.ConditionNodes.Count > 0)
          {
            writer.WritePropertyName("conditionNodes");
            writer.WriteRawValue(JsonSerializer.Serialize(cc.ConditionNodes, options));
          }
          break;
        case Consts.FlowNodeTypeStart:
          var start = value as StartNode;
          writer.WritePropertyName("applicants");
          writer.WriteRawValue(JsonSerializer.Serialize(start.Applicants));
          if (start.ConditionNodes != null && start.ConditionNodes.Count > 0)
          {
            writer.WritePropertyName("conditionNodes");
            writer.WriteRawValue(JsonSerializer.Serialize(start.ConditionNodes, options));
          }
          break;
      }
      writer.WriteEndObject();
    }

  }
}
