using Approval.Entities;
using Approval.Interface;
using Approval.Models;
using Approval.Utils;
using Microsoft.EntityFrameworkCore;

namespace Approval.Managers
{
  public class ApprovalFlowManager
  {
    private readonly ApprovalDbContext context;

    public ApprovalFlowManager(ApprovalDbContext context)
    {
      this.context = context;
    }

    /// <summary>
    /// 获取表单字段及类型
    /// </summary>
    /// <param name="formName"></param>
    /// <returns></returns>
    public async Task<IEnumerable<FormField>> GetFormFields(string formName)
    {
      var template = await context.ApprovalTemplates.FirstOrDefaultAsync(e => e.Name == formName);
      return template.Fields;
    }

    /// <summary>
    /// 判断条件是否为等于类型
    /// eg:="yyy"
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    private string TryGetEqualExpression(string condition)
    {
      return !string.IsNullOrEmpty(condition) && condition.StartsWith("=") ? condition.Substring(1) : null;
    }

    /// <summary>
    /// 判断条件是否为数值范围类型
    /// eg: ,1为小于1,
    /// eg: 2,5为2-5
    /// eg: 3,为大于3
    /// </summary>
    /// <param name="condition">
    /// 
    /// </param>
    /// <returns></returns>
    private List<float?> TryGetNumberRangeExpression(string condition)
    {
      if (string.IsNullOrEmpty(condition))
      {
        return null;
      }
      var parts = condition.Split(",");
      if (parts.Length != 2)
      {
        return null;
      }
      var numbers = new List<float?>();
      var result = parts.Select(x =>
      {
        if (string.IsNullOrEmpty(x))
        {
          numbers.Add(null);
          return true;
        }
        var result = float.TryParse(x, out var number);
        if (result) numbers.Add(number);
        return result;
      }).Where(x => x).Count() == 2;
      if (result)
        return numbers;
      else
      {
        return null;
      }
    }

    /// <summary>
    /// 判断条件是否为日期范围类型
    /// eg:“2020-01-01,2020-05-05” 为在此日期范围之间
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    private List<DateTime?> TryGetDateTimeRangeExpression(string condition)
    {
      if (string.IsNullOrEmpty(condition))
      {
        return null;
      }
      var parts = condition.Split(",");
      if (parts.Length != 2)
      {
        return null;
      }
      var dates = new List<DateTime?>();
      var result = parts.Select(x =>
      {
        if (string.IsNullOrEmpty(x))
        {
          dates.Add(null);
          return true;
        }
        var result = DateTime.TryParse(x, out var number);
        if (result) dates.Add(number);
        return result;
      }).Where(x => x).Count() == 2;
      if (result)
        return dates;
      else
      {
        return null;
      }
    }

    /// <summary>
    /// 判断条件是否为字符串范围类型
    /// eg: in string1,string2,string3 为在3个字符串之中
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    private IEnumerable<string> TryGetInRangeExpression(string condition)
    {
      return !string.IsNullOrEmpty(condition) && condition.StartsWith("in ") ? condition.Substring(3).Split(",") : null;
    }

    /// <summary>
    /// 验证条件是否通过
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private bool isValid(string condition, string value)
    {
      var equalCondition = TryGetEqualExpression(condition);
      if (!string.IsNullOrEmpty(equalCondition))
      {
        return equalCondition == value;
      }

      var floatRangeCondition = TryGetNumberRangeExpression(condition);
      if (floatRangeCondition != null)
      {
        var result = float.TryParse(value, out float floatValue);
        //如起始边界为null或值大于起始边界则为真
        result = result && (floatRangeCondition[0] == null || floatValue >= floatRangeCondition[0]);
        //如终止边界为null或值小于起始边界则为真
        result = result && (floatRangeCondition[1] == null || floatValue < floatRangeCondition[1]);
        return result;
      }

      var dateTimeRangeCondition = TryGetDateTimeRangeExpression(condition);
      if (dateTimeRangeCondition != null)
      {
        var result = DateTime.TryParse(value, out DateTime dateTimeValue);
        result = result && (dateTimeRangeCondition[0] == null || dateTimeValue > dateTimeRangeCondition[0]);
        result = result && (dateTimeRangeCondition[1] == null || dateTimeValue < dateTimeRangeCondition[1]);
        return result;
      }
      var inRangeCondition = TryGetInRangeExpression(condition);
      if (inRangeCondition != null)
      {
        var isArray = value.Contains(",");

        if (isArray)
        {
          var arr = value.Split(",");
          var result = inRangeCondition.Intersect(arr).ToList();
          return result.Count() > 0;
        }
        else
        {
          var result = inRangeCondition.Contains(value);
          return result;
        }
      }

      return true;
    }

