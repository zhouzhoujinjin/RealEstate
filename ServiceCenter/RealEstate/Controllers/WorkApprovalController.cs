using Approval;
using Approval.Managers;
using Approval.Models;
using CyberStone.Core;
using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RealEstate.Models;
using RealEstate.Utils;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Letian.Controllers
{
  [Route("wechat/approval")]
  [ApiController]
  public class WorkApprovalController : ControllerBase
  {
    private readonly ApprovalFlowManager flowManager;
    private readonly ApprovalManager approvalManager;
    private readonly DepartmentManager departmentManager;
    private readonly UserManager userManager;
    private readonly TemplateManager templateManager;
    private readonly ValueSpaceMap valueSpaceMap;
    //private readonly WeChatWorkApi weChatWorkApi;
    private WorkDomainOptions workDomainOptions;

    public WorkApprovalController(
      ApprovalFlowManager flowManager,
      ApprovalManager approvalManager,
      DepartmentManager departmentManager,
      UserManager userManager,
      TemplateManager templateManager,
      ValueSpaceMap valueSpaceMap,
      //WeChatWorkApi weChatWorkApi, 
      IOptions<WorkDomainOptions> workDomainOptionsAccessor)
    {
      this.flowManager = flowManager;
      this.approvalManager = approvalManager;
      this.departmentManager = departmentManager;
      this.userManager = userManager;
      this.templateManager = templateManager;
      this.valueSpaceMap = valueSpaceMap;
      //this.weChatWorkApi = weChatWorkApi;
      this.workDomainOptions = workDomainOptionsAccessor.Value;
    }

    [HttpGet("templateNames", Name = "审批模板名称")]
    public async Task<AjaxResp<Dictionary<string, string>>> GetTemplateNames()
    {
      var entities = await templateManager.GetUserTemplates(null);
      var models = entities.ToDictionary(x => x.Name, x => x.Title);
      return new AjaxResp<Dictionary<string, string>>
      {
        Data = models
      };
    }

    [HttpGet("templates", Name = "用户可提交审批模板列表")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp<IEnumerable<ApprovalTemplateModel>>> GetTemplates()
    {
      var userId = HttpContext.GetUserId();
      var User = await approvalManager.GetUserDepartment(userId);
      var entities = await templateManager.GetUserTemplates(User);
      var models = entities.Select(x => new ApprovalTemplateModel
      {
        Id = x.Id,
        Name = x.Name,
        Title = x.Title,
        IconUrl = x.IconUrl,
        Description = x.Description,
        GroupCode = x.Group,
        Fields = x.Fields,
        GroupTitle = valueSpaceMap.GetTitleByNameKey("approvalTemplateGroup", x.Group.ToString("d"))
      });
      return new AjaxResp<IEnumerable<ApprovalTemplateModel>>
      {
        Data = models
      };
    }

    [HttpGet("account-departments", Name = "获取用户所在的部门列表")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp<IEnumerable<DepartmentUser>>> GetAccountDepartments()
    {
      var userId = HttpContext.GetUserId();
      var departments = await departmentManager.GetUserDepartmentsAsync(userId);
      var user = await userManager.GetBriefUserAsync(userId);
      var userDepts = departments.Select(x => new DepartmentUser
      {
        UserName = user.UserName,
        Id = user.Id,
        Profiles = new Dictionary<string, object>
        {
          { "departmentName",x.Title },
          { "departmentId",x.DepartmentId}
        }
      }).ToList();

      return new AjaxResp<IEnumerable<DepartmentUser>>
      {
        Data = userDepts
      };
    }

    [HttpGet("users", Name = "获取用户列表")]
    public async Task<AjaxResp<IEnumerable<DepartmentUser>>> GetUsers()
    {

      var users = await userManager.FindUsersAsync(new Dictionary<string, string>(), 0, 999);
      var departmentUsers = new List<DepartmentUser>();
      foreach (var user in users)
      {
        var userProfiles = (await approvalManager.GetUserDepartment(user.Id)).Profiles;
        departmentUsers.Add(new DepartmentUser
        {
          Id = user.Id,
          UserName = user.UserName,
          Profiles = userProfiles
        });
      }

      return new AjaxResp<IEnumerable<DepartmentUser>>
      {
        Data = departmentUsers
      };
    }

    [HttpGet("approval-users", Name = "获取过滤用户列表")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp<IEnumerable<User>>> GetApprovalUsers(string query = null)
    {
      var userId = HttpContext.GetUserId();
      var filters = JsonSerializer.Deserialize<Dictionary<string, string>>(query ?? "{}");
      var users = await approvalManager.GetApprovalUsers(userId, filters);
      return new AjaxResp<IEnumerable<User>>
      {
        Data = users
      };
    }

    [HttpGet("counts", Name = "审批 - 待审批的数量")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp<int>> GetCounts()
    {
      var userId = HttpContext.GetUserId();
      var result = await approvalManager.GetUserApplyApprovalCounts(userId);
      return new AjaxResp<int>
      {
        Data = result
      };
    }

    [HttpGet("items", Name = "获取用户申请历史")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<PagedAjaxResp<ApprovalItemModel>> GetUserHistoryItemsAsync(string query = null, int page = 1, int size = 10)
    {

      var userId = HttpContext.GetUserId();
      var filters = JsonSerializer.Deserialize<Dictionary<string, string>>(query ?? "{}");
      var (total, items) = await approvalManager.GetUserHistoryItems(userId, filters, page, size);
      return PagedAjaxResp<ApprovalItemModel>.Create(items, total, page, size);
    }

    /// <summary>
    /// 获取用户提交的全部审批流
    /// </summary>    
    /// <param name="query">search</param>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    [HttpGet("approvals", Name = "获取用户参与的审批申请")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<PagedAjaxResp<ApprovalItemModel>> GetUserApprovalsAsync(string query = null, int page = 1, int size = 10)
    {
      var userId = HttpContext.GetUserId();
      var filters = JsonSerializer.Deserialize<Dictionary<string, string>>(query ?? "{}");
      if (filters.ContainsKey("actionType"))
      {
        if (filters["actionType"] == "done")
        {
          filters["notCreatorId"] = userId.ToString();
        }
      }

      var (total, items) = await approvalManager.GetUserApprovals(userId, filters, page, size);
      return PagedAjaxResp<ApprovalItemModel>.Create(items, total, page, size);
    }

    [HttpGet("items/{itemId}", Name = "获取审批及流程节点信息")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp<ApprovalItemModel>> GetItemAsync(int itemId)
    {
      var userId = HttpContext.GetUserId();
      //判断是否是抄送用于已读，王总排除
      if (userId != 9)
      {
        var code = await approvalManager.UpdateNodeCcAsync(itemId, userId);
        if (!string.IsNullOrEmpty(code))
        {
          var sendUser = await userManager.GetBriefUserAsync(userId);
          await UpdateCardStatusAsync(code, sendUser.UserName, "抄送消息");
        }
      }
      var result = await approvalManager.GetItemInfo(itemId, userId);
      return new AjaxResp<ApprovalItemModel>
      {
        Data = result
      };
    }

    [HttpPost("{templateName}/{type}", Name = "创建审批")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp<IEnumerable<ApprovalFlowNode>>> CreateAsync([FromRoute] string templateName, [FromRoute] string type, [FromBody] Dictionary<string, string> values)
    {
      var userId = HttpContext.GetUserId();
      var user = await approvalManager.GetUserDepartment(userId);
      var template = (await templateManager.GetTemplateByName(templateName));
      if (template == null)
      {
        return new AjaxResp<IEnumerable<ApprovalFlowNode>>
        {
          Code = 403,
          Message = "找不到流程"
        };
      }
      var title = approvalManager.GetTitle(values);
      var model = new ApprovalItemModel
      {
        Code = NumberUtility.CreateApprovalCode(),
        Title = string.IsNullOrEmpty(title) ? $"{user.Profiles["FullName"]}创建的 {template.Title} 申请审批({DateTime.Today:MM-dd})" : title,
        Status = type == "submit" ? ApprovalItemStatus.Approving : ApprovalItemStatus.Draft,
        Content = values,
        IsUpdate = type != "submit"
      };
      var item = await approvalManager.CreateApprovalItem(model.Code, model.Title,
        template.Id,
        templateName,
        userId,
        model.Content,
        model.Status,
        model.IsUpdate);
      model.Content["creatorId"] = userId.ToString();//用于判断创建人条件
      model.Content["createDepartmentId"] = user.Profiles["departmentIds"].ToString();//用于判断创建人部门条件
      var flowNodes = await flowManager.BuildFlowAsync(template.Id, model.Content, item.Id);
      var approvalNodes = await approvalManager.CreateApprovalNodes(flowNodes, userId, item.Id, 0);

      if (model.Status == ApprovalItemStatus.Approving)
      {
        var url = $"{workDomainOptions.BaseUrl}/pages/info/info?id={item.Id}";
        //将审批流程从初始状态，改为开始审批
        var sendUsers = await approvalManager.StartApproval(item.Id);
        //发送消息给直接
        var creator = await userManager.GetBriefUserAsync(userId);
        // slobber: ignore
        // await weChatWorkApi.SendTextCardMessageAsync("approval", creator.UserName, "", "", $"您收到了一条关于 {item.Title} 的新消息", "提及到了您", url, "点击查看");
        //发送消息给当前待审批的用户
        if (sendUsers != null)
        {
          await SendTemplateCard(creator, sendUsers, item.Id, item.Title, DateTime.Now.ToString("yyyy-MM-dd"), url);
        }
      }
      return new AjaxResp<IEnumerable<ApprovalFlowNode>>
      {
        Data = approvalNodes.Select(x => new ApprovalFlowNode
        {
          Id = x.Id,
          ItemId = x.ItemId,
          ActionType = x.ActionType,
          UserId = x.UserId,
          PreviousId = x.PreviousId,
          NextId = x.NextId
        }).ToList()
      };
    }

    [HttpPut("{templateName}/{itemId}/{type}", Name = "修改审批")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp<ApprovalItemModel>> UpdateAsync([FromRoute] string templateName, [FromRoute] int itemId, [FromRoute] string type, [FromBody] Dictionary<string, string> values)
    {
      var userId = HttpContext.GetUserId();
      var user = await approvalManager.GetUserDepartment(userId);
      var template = (await templateManager.GetTemplateByName(templateName));
      if (template == null)
      {
        return new AjaxResp<ApprovalItemModel>
        {
          Code = 403,
          Message = "找不到流程"
        };
      }
      //判断是否已经启动流程
      var isApporve = await approvalManager.IsApprovalStart(itemId);
      var title = approvalManager.GetTitle(values);
      var model = new ApprovalItemModel
      {
        Id = itemId,
        Title = string.IsNullOrEmpty(title) ? $"{user.Profiles["FullName"]}创建的 {template.Title} 申请审批({DateTime.Today:MM-dd})" : title,
        Status = type == "submit" ? ApprovalItemStatus.Approving : ApprovalItemStatus.Draft,
        Content = values,
        IsUpdate = type != "submit"
      };
      var item = await approvalManager.UpdateItemInfo(model);
      //判断流程是否存在，不存在则生成
      var existNode = await approvalManager.ExistNodeAsync(itemId);
      if (!existNode)
      {
        model.Content["creatorId"] = userId.ToString();//用于判断创建人条件
        model.Content["createDepartmentId"] = user.Profiles["departmentIds"].ToString();//用于判断创建人部门条件
        var flowNodes = await flowManager.BuildFlowAsync(template.Id, model.Content, itemId);
        var approvalNodes = await approvalManager.CreateApprovalNodes(flowNodes, userId, itemId, int.Parse(values["oldId"]));

      }
      if (model.Status == ApprovalItemStatus.Approving)
      {
        var creator = await userManager.GetBriefUserAsync(userId);
        var url = $"{workDomainOptions.BaseUrl}/pages/info/info?id={itemId}";
        if (!isApporve)
        {
          //将审批流程从初始状态，改为开始审批
          var sendUsers = await approvalManager.StartApproval(item.Id);
          //发送消息给直接
          //await weChatWorkApi.SendTextMessageAsync("approval", creator.UserName, "", "", "您已成功创建申请并等待审批通知");
          //发送消息给当前待审批的用户
          //if (sendUsers != null)
          //{
          //  await SendTemplateCard(creator, sendUsers, itemId, model.Title, item.CreatedTime.ToString("yyyy-MM-dd"), url);
          //}
        }
        else
        {
          //向最近的审批人发消息
          var sendUsers = await approvalManager.GetPendingUsers(itemId);
          if (sendUsers != null)
          {
            await SendTemplateCard(creator, sendUsers, itemId, model.Title, item.CreatedTime.ToString("yyyy-MM-dd"), url);
          }
        }
      }
      return new AjaxResp<ApprovalItemModel>
      {
        Data = model
      };
    }

    [HttpPut("{itemId}/nodes/self/next", Name = "审批通过-提交一下审批")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp<ApprovalItemModel>> UpdateSelfApprovingItemAsync([FromRoute] int itemId, [FromBody] List<ApprovalNodeModel> originalList)
    {
      var userId = HttpContext.GetUserId();
      //to-do 查找当前节点
      var currentNode = await approvalManager.FindUserCurrentNode(itemId, userId);
      if (currentNode == null)
      {
        return new AjaxResp<ApprovalItemModel>
        {
          Code = 403,
          Message = "等待您的环节修改后续审批人"
        };
      }
      //to-do 插入新增审批流
      var item = await approvalManager.InsertNodes(originalList, itemId, currentNode.Id);
      //to-do 审批通过-如果为start节点启动流程 如果为approval节点 更新当前节点为通过并更新后续节点
      if (currentNode.NodeType == ApprovalFlowNodeType.Start)
      {
        await approvalManager.StartApproval(itemId);
      }
      else
      {
        //审批通过当前节点更新后续节点
        currentNode.ActionType = ApprovalActionType.Approved;
        currentNode.LastUpdatedTime = DateTime.Now;
        approvalManager.UpdateNextNodes(item, currentNode);
      }
      if (item.Status == ApprovalItemStatus.Draft)
      {
        item.Status = ApprovalItemStatus.Approving;
      }
      item.LastUpdatedTime = DateTime.Now;
      await approvalManager.UpdateItem(item, ApprovalActionType.Pending);

      //获取当前流程待审批的用户并发送消息
      var sendUsers = await approvalManager.GetPendingUsers(item);
      if (sendUsers != null)
      {
        var url = $"{workDomainOptions.BaseUrl}/pages/info/info?id={item.Id}";
        var creator = await userManager.GetBriefUserAsync(item.CreatorId);
        await SendTemplateCard(creator, sendUsers, item.Id, item.Title, item.CreatedTime.ToString("yyyy-MM-dd"), url);
      }

      return new AjaxResp<ApprovalItemModel>
      {
        Data = new ApprovalItemModel
        {
          Id = item.Id,
          Status = item.Status
        }
      };
    }

    [HttpPut("{itemId}/nodes/{nodeId}/update", Name = "审批节点")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ClaimNames.ApiPermission)]
    public async Task<AjaxResp<ApprovalNodeModel>> UpdateNodeInfoAsync(int itemId, int nodeId, [FromBody] ApprovalNodeModel node)
    {
      var userId = HttpContext.GetUserId();
      var user = await approvalManager.GetUserDepartment(userId);
      var approval = await approvalManager.GetItem(itemId);
      var majorNode = approval.Nodes.FirstOrDefault(x => x.Id == nodeId);
      var currentNode = majorNode;

      //如果当前节点为会签或者或签的主节点，则当前节点要转成属于自己的子节点
      if (currentNode.NodeType == ApprovalFlowNodeType.And || currentNode.NodeType == ApprovalFlowNodeType.Or)
      {
        var majorId = majorNode.Id;
        currentNode = approval.Nodes.FirstOrDefault(x => x.PreviousId == majorId && x.UserId == userId && x.ActionType == ApprovalActionType.Pending);
      }

      //var canComment = await approvalManager.CheckUserCanComment(userId, majorNode);
      var canComment = true; //所有人可以评论

      //判断是否有评论内容以及用户是否可以评论该节点
      node.Comment = node.Comment.Replace("<p><br></p>", "");
      if (!string.IsNullOrEmpty(node.Comment))
      {
        if (canComment)
        {
          majorNode.Comments.Add(new BriefComment
          {
            Content = node.Comment,
            CreatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            UserId = userId,
            UserAvatar = user.Profiles.ContainsKey("Avatar") ? user.Profiles["Avatar"].ToString() : "",
            UserFullName = user.Profiles.ContainsKey("FullName") ? user.Profiles["FullName"].ToString() : "暂无",
            UserName = user.Profiles.ContainsKey("Pinyin") ? user.Profiles["Pinyin"].ToString() : "暂无"
          });
          majorNode.Comments = majorNode.Comments.Select(x => x).ToList();
          majorNode.LastUpdatedTime = DateTime.Now;
          //是否评论涉及提及用户
          string pattern = @"data-mention-id=""([\d]+)""";
          var regex = new Regex(pattern);
          var mc = regex.Matches(node.Comment);
          var mentionUserIds = mc.Select(x => long.Parse(x.Groups[1].Value)).ToList();
          var mentionUsers = await userManager.GetBriefUsersAsync(mentionUserIds);
          // slobber: ignore
          //await weChatWorkApi.SendTextCardMessageAsync(
          //  "approval",
          //  toUser: string.Join("|", mentionUsers.Select(x => x.UserName)),
          //  title: $"您收到了一条关于 {approval.Title} 的新消息",
          //  description: "提及到了您",
          //  url: $"{workDomainOptions.BaseUrl}/pages/info/info?id={itemId}",
          //  btntxt: "点击查看");

        }
      }

      //判断是否为评论，不是则更新下一步流程
      if (node.ActionType != ApprovalActionType.Comment)
      {
        //如果没有当前用户的子节点 或者 如果 actionType 不是仅评论节点 且 用户不是本人 返回错误
        if (currentNode == null)
        {
          return new AjaxResp<ApprovalNodeModel>
          {
            Data = null
          };
        }
        currentNode.ActionType = node.ActionType;
        var nextNode = approvalManager.UpdateNextNodes(approval, currentNode);
        currentNode.LastUpdatedTime = DateTime.Now;
        majorNode.LastUpdatedTime = DateTime.Now;
      }

      await approvalManager.UpdateItem(approval, node.ActionType);

      //修改当前审批人的消息按钮状态
      if (!string.IsNullOrEmpty(currentNode.ResponseCode))
      {
        var currentApporvalUser = await userManager.GetBriefUserAsync(currentNode.UserId);
        await UpdateCardStatusAsync(currentNode.ResponseCode, currentApporvalUser.UserName, "审批消息");
      }

      //通知流程创建人当前审批结果
      var url = $"{workDomainOptions.BaseUrl}/pages/info/info?id={approval.Id}";
      var creator = await userManager.GetBriefUserAsync(approval.CreatorId);
      string creatorMsg = "";
      creatorMsg = node.ActionType switch
      {
        ApprovalActionType.Approved => $"{user.Profiles["FullName"]} 同意了您创建的审批",
        ApprovalActionType.Rejected => $"{user.Profiles["FullName"]} 拒绝了您创建的审批",
        ApprovalActionType.Comment => $"{user.Profiles["FullName"]} 对您创建的审批进行了评论",
        _ => "暂无内容",
      };
      // slobber: ignore
      // await weChatWorkApi.SendTextCardMessageAsync("approval", creator.UserName, "", "", $"您收到了一条待审批的消息", $"{approval.Title} --{creatorMsg}", url, "点击查看详情");

      //获取当前流程待审批的用户并发送消息
      var sendUsers = await approvalManager.GetPendingUsers(approval);
      if (sendUsers != null && node.ActionType != ApprovalActionType.Comment)
      {
        await SendTemplateCard(creator, sendUsers, itemId, approval.Title, approval.CreatedTime.ToString("yyyy-MM-dd"), url);
      }

      //如果流程已经全部完结，发送消息给抄送人,王总排除
      var ccUsers = (await approvalManager.GetItemCcUsers(itemId)).Where(x => x.Id != 9);
      if (ccUsers.Count() > 0)
      {
        await SendTemplateCard(creator, ccUsers, itemId, approval.Title, approval.CreatedTime.ToString("yyyy-MM-dd"), url, "抄送消息");
      }

      return new AjaxResp<ApprovalNodeModel>
      {
        Data = new ApprovalNodeModel
        {
          Id = majorNode.Id,
          ItemId = majorNode.ItemId,
          ActionType = majorNode.ActionType,
          //Attachments = currentNode.Attachments,
          Comments = majorNode.Comments,
          User = user
        }
      };
    }

    [HttpPost("trans/{nodeId}", Name = "转办")]
    public async Task<AjaxResp<ApprovalNodeModel>> TransNodeAsync(int nodeId, [FromBody] TransInfo trans)
    {
      var currentNode = await approvalManager.GetNode(nodeId);
      var currentItem = currentNode.Item;
      //验证可否进行转办
      if (currentNode.ActionType != ApprovalActionType.Pending || currentNode.NodeType != ApprovalFlowNodeType.Approval || currentItem.Status != ApprovalItemStatus.Approving)
      {
        return new AjaxResp<ApprovalNodeModel> { Data = null };
      }

      var user = await approvalManager.GetUserDepartment(trans.UserId);
      var userName = user.Profiles.ContainsKey("FullName") ? user.Profiles["FullName"].ToString() : "未知";
      var t = trans.Comment.Replace("<p><br></p>", "");
      var comment = string.IsNullOrEmpty(t) ? $"向{userName}发起转办" : t;

      var currentUser = await approvalManager.GetUserDepartment(currentNode.UserId);
      var currentName = currentUser.Profiles.ContainsKey("FullName") ? currentUser.Profiles["FullName"].ToString() : "未知";

      //插入新转办节点
      var result = await approvalManager.InsertTransNode(nodeId, trans.UserId, comment, currentUser);
      //同意当前节点      
      currentNode.ActionType = ApprovalActionType.Approved;
      currentNode.LastUpdatedTime = DateTime.Now;
      approvalManager.UpdateNextNodes(currentItem, currentNode);
      //给转办人发送消息
      var sendUsers = await approvalManager.GetPendingUsers(currentItem);
      if (sendUsers != null)
      {
        var url = $"{workDomainOptions.BaseUrl}/pages/info/info?id={currentItem.Id}";
        var creator = await userManager.GetBriefUserAsync(currentItem.CreatorId);
        await SendTemplateCard(creator, sendUsers, currentItem.Id, currentItem.Title, currentItem.CreatedTime.ToString("yyyy-MM-dd"), url, "转办消息");
      }
      return new AjaxResp<ApprovalNodeModel>
      {
        Data = new ApprovalNodeModel
        {
          Id = result.Id,
          UserId = result.UserId
        }
      };
    }

    [HttpPut("items/{itemId}/{nodeId}/isupdate", Name = "退改")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp> IsUpdateAsync([FromRoute] int itemId, [FromRoute] int nodeId, [FromBody] ApprovalNodeModel node)
    {
      //添加一条默认的评论
      var userId = HttpContext.GetUserId();
      var user = await approvalManager.GetUserDepartment(userId);
      await approvalManager.IsUpdateCommentAsync(nodeId, user, node.Comment);
      var result = await approvalManager.IsUpdateAsync(itemId);
      //发消息给创建人
      var creator = await approvalManager.GetCreator(itemId);
      if (creator != null)
      {
        var name = user.Profiles.ContainsKey("FullName") ? user.Profiles["FullName"].ToString() : "未知";
        var content = $"{name} 退改了 {result} 的申请";
        // slobber: ignore
        // await weChatWorkApi.SendTextCardMessageAsync("approval", creator.UserName, "", "", "您收到一条需要退改的消息，不需要撤回，直接修改即可。", content, $"{workDomainOptions.BaseUrl}/pages/info/info?id={itemId}", "点击查看详情");
      }
      return new AjaxResp { Data = true };
    }

    [HttpPost("items/copy", Name = "微信 - 申请详情 - 再提交")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp<ApprovalItemModel>> CreateCopyApproval([FromBody] int itemId)
    {
      var userId = HttpContext.GetUserId();
      var user = await approvalManager.GetUserDepartment(userId);
      var code = NumberUtility.CreateApprovalCode();
      var entity = await approvalManager.CreateCopyItem(itemId, userId, code);
      //if (entity != null)
      //{
      //  entity.Content["creatorId"] = userId.ToString();//用于判断创建人条件
      //  entity.Content["createDepartmentId"] = user.Profiles["departmentIds"].ToString();//用于判断创建人部门条件
      //  var flowNodes = await flowManager.BuildFlowAsync(entity.TemplateId, entity.Content, entity.Id);
      //  await approvalManager.CreateApprovalNodes(flowNodes, userId, entity.Id);
      //}
      var template = await templateManager.GetTemplateById(entity.TemplateId);

      return new AjaxResp<ApprovalItemModel>
      {
        Data = new ApprovalItemModel
        {
          Id = entity.Id,
          Title = entity.Title,
          Content = entity.Content,
          TemplateName = template.Name,
          TemplateTitle = template.Title
        }
      };
    }

    [HttpPut("items/{itemId}/recall", Name = "微信 - 审批 - 撤回(已经提交审批的流程发起人发现错误主动撤回)")]
    public async Task<AjaxResp> RecallAsync([FromRoute] int itemId)
    {
      var result = await approvalManager.RecallApprovalAsync(itemId);
      if (result != null && result.Count > 0)
      {
        foreach (var code in result)
        {
          var user = await userManager.GetBriefUserAsync(code.UserId);
          await UpdateCardStatusAsync(code.ResponseCode, user.UserName, "审批消息");
        }
      }
      return new AjaxResp { Data = true };
    }

    [HttpGet("items/{itemId}/press", Name = "微信 - 审批 - 催办")]
    public async Task<AjaxResp> PressAsync([FromRoute] int itemId)
    {
      //获得当前申请待审批人员
      var sendUsers = await approvalManager.GetPendingUsers(itemId);
      var approval = await approvalManager.GetItem(itemId);
      if (sendUsers != null)
      {
        var url = $"{workDomainOptions.BaseUrl}/pages/info/info?id={itemId}";
        var creator = await userManager.GetBriefUserAsync(approval.CreatorId);
        await SendTemplateCard(creator, sendUsers, itemId, approval.Title, approval.CreatedTime.ToString("yyyy-MM-dd"), url);
      }
      return new AjaxResp { Data = true };
    }

    [HttpGet("items/notice/exist", Name = "微信 - 公告 - 是否有新的公告")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp> IsNewPublishItemsAsync()
    {
      var userId = HttpContext.GetUserId();
      var result = await approvalManager.IsNewPublishItemsAsync(userId);
      return new AjaxResp { Data = result };
    }

    [HttpGet("items/notice", Name = "微信 - 公告 - 公告列表")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp> ListPublishItemsAsync()
    {
      var userId = HttpContext.GetUserId();
      var result = await approvalManager.ListPublishItemsAsync(userId, workDomainOptions.WebUrl);
      return new AjaxResp { Data = result };
    }

    [HttpPut("items/notice/read/{itemId}", Name = "微信 - 公告 - 已读")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp> ReadItemAsync([FromRoute] int itemId)
    {
      var userId = HttpContext.GetUserId();
      var result = await approvalManager.ReadItemAsync(itemId, userId);
      return new AjaxResp { Data = result };
    }

    /// <summary>
    /// 发送消息给当前待审批的用户
    /// </summary>
    private async Task SendTemplateCard(User creator, IEnumerable<User> sendUsers, int itemId, string title, string approvalDate, string url, string sourceDesc = "审批消息")
    {
      //构造审批消息
      //var userName = creator != null && creator.Profiles.ContainsKey("FullName") ? creator.Profiles["FullName"].ToString() : "未知";
      //var content = $"发起人：{userName}  发起日期：{approvalDate}";
      //foreach (var sendUser in sendUsers)
      //{
      //  var card = approvalManager.GetTemplateButtonCard(title, content, url, sourceDesc);
      //  var res = await weChatWorkApi.SendTemplateCardButtonMessageAsync("approval", card, sendUser.UserName, "", "");
      //  if (res != null && res.ResponseCode != null)
      //  {
      //    //将ResponseCode保存到数据库
      //    var code = await approvalManager.UpdateResponseCodeAsync(itemId, sendUser.Id, res.ResponseCode, sourceDesc);
      //    if (!string.IsNullOrEmpty(code))//返回原有的ResponseCode值，进行已处理操作
      //    {
      //      await UpdateCardStatusAsync(code, sendUser.UserName, sourceDesc);
      //    }
      //  }
      //}
    }

    //操作后修改按钮状态
    private async Task UpdateCardStatusAsync(string code, string userName, string sourceDesc)
    {
      var buttonText = sourceDesc.Equals("抄送消息") ? "已读" : "已审";
      var updateButton = new Dictionary<string, string> { { "replace_name", buttonText } };
      //await weChatWorkApi.UpdateTemplateCardAsync("approval", code, updateButton, userName, "", "");
    }


    [HttpGet("{templateName}/departmentIds", Name = "微信 - 审批 - 获取部门")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResp> Get([FromRoute] string templateName)
    {
      var userId = HttpContext.GetUserId();
      var result = await templateManager.GetDepartmentIdsAsync(userId, templateName);
      return new AjaxResp { Data = result };
    }


  }
}
