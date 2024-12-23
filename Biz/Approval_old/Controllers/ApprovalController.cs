using Approval.Managers;
using Approval.Models;
using Approval.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PureCode.Identity;
using PureCode.Managers;
using PureCode.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Approval.Controllers
{
  [Route("api/approval", Name = "审批工作流")]
  [ApiController]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ClaimTypes.Permission)]
  public class ApprovalController : ControllerBase
  {
    private readonly ApprovalFlowManager flowManager;
    private readonly ApprovalManager approvalManager;
    private readonly TemplateManager templateManager;
    private readonly ValueSpaceMap valueSpaceMap;
    private readonly ProfileManager profileManager;


    public ApprovalController(
      ApprovalFlowManager flowManager,
      ApprovalManager approvalManager,
      TemplateManager templateManager,
      ValueSpaceMap valueSpaceMap,
      ProfileManager profileManager)
    {
      this.flowManager = flowManager;
      this.approvalManager = approvalManager;
      this.templateManager = templateManager;
      this.valueSpaceMap = valueSpaceMap;
      this.profileManager = profileManager;
    }

    [HttpGet("templates", Name = "模板列表")]
    public async Task<AjaxResponse<IEnumerable<ApprovalTemplateModel>>> GetTemplates()
    {
      var entities = await templateManager.GetUserTemplates(0);
      var models = entities.Select(x => new ApprovalTemplateModel
      {
        Id = x.Id,
        Name = x.Name,
        Title = x.Title,
        IconUrl = x.IconUrl,
        Description = x.Description,
        GroupCode = x.Group,
        GroupTitle = valueSpaceMap.GetTitleByNameKey("approvalTemplateGroup", x.Group)
      });
      return new AjaxResponse<IEnumerable<ApprovalTemplateModel>>
      {
        Data = models
      };
    }

    [HttpPost("create/leave", Name = "创建请假审批流")]
    public async Task<AjaxResponse<IEnumerable<ApprovalFlowNode>>> CreateLeaveAsync([FromBody] LeaveModel leave)
    {
      var userId = HttpContext.GetUserId();
      var formValues = leave.GetType()
          .GetProperties(BindingFlags.Instance | BindingFlags.Public)
          .ToDictionary(prop => prop.Name.ConvertFirstLetter(LetterType.Lower)
          , prop => prop.GetValue(leave, null)?.ToString());
      var item = await approvalManager.CreateApprovalItem(leave.Title,
        leave.TemplateId,
        userId,
        formValues,
        leave.Status);
      if (leave.Status == ApprovalItemStatus.Draft) return new AjaxResponse<IEnumerable<ApprovalFlowNode>> { Data = null };
      var flowNodes = await flowManager.BuildFlowAsync(leave.TemplateId,
        formValues,
        item.Id);
      var approvalNodes = await approvalManager.CreateApprovalNodes(flowNodes, userId, item.Id);
      return new AjaxResponse<IEnumerable<ApprovalFlowNode>>
      {
        Data = approvalNodes.Select(x => new ApprovalFlowNode
        {
          ItemId = x.ItemId,
          ActionType = x.ActionType,
          UserId = x.UserId,
          PreviousId = x.PreviousId,
          NextId = x.NextId
        }).ToList()
      };
    }

    [HttpGet("items", Name = "获取用户提交的全部审批流")]
    public async Task<AjaxResponse<IEnumerable<ApprovalItemModel>>> GetUserApprovalFlowNodesAsync()
    {
      var userId = HttpContext.GetUserId();
      var items = await approvalManager.GetUserApprovalItemNodes(userId);
      List<ApprovalItemModel> approvalModels = new List<ApprovalItemModel>();
      foreach (var item in items)
      {
        var model = new ApprovalItemModel()
        {
          CreatorId = item.CreatorId,
          CreatedTime = item.CreatedTime,
          LastUpdateTime = item.LastUpdatedTime,
          Status = item.Status,
          Title = item.Title,
          TemplateGroup = item.Template.Group,
          TemplateId = item.TemplateId,
          Nodes = item.Nodes.Select(x => new ApprovalFlowNode
          {
            ActionType = x.ActionType,
            ItemId = x.ItemId,
            NodeType = x.NodeType,
            NextId = x.NextId,
            PreviousId = x.PreviousId
          }).ToList()
        };
        approvalModels.Add(model);
      }
      return new AjaxResponse<IEnumerable<ApprovalItemModel>>
      {
        Data = approvalModels
      };
    }

    [HttpPut("{itemId}/nodes/{nodeId}/update", Name = "修改审批节点状态")]
    public async Task<AjaxResponse<ApprovalNodeModel>> UpdateNodeInfoAsync(int itemId, int nodeId, [FromBody] ApprovalNodeModel node)
    {
      var approval = await approvalManager.GetItem(itemId);
      var currentNode = approval.Nodes.FirstOrDefault(x => x.Id == nodeId);
      currentNode.ActionType = node.ActionType;

      var nextNode = approvalManager.UpdateNextNodes(approval, currentNode);

      currentNode.Attachments = node.Attachments;
      if (!string.IsNullOrEmpty(node.Comment))
      {
        // 可以通过profileManager获得briefUser 填充briefComment 
        var userId = HttpContext.GetUserId();
        var profiles = await profileManager.GetUserProfilesAsync(userId, new string[] { "avatar", "fullName", "pinyin" });
        currentNode.Comments = new List<BriefComment>();
        currentNode.Comments.Add(new BriefComment
        {
          Content = node.Comment,
          CreatedTime = DateTime.Now,
          UserId = userId,
          UserAvatar = profiles.ContainsKey("avatar") ? profiles["avatar"].ToString() : "",
          UserFullName = profiles.ContainsKey("fullName") ? profiles["fullName"].ToString() : "暂无",
          UserName = profiles.ContainsKey("pinyin") ? profiles["pinyin"].ToString() : "暂无"
        });
      }
      currentNode.LastUpdatedTime = DateTime.Now;

      await approvalManager.UpdateItem(approval);
      return new AjaxResponse<ApprovalNodeModel>
      {
        Data = new ApprovalNodeModel
        {
          ActionType = currentNode.ActionType,
          Attachments = currentNode.Attachments,
          Comments = currentNode.Comments
        }
      };
    }

    [HttpGet("items/history", Name = "获取用户申请历史")]
    public async Task<AjaxResponse<IEnumerable<ApprovalItemModel>>> GetUserHistoryAsync()
    {
      var userId = HttpContext.GetUserId();
      var result = await approvalManager.GetUserFinishedItems(userId);
      return new AjaxResponse<IEnumerable<ApprovalItemModel>>
      {
        Data = result
      };
    }

    [HttpGet("items/pending", Name = "获取用户待处理")]
    public async Task<AjaxResponse<IEnumerable<ApprovalItemModel>>> GetUserPendingAsync()
    {
      var userId = HttpContext.GetUserId();
      var result = await approvalManager.GetUserPendingItems(userId);
      return new AjaxResponse<IEnumerable<ApprovalItemModel>>
      {
        Data = result
      };
    }

    [HttpGet("items/approved", Name = "获取用户已处理")]
    public async Task<AjaxResponse<IEnumerable<ApprovalItemModel>>> GetUserApprovedAsync()
    {
      var userId = HttpContext.GetUserId();
      var result = await approvalManager.GetUserApprovedItems(userId);
      return new AjaxResponse<IEnumerable<ApprovalItemModel>>
      {
        Data = result
      };
    }

    [HttpGet("items/cc", Name = "获取用户抄送")]
    public async Task<AjaxResponse<IEnumerable<ApprovalItemModel>>> GetUserCCAsync()
    {
      var userId = HttpContext.GetUserId();
      var result = await approvalManager.GetUserCCItems(userId);
      return new AjaxResponse<IEnumerable<ApprovalItemModel>>
      {
        Data = result
      };
    }

    [HttpGet("items/{itemId}",Name ="获取审批及流程节点信息")]
    public async Task<AjaxResponse<ApprovalItemModel>> GetItemAsync(int itemId)
    {
      var result = await approvalManager.GetItemInfo(itemId);
      return new AjaxResponse<ApprovalItemModel>
      {
        Data = result
      };
    }
  }
}