    /// <summary>
    /// 创建工作流节点
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="node"></param>
    /// <param name="previousNode"></param>
    /// <returns></returns>
    private IApprovalFlowNode createApprovalFlowNode(int itemId, FlowNode node, IApprovalFlowNode previousNode)
    {
      switch (node.Type)
      {
        //审批工作流类型
        case Consts.FlowNodeTypeApproval:
        case "approver":
          //节点审批人大于1人，默认为或签，
          if (node.UserIds.Count > 1)
          {
            var logicNode = new LogicApprovalFlowNode
            {
              //操作类型初始为创建，随着子节点的操作类型的改变，
              ActionType = ApprovalActionType.Created,
              //为节点的审批类型复制
              NodeType = ((ApprovalNode)node).CounterSign ? ApprovalFlowNodeType.And : ApprovalFlowNodeType.Or,
              ItemId = itemId,
              Previous = previousNode,
            };
            previousNode.Next = logicNode;
            previousNode = logicNode;
          }

          //按审批人生成审批工作流，需要判断UserIds中的是人员还是部门
          List<IApprovalFlowNode> children = null;
          if (((ApprovalNode)node).AssigneeType.Equals("director"))//审批设置人
          {
            children = node.UserIds.Select(id => new ApprovalFlowNode
            {
              ActionType = ApprovalActionType.Created,
              NodeType = ApprovalFlowNodeType.Sub,
              Hooks = node.Hooks,
              ItemId = itemId,
              UserId = id,
              Previous = previousNode
            } as IApprovalFlowNode).ToList();
          }
          else
          {
            //to-do审批设置职位
            foreach (var one in ((ApprovalNode)node).Users)
            {
              //得到每个职位所对应的人

            }
          }

          //如果单一审批人正常，否则转换为逻辑流程，并将上一个节点的子节点赋值为审批人生成的工作流
          if (children != null && children.Count == 1)
          {
            previousNode.Next = children.First();
            previousNode.Next.NodeType = ApprovalFlowNodeType.Approval;
            return children.First();
          }
          else
          {
            ((ILogicApprovalFlowNode)previousNode).Children = children;
            return previousNode;
          }
        //工作流起始节点类型
        case Consts.FlowNodeTypeStart:
          var approvalFlowNode = new ApprovalFlowNode
          {
            ActionType = ApprovalActionType.Created,
            NodeType = ApprovalFlowNodeType.Start,
            ItemId = itemId,
            Previous = previousNode,
            Hooks = node.Hooks
          };
          return approvalFlowNode;
        //抄送节点类型
        case Consts.FlowNodeTypeCarbonCopy:
          var ccChildren = node.UserIds.Select(id => new ApprovalFlowNode
          {
            ActionType = ApprovalActionType.Created,
            NodeType = ApprovalFlowNodeType.Cc,
            ItemId = itemId,
            UserId = id,
            Hooks = node.Hooks,
            Previous = previousNode
          } as IApprovalFlowNode).ToList();
          var current = previousNode;
          //创建抄送子节点的链表
          foreach (var cc in ccChildren)
          {
            current.Next = cc;
            cc.Previous = current;
            current = cc;
          }
          return ccChildren.First();
      }

      return null;
    }

    /// <summary>
    /// 创建工作流
    /// </summary>
    /// <param name="flowNode"></param>
    /// <param name="previousNode"></param>
    /// <param name="values"></param>
    /// <param name="result"></param>
    /// <param name="itemId"></param>
    private void buildFlows(FlowNode flowNode, IApprovalFlowNode previousNode, Dictionary<string, string> values, List<IApprovalFlowNode> result, int itemId)
    {
      var approvalFlowNode = createApprovalFlowNode(itemId, flowNode, previousNode);
      result.Add(approvalFlowNode);

      #region 抄送处理-遍历所有抄送人插入流程列表
      while (approvalFlowNode.Next != null)
      {
        approvalFlowNode = approvalFlowNode.Next;
        result.Add(approvalFlowNode);
      }
      #endregion

      if (flowNode.ConditionNodes != null)
      {
        foreach (var conditionNode in flowNode.ConditionNodes)
        {
          if (conditionNode.Conditions != null && conditionNode.Conditions.Count > 0)
          {
            
            var isInvalid = conditionNode.Conditions.Select(x => isValid(x.Value, values[x.Key])).Any(x => x == false);
            if (isInvalid) continue; //如所有条件不符合任何类型跳出循环
            //递归Conditions下的 NEXTNODE子节点
            buildFlows(conditionNode.NextNode, result.Last(), values, result, itemId);
          }
        }
      }
      //递归FlowNode下的 NEXTNODE子节点
      if (flowNode.NextNode != null) buildFlows(flowNode.NextNode, result.Last(), values, result, itemId);
    }

    /// <summary>
    /// 创建工作流-对外调用方法-by表单
    /// </summary>
    /// <param name="formName"></param>
    /// <param name="formValues"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async Task<List<IApprovalFlowNode>> BuildFlowAsync(long templateId, Dictionary<string, string> formValues, int itemId)
    {
      var template = await context.ApprovalTemplates.FirstOrDefaultAsync(e => e.Id==templateId);
      var model = DotSplittedKeyDictionaryToObjectConverter.Parse(formValues, TemplateUtils.GetTemplateModelType(template.Name)) as IFieldsModel;
      formValues["departmentIds"] = string.Join(',', model.Departments.Select(x => x.Id));
      return BuildFlow(template, formValues, itemId);
    }

    /// <summary>
    /// 创建工作流-对外调用方法-by模板
    /// </summary>
    /// <param name="template"></param>
    /// <param name="formValues"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public List<IApprovalFlowNode> BuildFlow(ApprovalTemplateEntity template, Dictionary<string, string> formValues, int itemId)
    {
      var workflow = template.Workflow;
      var approveNodes = new List<IApprovalFlowNode>();
      buildFlows(workflow.StartNode, null, formValues, approveNodes, itemId);
      return approveNodes;
    }
  }
}
