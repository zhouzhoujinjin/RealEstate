using Approval.Entities;
using Approval.Interface;
using Approval.Models;
using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Approval.Managers
{
  public class ApprovalManager
  {
    private readonly ApprovalDbContext context;
    private readonly UserManager userManager;
    private readonly DepartmentManager departmentManager;
    private readonly UploadOptions uploadOptions;
    private readonly IDistributedCache cache;
    //private readonly ApprovalHooksManager hooksManager;

    public ApprovalManager(ApprovalDbContext context,
      UserManager userManager,
      DepartmentManager departmentManager,
      //ApprovalHooksManager hooksManager,
      IDistributedCache cache,
      IOptions<UploadOptions> uploadOptionsAccessor)
    {
      this.context = context;
      this.userManager = userManager;
      this.departmentManager = departmentManager;
      //this.hooksManager = hooksManager;
      this.cache = cache;
      this.uploadOptions = uploadOptionsAccessor.Value;
    }

    public async Task<ApprovalItemEntity> GetItem(int itemId)
    {
      return await context.ApprovalItems.Include(x => x.Template).Include(x => x.Nodes).Where(x => x.Id == itemId).FirstOrDefaultAsync();
    }

    /// <summary>
    /// 修改审批申请用于节点更新
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public async Task<ApprovalItemEntity> UpdateItem(ApprovalItemEntity item, ApprovalActionType status)
    {
      if (await IsApprovalFinished(item.Id))
      {
        //如果模板是要求传终稿，改为转终稿
        if (await IsFinal(item.TemplateId))
        {
          item.Status = ApprovalItemStatus.Upload;
        }
        else
        {
          item.Status = ApprovalItemStatus.Approved;
        }
      }
      else
      {
        //没有结束流程,同时审批为同意，这个时候需要查看一下流程主状态，务必保持为待审批，防止误操作，本来点击同意结果点成拒绝的情况
        if (status == ApprovalActionType.Approved)
        {
          item.Status = ApprovalItemStatus.Approving;
        }
      }
      if (status == ApprovalActionType.Rejected)
      {
        item.Status = ApprovalItemStatus.Rejected;
      }
      context.ApprovalItems.Update(item);
      await context.SaveChangesAsync();
      return item;
    }

    /// <summary>
    /// 修改审批申请用于开启审批流程，及修改审批申请内容
    /// </summary>
    /// <returns></returns>
    public async Task<ApprovalItemEntity> UpdateItemInfo(ApprovalItemModel item)
    {
      var entity = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == item.Id);
      entity.Title = item.Title ?? entity.Title;

      entity.Content = item.Content ?? entity.Content;
      var template = await context.ApprovalTemplates.Where(x => x.Id == item.TemplateId || x.Name == item.TemplateName).FirstAsync();
      entity.Summary = string.Join(",", entity.Content.Where(x => template.SummaryFields.Contains(x.Key)).Select(x => x.Value.ToString()));
      entity.Status = item.Status;
      entity.IsUpdate = item.IsUpdate;
      entity.LastUpdatedTime = DateTime.Now;
      context.ApprovalItems.Update(entity);
      await context.SaveChangesAsync();
      return entity;
    }

    public async Task<bool> DeleteItemAsync(int itemId)
    {
      var entity = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == itemId);
      entity.Status = ApprovalItemStatus.Cancel;
      entity.IsUpdate = false;
      entity.LastUpdatedTime = DateTime.Now;
      context.ApprovalItems.Update(entity);
      return (await context.SaveChangesAsync()) > 0;
    }

    // 判断该申请流程是否已经启动
    public async Task<bool> IsApprovalStart(int itemId)
    {
      var entity = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == itemId);
      if (entity == null) return false;
      return entity.Status != ApprovalItemStatus.Draft;
    }

    /// <summary>
    /// 启动审批流程
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<User>> StartApproval(int itemId)
    {
      var item = await context.ApprovalItems.Include(x => x.Nodes).FirstOrDefaultAsync(x => x.Id == itemId);
      var startNode = item.Nodes.FirstOrDefault(x => x.NodeType == ApprovalFlowNodeType.Start);
      if (startNode.ActionType == ApprovalActionType.Created || startNode.ActionType == ApprovalActionType.Approved)
      {
        startNode.ActionType = ApprovalActionType.Approved;
        UpdateNextNodes(item, startNode);
      }
      await UpdateItem(item, ApprovalActionType.Pending);
      var pendingUsers = await GetPendingUsers(item);
      return pendingUsers;
    }

    /// <summary>
    /// 获取当前审批待审批的用户列表
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public async Task<List<User>> GetPendingUsers(ApprovalItemEntity item)
    {
      if (item.Status != ApprovalItemStatus.Approving)
      {
        return null;
      }
      var userIds = await context.ApprovalNodes.Where(x => x.ItemId == item.Id
      && x.ActionType == ApprovalActionType.Pending
      && x.UserId != 0).Select(x => x.UserId).ToListAsync();
      var users = new List<User>();
      foreach (var userId in userIds)
      {
        var user = await userManager.GetBriefUserAsync(userId);
        if (user != null)
        {
          users.Add(user);
        }
      }
      return users;
    }

    /// <summary>
    /// 获取当前审批待审批的用户列表
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async Task<List<User>> GetPendingUsers(int itemId)
    {
      var item = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == itemId);
      if (item == null) return null;
      if (item.Status != ApprovalItemStatus.Approving)
      {
        return null;
      }
      var userIds = await context.ApprovalNodes.Where(x => x.ItemId == item.Id
      && x.ActionType == ApprovalActionType.Pending
      && x.UserId != 0).Select(x => x.UserId).ToListAsync();
      var users = new List<User>();
      foreach (var userId in userIds)
      {
        var user = await userManager.GetBriefUserAsync(userId);
        if (user != null)
        {
          users.Add(user);
        }
      }
      return users;
    }

    /// <summary>
    /// 获得创建人
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async Task<User> GetCreator(int itemId)
    {
      var item = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == itemId);
      if (item == null) return null;
      return await userManager.GetBriefUserAsync(item.CreatorId);
    }

    /// <summary>
    /// 如果申请状态为完成或待上传状态，返回抄送的人员列表
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<User>> GetItemCcUsers(int itemId)
    {
      IEnumerable<User> ts = new List<User>();
      var item = await context.ApprovalItems.Include(c => c.Nodes).FirstOrDefaultAsync(x => x.Id == itemId);
      if (item == null) return null;
      if (item.Status == ApprovalItemStatus.Approved || item.Status == ApprovalItemStatus.Upload)
      {
        if (item.Nodes != null)
        {
          var userIds = item.Nodes.Where(x => x.NodeType == ApprovalFlowNodeType.Cc).Select(x => x.UserId).ToArray();
          var users = new List<User>();
          foreach (var userId in userIds)
          {
            var user = await userManager.GetBriefUserAsync(userId);
            if (user != null)
            {
              users.Add(user);
            }
          }
          return users;
        }
        return ts;
      }
      return ts;
    }

    public async Task<List<User>> GetApprovalUsers(long creatorId, Dictionary<string, string> filters)
    {
      var query = GetApprovalUsersQuery(creatorId, filters);
      var items = await query.ToListAsync();
      var itemUserIds = items.Select(x => x.Nodes.Select(n => n.UserId)).ToList();
      var userIds = new List<long>();
      foreach (var itemUserId in itemUserIds)
      {
        userIds.AddRange(itemUserId.Select(x => x).ToList());
      }

      var users = new List<User>();

      foreach (var userId in userIds.Where(x => x != 0).Distinct())
      {
        var user = await GetUserDepartment(userId);
        users.Add(user);
      }
      return users;
    }

    private IQueryable<ApprovalItemEntity> GetApprovalUsersQuery(long currentUserId, Dictionary<string, string> filters)
    {
      var query = context.ApprovalItems.Include(x => x.Nodes).Include(x => x.Template).Where(x =>
        x.Nodes.Any(n => n.UserId == currentUserId)
        && x.Status == ApprovalItemStatus.Approving
        //&& x.CreatorId != currentUserId
        );
      if (filters.ContainsKey("templateName"))
      {
        query = query.Where(x => x.Template.Name == filters["templateName"]);
      }
      if (filters.ContainsKey("actionType"))
      {
        string actionType = filters["actionType"];
        switch (actionType)
        {
          case "pending":
            query = query.Where(x => x.Nodes.Any(n => n.ActionType == ApprovalActionType.Pending));
            break;

          case "done":
            query = query.Where(x => x.Nodes.Any(n => n.ActionType == ApprovalActionType.Approved));
            break;

          default:
            break;
        }
      }
      if (filters.ContainsKey("startDate") && !string.IsNullOrEmpty(filters["startDate"]))
      {
        var r = DateTime.TryParse(filters["startDate"], out DateTime startDate);
        if (r)
        {
          query = query.Where(x => x.CreatedTime >= startDate);
        }
      }
      if (filters.ContainsKey("endDate") && !string.IsNullOrEmpty(filters["endDate"]))
      {
        var r = DateTime.TryParse(filters["endDate"], out DateTime endDate);
        if (r)
        {
          query = query.Where(x => x.CreatedTime <= endDate);
        }
      }
      if (filters.ContainsKey("itemId"))
      {
        var r = int.TryParse(filters["itemId"], out int itemId);
        query = query.Where(x => x.Id == itemId);
      }
      return query;
    }

    public void UpdateLogicNodes(IEnumerable<ApprovalNodeEntity> children, ApprovalNodeEntity? logicNode)
    {
      // 还没到此节点的审批时机
      if (logicNode == null || logicNode.ActionType == ApprovalActionType.Created)
      {
        return;
      }

      if (logicNode.NodeType == ApprovalFlowNodeType.And)
      {
        logicNode.ActionType = children.Any(x => x.ActionType == ApprovalActionType.Rejected) ? ApprovalActionType.Rejected : children.Any(x => x.ActionType == ApprovalActionType.Pending) ? ApprovalActionType.Pending : ApprovalActionType.Approved;
      }
      else if (logicNode.NodeType == ApprovalFlowNodeType.Or)
      {
        logicNode.ActionType = children.Any(x => x.ActionType == ApprovalActionType.Approved) ? ApprovalActionType.Approved : children.Any(x => x.ActionType == ApprovalActionType.Pending) ? ApprovalActionType.Pending : ApprovalActionType.Rejected;
      }
    }

    /// <summary>
    /// 初始化创建工作流节点
    /// </summary>
    /// <param name="formName"></param>
    /// <param name="formValues"></param>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ApprovalNodeEntity>> CreateApprovalNodes(IEnumerable<IApprovalFlowNode> flowNodes, long startUserId, int itemId, int oldId)
    {
      var nodes = new List<ApprovalNodeEntity>();
      if (oldId > 0)
      {
        nodes = await context.ApprovalNodes.Where(x => x.ItemId == oldId).ToListAsync();
      }
      var entities = new List<ApprovalNodeEntity>();
      //主流程按节点顺序插入node表
      var lastNode = default(ApprovalNodeEntity);
      int index = 1;
      foreach (var node in flowNodes)
      {
        var com = nodes.Where(x => x.UserId == node.UserId).Select(x => x.Comments).FirstOrDefault();
        var entity = new ApprovalNodeEntity
        {
          ItemId = itemId,
          Comments = com != null ? com : new List<BriefComment>(),
          UserId = node.NodeType == ApprovalFlowNodeType.Start ? startUserId : node.UserId,
          ActionType = node.ActionType,
          NodeType = node.NodeType,
          CreatedTime = DateTime.Now,
          PreviousNode = lastNode,
          Hooks = node.Hooks,
          Seq = index,
          ResponseCode = ""
        };
        lastNode = entity;
        entities.Add(entity);
        //如果是逻辑工作流则将子流程遍历子节点一并插入node表
        if (node.NodeType == ApprovalFlowNodeType.And || node.NodeType == ApprovalFlowNodeType.Or)
        {
          var tempNode = (ILogicApprovalFlowNode)node;
          tempNode.Children.ForEach(x =>
          {
            var e = new ApprovalNodeEntity
            {
              ItemId = itemId,
              UserId = x.UserId,
              ActionType = x.ActionType,
              NodeType = x.NodeType,
              Hooks = x.Hooks,
              CreatedTime = DateTime.Now,
              LastUpdatedTime = DateTime.Now,
              PreviousNode = lastNode,
              Seq = index
            };
            entities.Add(e);
          });
        }
        index++;
      }
      context.ApprovalNodes.AddRange(entities);
      var result = await context.SaveChangesAsync();
      foreach (var entity in entities)
      {
        if (entity.PreviousNode != null && entity.NodeType != ApprovalFlowNodeType.Sub)
        {
          //为entity配置Next
          entity.PreviousNode.NextNode = entity;
          context.ApprovalNodes.Update(entity);
        }
      }
      result += await context.SaveChangesAsync();
      return result > 0 ? entities : null;
    }

    public string GetTitle(Dictionary<string, string> values)
    {
      string text;
      if (values.ContainsKey("docTitle"))
      {
        text = values["docTitle"];
      }
      else if (values.ContainsKey("contractTitle"))
      {
        text = values["contractTitle"];
      }
      else if (values.ContainsKey("projectTitle"))
      {
        text = values["projectTitle"];
      }
      else if (values.ContainsKey("projectName"))
      {
        text = values["projectName"];
      }
      else
      {
        text = "";
      }
      return text;
    }

    /// <summary>
    /// 创建审批信息
    /// </summary>
    /// <param name="title"></param>
    /// <param name="templateId"></param>
    /// <param name="creatorId"></param>
    /// <param name="content">前端表单提交全部内容.toDictionary(x=>x.Key,x=>x.Value)</param>
    /// <returns></returns>
    public async Task<ApprovalItemEntity> CreateApprovalItem(string code, string title, long templateId, string templateName, long creatorId, Dictionary<string, string> content, ApprovalItemStatus status, bool isUpdate)
    {
      var entity = new ApprovalItemEntity
      {
        Code = code,
        Title = title,
        TemplateId = templateId,
        CreatorId = creatorId,
        CreatedTime = DateTime.Now,
        LastUpdatedTime = DateTime.Now,
        Status = status,
        IsUpdate = isUpdate,
        Purview = new List<string>()
      };
      context.ApprovalItems.Add(entity);
      var result = await context.SaveChangesAsync();

      entity.Content = content;

      var template = await context.ApprovalTemplates.Where(x => x.Id == templateId).FirstOrDefaultAsync();
      entity.Summary = string.Join(";", content.Where(x => template.SummaryFields.Contains(x.Key)).Select(x => x.Value?.ToString()));
      context.Update(entity);
      result = await context.SaveChangesAsync();

      return result > 0 ? entity : null;
    }

    /// <summary>
    /// 检查所在节点的审批状态是否通过
    /// </summary>
    /// <param name="thisNode"></param>
    /// <returns></returns>
    public async Task<bool> CheckOtherApprovalResult(ApprovalNodeEntity thisNode)
    {
      var nodes = await context.ApprovalNodes.Where(x => x.ItemId == thisNode.ItemId && x.PreviousId == thisNode.PreviousId).ToListAsync();
      var otherNodes = nodes.Where(x => x.Id != thisNode.Id);
      var previousNode = nodes.FirstOrDefault(x => x.Id == thisNode.PreviousId);

      if (previousNode.NodeType == ApprovalFlowNodeType.And && otherNodes.Count() > 0)
      {
        return otherNodes.Select(x => x.ActionType == ApprovalActionType.Approved).All(x => true);
      }
      if (previousNode.NodeType == ApprovalFlowNodeType.Or && otherNodes.Count() > 0)
      {
        return otherNodes.Select(x => x.ActionType == ApprovalActionType.Approved).Any(x => true);
      }
      return true;
    }

    /// <summary>
    /// 更新下一个或多个审批流节点意见
    /// </summary>
    /// <param name="item"></param>
    /// <param name="currentNode"></param>
    /// <returns></returns>
    public ApprovalNodeEntity? UpdateNextNodes(ApprovalItemEntity item, ApprovalNodeEntity currentNode)
    {
      if (currentNode == null)
      {
        return null;
      }

      var nextNode = currentNode.NextNode;
      if (nextNode == null && currentNode.NodeType != ApprovalFlowNodeType.Sub)
      {
        return null;
      }

      if (currentNode.ActionType == ApprovalActionType.Approved)
      {
        if (currentNode.NodeType == ApprovalFlowNodeType.Sub)
        {
          nextNode = currentNode.PreviousNode;
          UpdateLogicNodes(item.Nodes.Where(x => x.PreviousNode == nextNode && x.NodeType == ApprovalFlowNodeType.Sub), nextNode);
          UpdateNextNodes(item, nextNode);
        }
        else if (nextNode.NodeType == ApprovalFlowNodeType.Cc)
        {
          nextNode.ActionType = ApprovalActionType.Approved;
          // 这里发， 再研究下结束时给 startnode的user 发，同意也要发消息是吧，同意时是给cc的发，其他就是在rejected，和finished时给发起人发，没有finished，没有finished你也能判断出来吧，应该可以嗯呢，钉钉流程怎么看效果
          UpdateNextNodes(item, nextNode);
        }
        else if (nextNode.NodeType == ApprovalFlowNodeType.Approval)
        {
          nextNode.ActionType = ApprovalActionType.Pending;
          // 这里发
        }
        else if (nextNode.NodeType == ApprovalFlowNodeType.And || nextNode.NodeType == ApprovalFlowNodeType.Or)
        {
          item.Nodes.Where(x => x.PreviousNode == nextNode && x.NodeType == ApprovalFlowNodeType.Sub).ForEach(x => x.ActionType = ApprovalActionType.Pending);

          nextNode.ActionType = ApprovalActionType.Pending;
          // 这里发
        }
        return nextNode;
      }
      else if (currentNode.ActionType == ApprovalActionType.Rejected)
      {
        if (currentNode.NodeType == ApprovalFlowNodeType.Sub)
        {
          nextNode = currentNode.PreviousNode;
          UpdateLogicNodes(item.Nodes.Where(x => x.PreviousNode == nextNode && x.NodeType == ApprovalFlowNodeType.Sub), nextNode);
          if (currentNode.PreviousNode?.NodeType == ApprovalFlowNodeType.And)
          {
            item.Status = ApprovalItemStatus.Rejected;
          }
        }
        return null;
      }
      else
      {
        return null;
      }
    }

    /// <summary>
    /// 获取审批及流程节点信息
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async Task<ApprovalItemModel> GetItemInfo(int itemId, long userId = 0)
    {
      var item = await context.ApprovalItems.Include(x => x.Nodes).Include(x => x.Template).FirstOrDefaultAsync(x => x.Id == itemId);
      var creator = await GetUserDepartment(item.CreatorId, false);
      var nodes = item.Nodes.Where(x => x.NodeType != ApprovalFlowNodeType.Sub)
        .OrderBy(x => x.Seq)
        .Select(
        x => x.NodeType == ApprovalFlowNodeType.And || x.NodeType == ApprovalFlowNodeType.Or
          ? new LogicApprovalFlowNode
          {
            Id = x.Id,
            ActionType = x.ActionType,
            NodeType = x.NodeType,
            Comments = x.Comments,
            UserId = x.UserId,
            NextId = x.NextId,
            PreviousId = x.PreviousId,
            ItemId = x.ItemId,
            LastUpdatedTime = x.LastUpdatedTime,
            IsCurrentPendingNode = IsCurrentNode(userId, x),
          }
          : new ApprovalFlowNode
          {
            Id = x.Id,
            ActionType = x.ActionType,
            NodeType = x.NodeType,
            Comments = x.Comments,
            UserId = x.UserId,
            NextId = x.NextId,
            PreviousId = x.PreviousId,
            ItemId = x.ItemId,
            LastUpdatedTime = x.LastUpdatedTime,
            IsCurrentPendingNode = IsCurrentNode(userId, x)
          }
      ).ToList();
      foreach (var node in nodes)
      {
        if (node.NodeType == ApprovalFlowNodeType.And || node.NodeType == ApprovalFlowNodeType.Or)
        {
          var n = (LogicApprovalFlowNode)node;
          var children = item.Nodes.Where(x => x.PreviousId == node.Id && x.NodeType == ApprovalFlowNodeType.Sub)
            .Select(x => new ApprovalFlowNode
            {
              Id = x.Id,
              ActionType = x.ActionType,
              NodeType = x.NodeType,
              Comments = x.Comments,
              UserId = x.UserId,
              LastUpdatedTime = x.LastUpdatedTime,
              IsCurrentPendingNode = IsCurrentNode(userId, x)
            }).ToList();
          foreach (var child in children)
          {
            child.User = await GetUserDepartment(child.UserId);
          }
          n.Children = children.Select(x => x as IApprovalFlowNode).ToList();
        }
        else
        {
          node.User = await GetUserDepartment(node.UserId);
        }
      }
      var itemModel = new ApprovalItemModel()
      {
        Id = item.Id,
        CreatorId = item.CreatorId,
        Creator = creator,
        Code = item.Code,
        Title = item.Title,
        Content = item.Content,
        CreatedTime = item.CreatedTime,
        Status = item.Status,
        TemplateId = item.TemplateId,
        TemplateGroup = item.Template.Group,
        TemplateTitle = item.Template.Title,
        FinalFiles = item.FinalFiles,
        VerifiedFiles = item.VerifiedFiles,
        IsUpdate = item.IsUpdate,
        Template = new ApprovalTemplateModel
        {
          IsCustomFlow = item.Template.IsCustomFlow,
          Title = item.Template.Title,
          Fields = item.Template.Fields,
          SummaryFields = item.Template.SummaryFields,
          Id = item.Template.Id,
          Name = item.Template.Name
        },
        Nodes = nodes
      };
      return itemModel;
    }

    /// <summary>
    /// 获取用户审批事项及节点信息
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<(int, IEnumerable<ApprovalItemModel>)> GetUserHistoryItems(long userId, Dictionary<string, string> filters, int page = 1, int size = 20)
    {
      var query = GetItemQuery(filters, userId);
      var total = await query.CountAsync();
      var result = await query.OrderByDescending(x => x.CreatedTime).Skip(Math.Max(page - 1, 0) * size).Take(size).Select(x => new ApprovalItemModel
      {
        Id = x.Id,
        TemplateTitle = x.Template.Title,
        TemplateName = x.Template.Name,
        TemplateGroup = x.Template.Group,
        TemplateId = x.Template.Id,
        CreatedTime = x.CreatedTime,
        CreatorId = x.CreatorId,
        LastUpdateTime = x.LastUpdatedTime,
        Title = x.Title,
        Status = x.Status,
        IsFinal = x.Template.IsFinal,
        FinalFiles = x.FinalFiles,
        IsUpdate = x.IsUpdate,
        Content = x.Content,
        Nodes = x.Nodes.Where(m => m.ActionType == ApprovalActionType.Pending).FirstOrDefault() == null
        ?
        null
        :
        new List<ApprovalFlowNode>
        {
          new ApprovalFlowNode
          {
            UserId = x.Nodes.Where(m => m.ActionType == ApprovalActionType.Pending && m.NodeType == ApprovalFlowNodeType.Approval).FirstOrDefault() != null
            ?
            x.Nodes.Where(m => m.ActionType == ApprovalActionType.Pending && m.NodeType == ApprovalFlowNodeType.Approval).FirstOrDefault().UserId
            :
            x.Nodes.Where(m => m.ActionType == ApprovalActionType.Pending && m.NodeType == ApprovalFlowNodeType.Sub).FirstOrDefault().UserId
          }
        }
      }).ToListAsync();
      foreach (var item in result)
      {
        item.Creator = await GetUserDepartment(item.CreatorId.Value, false);
        //如果是待审批获取当前审批人，完结就是完结
        if (item.Status == ApprovalItemStatus.Approving && item.Nodes != null && item.Nodes.Count > 0)
        {
          var u = item.Nodes.First().UserId > 0 ? await userManager.GetBriefUserAsync(item.Nodes.First().UserId, new string[] { "FullName" }) : null;
          item.ApprovalMsg = u != null ? u.Profiles["FullName"].ToString() : "佚名";
        }
        else
        {
          item.ApprovalMsg = "";
        }
      }
      return (total, result);
    }

    public async Task<(int, IEnumerable<ApprovalItemModel>)> GetLeaveItems(Dictionary<string, string> filters, int page = 1, int size = 20)
    {
      var query = context.ApprovalItems
        .Include(e => e.Template).Where(x => true);
      if (filters.ContainsKey("type") && filters["type"].Equals("leave"))
      {
        query = query.Where(x => x.Template.Name.Equals("leave"));
      }
      if (filters.ContainsKey("type") && filters["type"].Equals("out"))
      {
        query = query.Where(x => x.Template.Name.Equals("out"));
      }
      var temp = await query.Select(x => new ApprovalItemModel
      {
        Id = x.Id,
        Code = x.Code,
        TemplateTitle = x.Template.Title,
        TemplateName = x.Template.Name,
        TemplateGroup = x.Template.Group,
        TemplateId = x.Template.Id,
        CreatedTime = x.CreatedTime,
        CreatorId = x.CreatorId,
        Content = x.Content,
        Title = x.Title,
        Status = x.Status,
        IsFinal = x.Template.IsFinal,
        FinalFiles = x.FinalFiles
      }).ToArrayAsync();

      if (filters.ContainsKey("startDate") && !string.IsNullOrEmpty(filters["startDate"]))
      {
        temp = temp.Where(x => DateTime.Parse(x.Content["startDate"]) >= DateTime.Parse($"{filters["startDate"]} 00:00")).ToArray();
      }
      if (filters.ContainsKey("endDate") && !string.IsNullOrEmpty(filters["endDate"]))
      {
        temp = temp.Where(x => DateTime.Parse(x.Content["startDate"]) <= DateTime.Parse($"{filters["endDate"]} 23:59")).ToArray();
      }
      var total = temp.Count();
      var result = temp.OrderBy(x => x.CreatedTime).ThenBy(x => x.CreatorId).Skip(Math.Max(page - 1, 0) * size).Take(size).ToArray();
      foreach (var item in result)
      {
        item.Creator = await GetUserDepartment(item.CreatorId.Value, false);
      }
      return (total, result);
    }

    //导出请假数据
    public async Task<string> ExportLeaveData(Dictionary<string, string> filters)
    {
      var query = context.ApprovalItems
        .Include(e => e.Template).Where(x => true);
      if (filters.ContainsKey("type") && filters["type"].Equals("leave"))
      {
        query = query.Where(x => x.Template.Name.Equals("leave"));
      }
      if (filters.ContainsKey("type") && filters["type"].Equals("out"))
      {
        query = query.Where(x => x.Template.Name.Equals("out"));
      }

      var temp = await query.Select(x => new ApprovalItemModel
      {
        Id = x.Id,
        Code = x.Code,
        TemplateTitle = x.Template.Title,
        TemplateName = x.Template.Name,
        TemplateGroup = x.Template.Group,
        TemplateId = x.Template.Id,
        CreatedTime = x.CreatedTime,
        CreatorId = x.CreatorId,
        Content = x.Content,
        Title = x.Title,
        Status = x.Status,
        IsFinal = x.Template.IsFinal,
        FinalFiles = x.FinalFiles
      }).ToArrayAsync();

      if (filters.ContainsKey("startDate") && !string.IsNullOrEmpty(filters["startDate"]))
      {
        temp = temp.Where(x => DateTime.Parse(x.Content["startDate"]) >= DateTime.Parse($"{filters["startDate"]} 00:00")).ToArray();
      }
      if (filters.ContainsKey("endDate") && !string.IsNullOrEmpty(filters["endDate"]))
      {
        temp = temp.Where(x => DateTime.Parse(x.Content["startDate"]) <= DateTime.Parse($"{filters["endDate"]} 23:59")).ToArray();
      }
      var result = temp.OrderBy(x => x.CreatedTime).ThenBy(x => x.CreatorId).ToArray();
      foreach (var item in result)
      {
        item.Creator = await GetUserDepartment(item.CreatorId.Value, false);
      }

      var path = Path.Combine(uploadOptions.Path, "approval", "export");
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }
      var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
      path = Path.Combine(uploadOptions.Path, "approval", "export", $"{fileName}.xlsx");
      FileInfo file = new FileInfo(path);
      if (file.Exists)
      {
        file.Delete();
        file = new FileInfo(path);
      }

      using (OfficeOpenXml.ExcelPackage package = new OfficeOpenXml.ExcelPackage(file))
      {
        var worksheet = package.Workbook.Worksheets.Add("导出结果");
        //写表头
        string[] header = null;
        if (filters.ContainsKey("type") && filters["type"].Equals("leave"))
        {
          header = new string[] { "编号", "标题", "请假开始时间", "请假结束时间", "请假类型", "时长(小时)", "申请人", "申请时间", "状态" };
        }
        if (filters.ContainsKey("type") && filters["type"].Equals("out"))
        {
          header = new string[] { "编号", "标题", "外出预计时间", "预计返回时间", "事由", "申请人", "申请时间", "状态" };
        }
        int column = 1;
        foreach (var item in header)
        {
          worksheet.Cells[1, column].Value = item;
          worksheet.Cells[1, column].Style.Font.Bold = true;
          column++;
        }
        //
        int row = 2;
        foreach (var one in result)
        {
          worksheet.Cells[row, 1].Value = one.Code;
          worksheet.Cells[row, 2].Value = one.Title;
          if (filters.ContainsKey("type") && filters["type"].Equals("leave"))
          {
            worksheet.Cells[row, 3].Value = one.Content["startDate"];
            worksheet.Cells[row, 4].Value = one.Content["endDate"];
            worksheet.Cells[row, 5].Value = one.Content["leaveType"];
            worksheet.Cells[row, 6].Value = one.Content["days"];
            worksheet.Cells[row, 7].Value = one.Creator.Profiles["FullName"];
            worksheet.Cells[row, 8].Value = one.CreatedTime?.ToString("yyyy-MM-dd");
            worksheet.Cells[row, 9].Value = one.Status switch
            {
              ApprovalItemStatus.Draft => "草稿",
              ApprovalItemStatus.Approving => "待审批",
              ApprovalItemStatus.Approved => "通过",
              ApprovalItemStatus.Rejected => "拒绝",
              ApprovalItemStatus.Cancel => "取消",
              ApprovalItemStatus.Upload => "需要上传终稿",
              _ => "未知",
            };
          }
          if (filters.ContainsKey("type") && filters["type"].Equals("out"))
          {
            worksheet.Cells[row, 3].Value = one.Content["startDate"];
            worksheet.Cells[row, 4].Value = one.Content["returnDate"];
            worksheet.Cells[row, 5].Value = one.Content["description"];
            worksheet.Cells[row, 6].Value = one.Creator.Profiles["FullName"];
            worksheet.Cells[row, 7].Value = one.CreatedTime?.ToString("yyyy-MM-dd");
            worksheet.Cells[row, 8].Value = one.Status switch
            {
              ApprovalItemStatus.Draft => "草稿",
              ApprovalItemStatus.Approving => "待审批",
              ApprovalItemStatus.Approved => "通过",
              ApprovalItemStatus.Rejected => "拒绝",
              ApprovalItemStatus.Cancel => "取消",
              ApprovalItemStatus.Upload => "需要上传终稿",
              _ => "未知",
            };
          }

          row++;
        }
        package.Save();
      }
      return $"/uploads/approval/export/{fileName}.xlsx";
    }

    public async Task<(int, IEnumerable<ApprovalItemModel>)> GetOvertimeItems(Dictionary<string, string> filters, int page = 1, int size = 20)
    {
      var query = context.ApprovalItems
        .Include(e => e.Template)
        .Where(x => x.Template.Name.Equals("overtime") && x.CreatedTime >= DateTime.Now.AddDays(-45));
      var temp = await query.Select(x => new ApprovalItemModel
      {
        Id = x.Id,
        Code = x.Code,
        TemplateTitle = x.Template.Title,
        TemplateName = x.Template.Name,
        TemplateGroup = x.Template.Group,
        TemplateId = x.Template.Id,
        CreatedTime = x.CreatedTime,
        CreatorId = x.CreatorId,
        Content = x.Content,
        Title = x.Title,
        Status = x.Status,
        IsFinal = x.Template.IsFinal,
        FinalFiles = x.FinalFiles
      }).ToArrayAsync();

      if (filters.ContainsKey("startDate") && !string.IsNullOrEmpty(filters["startDate"]))
      {
        temp = temp.Where(x => DateTime.Parse(x.Content["startDate"]) >= DateTime.Parse($"{filters["startDate"]} 00:00")).ToArray();
      }
      if (filters.ContainsKey("endDate") && !string.IsNullOrEmpty(filters["endDate"]))
      {
        temp = temp.Where(x => DateTime.Parse(x.Content["startDate"]) <= DateTime.Parse($"{filters["endDate"]} 23:59")).ToArray();
      }
      var total = temp.Count();
      var result = temp.OrderBy(x => x.CreatorId).ThenBy(x => x.CreatedTime).Skip(Math.Max(page - 1, 0) * size).Take(size).ToArray();
      foreach (var item in result)
      {
        item.Creator = await GetUserDepartment(item.CreatorId.Value, false);
      }
      return (total, result);
    }

    //导出加班数据
    public async Task<string> ExportOvertimeData(Dictionary<string, string> filters)
    {
      var query = context.ApprovalItems
        .Include(e => e.Template)
        .Where(x => x.Template.Name.Equals("overtime") && x.CreatedTime >= DateTime.Now.AddDays(-45));
      var temp = await query.Select(x => new ApprovalItemModel
      {
        Id = x.Id,
        Code = x.Code,
        TemplateTitle = x.Template.Title,
        TemplateName = x.Template.Name,
        TemplateGroup = x.Template.Group,
        TemplateId = x.Template.Id,
        CreatedTime = x.CreatedTime,
        CreatorId = x.CreatorId,
        Content = x.Content,
        Title = x.Title,
        Status = x.Status,
        IsFinal = x.Template.IsFinal,
        FinalFiles = x.FinalFiles
      }).ToArrayAsync();

      if (filters.ContainsKey("startDate") && !string.IsNullOrEmpty(filters["startDate"]))
      {
        temp = temp.Where(x => DateTime.Parse(x.Content["startDate"]) >= DateTime.Parse($"{filters["startDate"]} 00:00")).ToArray();
      }
      if (filters.ContainsKey("endDate") && !string.IsNullOrEmpty(filters["endDate"]))
      {
        temp = temp.Where(x => DateTime.Parse(x.Content["startDate"]) <= DateTime.Parse($"{filters["endDate"]} 23:59")).ToArray();
      }
      var result = temp.OrderBy(x => x.CreatorId).ThenBy(x => x.CreatedTime).ToArray();
      foreach (var item in result)
      {
        item.Creator = await GetUserDepartment(item.CreatorId.Value, false);
      }
      var path = Path.Combine(uploadOptions.Path, "approval", "export");
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }
      var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
      path = Path.Combine(uploadOptions.Path, "approval", "export", $"{fileName}.xlsx");
      FileInfo file = new FileInfo(path);
      if (file.Exists)
      {
        file.Delete();
        file = new FileInfo(path);
      }

      using (OfficeOpenXml.ExcelPackage package = new OfficeOpenXml.ExcelPackage(file))
      {
        var worksheet = package.Workbook.Worksheets.Add("导出结果");
        //写表头
        string[] header = new string[] { "编号", "标题", "开始时间", "预计结束时间", "实际结束时间", "时长(小时)", "事由", "申请人", "申请时间", "状态" };
        int column = 1;
        foreach (var item in header)
        {
          worksheet.Cells[1, column].Value = item;
          worksheet.Cells[1, column].Style.Font.Bold = true;
          column++;
        }
        //
        int row = 2;
        foreach (var one in result)
        {
          worksheet.Cells[row, 1].Value = one.Code;
          worksheet.Cells[row, 2].Value = one.Title;
          worksheet.Cells[row, 3].Value = one.Content["startDate"];
          worksheet.Cells[row, 4].Value = one.Content["endDate"];
          worksheet.Cells[row, 5].Value = one.Content.ContainsKey("finishDate") ? one.Content["finishDate"] : "无";
          worksheet.Cells[row, 6].Value = one.Content["days"];
          worksheet.Cells[row, 7].Value = one.Content["description"];
          worksheet.Cells[row, 8].Value = one.Creator.Profiles["FullName"];
          worksheet.Cells[row, 9].Value = one.CreatedTime?.ToString("yyyy-MM-dd");
          worksheet.Cells[row, 10].Value = one.Status switch
          {
            ApprovalItemStatus.Draft => "草稿",
            ApprovalItemStatus.Approving => "待审批",
            ApprovalItemStatus.Approved => "通过",
            ApprovalItemStatus.Rejected => "拒绝",
            ApprovalItemStatus.Cancel => "取消",
            ApprovalItemStatus.Upload => "需要上传终稿",
            _ => "未知",
          };
          row++;
        }
        package.Save();
      }
      return $"/uploads/approval/export/{fileName}.xlsx";
    }

    public async Task<bool> UpdateOvertimeFinishDateAsync(int itemId, DateTime finishDate)
    {
      var entity = await context.ApprovalItems.Where(x => x.Id == itemId).FirstOrDefaultAsync();
      if (entity == null) return false;
      if (entity.Content.ContainsKey("finishDate"))
      {
        entity.Content["finishDate"] = finishDate.ToString();
      }
      else
      {
        entity.Content.Add("finishDate", finishDate.ToString());
      }
      context.ApprovalItems.Update(entity);
      return (await context.SaveChangesAsync()) > 0;
    }

    /// <summary>
    /// 获取用户参与的审批申请
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="filters"></param>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public async Task<(int, IEnumerable<ApprovalItemModel>)> GetUserApprovals(long userId, Dictionary<string, string> filters, int page = 1, int size = 20)
    {
      var query = GetNodeQuery(filters, userId);
      var total = await query.CountAsync();
      query = query.OrderByDescending(x => x.CreatedTime);
      var entities = await query.Skip(Math.Max(page - 1, 0) * size).Take(size).ToListAsync();
      var items = entities.Select(x => new ApprovalItemModel
      {
        Id = x.Item.Id,
        CreatedTime = x.Item.CreatedTime,
        CreatorId = x.Item.CreatorId,
        Title = x.Item.Title,
        TemplateGroup = x.Item.Template.Group,
        TemplateId = x.Item.TemplateId,
        TemplateTitle = x.Item.Template.Title,
        TemplateName = x.Item.Template.Name,
        Content = x.Item.Content,
        Status = x.Item.Status,
        IsUpdate = x.Item.IsUpdate
      }).GroupBy(item => item.Id).Select(item => item.First()).ToList();//去掉重复item
      foreach (var item in items)
      {
        item.Creator = await GetUserDepartment(item.CreatorId.Value, false);
      }

      return (total, items);
    }

    /// <summary>
    /// 获取用户按照模板待审批的数量
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, int>> GetUserApprovalCounts(long userId)
    {
      return await context.ApprovalNodes
        .Include(x => x.Item)
        .ThenInclude(x => x.Template)
        .Where(x => x.UserId == userId &&
           ((x.Item.Status == ApprovalItemStatus.Approving && x.ActionType == ApprovalActionType.Pending && !x.Item.IsUpdate) ||
           (x.Item.CreatorId == userId && x.Item.IsUpdate && x.Item.Status == ApprovalItemStatus.Approving)))
        .GroupBy(x => x.Item.Template.Name).Select(x => new { x.Key, Count = x.Count() })
        .ToDictionaryAsync(k => k.Key, v => v.Count);
    }

    /// <summary>
    /// 获取用户待审批的数量
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<int> GetUserApplyApprovalCounts(long userId)
    {
      return await context.ApprovalNodes
        .Include(x => x.Item)
        .ThenInclude(x => x.Template)
        .Where(x => x.UserId == userId &&
           ((x.Item.Status == ApprovalItemStatus.Approving && x.ActionType == ApprovalActionType.Pending && !x.Item.IsUpdate) ||
           (x.Item.CreatorId == userId && x.Item.IsUpdate && x.Item.Status == ApprovalItemStatus.Approving))).CountAsync();
    }

    public async Task<Dictionary<string, int>> GetUserCcCounts(long userId)
    {
      //提示30天以内的
      var startDate = DateTime.Now.AddDays(-30);

      return await context.ApprovalNodes
        .Include(x => x.Item)
        .ThenInclude(x => x.Template)
        .Where(x => x.UserId == userId
           && x.Item.Status == ApprovalItemStatus.Approved
           && x.ActionType == ApprovalActionType.Approved
           && x.NodeType == ApprovalFlowNodeType.Cc
           && x.CreatedTime >= startDate)
        .GroupBy(x => x.Item.Template.Name).Select(x => new { x.Key, Count = x.Count() })
        .ToDictionaryAsync(k => k.Key, v => v.Count);
    }

    /// <summary>
    /// 判断用户是否可以评论当前节点
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    public async Task<bool> CheckUserCanComment(long userId, ApprovalNodeEntity currentNode)
    {
      var currentNodeId = currentNode.Id;
      if (currentNode.NodeType == ApprovalFlowNodeType.And || currentNode.NodeType == ApprovalFlowNodeType.Or)
      {
        var children = await context.ApprovalNodes.Where(x => x.PreviousId == currentNode.Id
        && x.NodeType == ApprovalFlowNodeType.Sub).ToListAsync();
        currentNodeId = children.Max(x => x.Id);
      }
      var node = await context.ApprovalNodes.FirstOrDefaultAsync(x => x.Id == currentNodeId);
      var previousNodes = await context.ApprovalNodes.Where(x => x.Seq <= node.Seq && x.UserId > 0).ToListAsync();
      return previousNodes.Any(x => x.UserId == userId);
    }

    /// <summary>
    /// 获取ApprovalItem的筛选条件
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    private IQueryable<ApprovalItemEntity> GetItemQuery(Dictionary<string, string> filters, long userId = 0)
    {
      var query = context.ApprovalItems
        .Include(e => e.Template)
        .Include(e => e.Nodes)
        .Where(x => x.Status != ApprovalItemStatus.Cancel);
      if (userId != 0)
      {
        query = query.Where(x => x.CreatorId == userId);
      }
      if (filters.ContainsKey("query") && !string.IsNullOrEmpty(filters["query"]))
      {
        query = query.Where(x => x.Title.Contains(filters["query"]));
      }
      if (filters.ContainsKey("status") && !string.IsNullOrEmpty(filters["status"]))
      {
        switch (filters["status"])
        {
          case "Approving":
            query = query.Where(x => x.Status == ApprovalItemStatus.Approving);
            break;

          case "Approved":
            query = query.Where(x => x.Status == ApprovalItemStatus.Approved);
            break;

          case "Rejected":
            query = query.Where(x => x.Status == ApprovalItemStatus.Rejected);
            break;

          case "Draft":
            query = query.Where(x => x.Status == ApprovalItemStatus.Draft);
            break;

          case "Cancel":
            query = query.Where(x => x.Status == ApprovalItemStatus.Cancel);
            break;

          case "Upload":
            query = query.Where(x => x.Status == ApprovalItemStatus.Upload);
            break;

          default:
            break;
        }
      }
      if (filters.ContainsKey("templateName") && !string.IsNullOrEmpty(filters["templateName"]))
      {
        query = query.Where(x => x.Template.Name == filters["templateName"]);
      }
      if (filters.ContainsKey("startDate") && !string.IsNullOrEmpty(filters["startDate"]))
      {
        var r = DateTime.TryParse(filters["startDate"], out DateTime startDate);
        if (r)
        {
          query = query.Where(x => x.CreatedTime >= startDate);
        }
      }
      if (filters.ContainsKey("endDate") && !string.IsNullOrEmpty(filters["endDate"]))
      {
        var r = DateTime.TryParse(filters["endDate"], out DateTime endDate);
        if (r)
        {
          query = query.Where(x => x.CreatedTime <= endDate);
        }
      }

      return query;
    }

    /// <summary>
    /// 获取ApprovalNode的筛选条件
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    private IQueryable<ApprovalNodeEntity> GetNodeQuery(Dictionary<string, string> filters, long userId)
    {
      var query = context.ApprovalNodes
        .Include(x => x.Item)
        .ThenInclude(x => x.Template)
        .Where(x => x.UserId == userId);
      if (filters.ContainsKey("actionType") && !string.IsNullOrEmpty(filters["actionType"]))
      {
        var type = filters["actionType"];
        switch (type)
        {
          case "pending":
            //如果是退改应归到创建人
            query = query.Where(x => (x.ActionType == ApprovalActionType.Pending && x.Item.Status == ApprovalItemStatus.Approving && !x.Item.IsUpdate) || (x.Item.CreatorId == userId && x.Item.IsUpdate && x.Item.Status == ApprovalItemStatus.Approving));
            break;

          case "done":
            query = query.Where(x => x.ActionType == ApprovalActionType.Approved || x.ActionType == ApprovalActionType.Rejected);
            break;

          case "cc":
            var start = DateTime.Now.AddDays(-30);
            query = query.Where(x => x.ActionType == ApprovalActionType.Approved && x.NodeType == ApprovalFlowNodeType.Cc && x.CreatedTime >= start);
            break;

          default:
            break;
        }
      }
      if (filters.ContainsKey("notCreatorId"))
      {
        var r = long.TryParse(filters["notCreatorId"], out long creatorId);
        query = query.Where(x => x.Item.CreatorId != creatorId);
      }
      if (filters.ContainsKey("query") && !string.IsNullOrEmpty(filters["query"]))
      {
        query = query.Where(x => x.Item.Title.Contains(filters["query"]));
      }
      if (filters.ContainsKey("templateName") && !string.IsNullOrEmpty(filters["templateName"]))
      {
        query = query.Where(x => x.Item.Template.Name == filters["templateName"]);
      }
      if (filters.ContainsKey("userIds") && !string.IsNullOrEmpty(filters["userIds"]))
      {
        var userIds = JsonSerializer.Deserialize<List<long>>(filters["userIds"]);
        query = query.Where(x => userIds.Contains(x.Item.CreatorId));
      }
      if (filters.ContainsKey("startDate") && !string.IsNullOrEmpty(filters["startDate"]))
      {
        var r = DateTime.TryParse(filters["startDate"], out DateTime startDate);
        if (r)
        {
          query = query.Where(x => x.Item.CreatedTime >= startDate);
        }
      }
      if (filters.ContainsKey("endDate") && !string.IsNullOrEmpty(filters["endDate"]))
      {
        var r = DateTime.TryParse(filters["endDate"], out DateTime endDate);
        if (r)
        {
          query = query.Where(x => x.Item.CreatedTime >= endDate);
        }
      }

      return query;
    }

    /// <summary>
    /// 获取用户Profiles和Department信息
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<User> GetUserDepartment(long userId, bool isMajor = true)
    {
      return await cache.GetAsync($"{Consts.UserProfileWithDepartmentCacheKey}:{userId}_{(isMajor ? "OnlyMajor" : "All")}", async () =>
         {
           var user = await userManager.GetBriefUserAsync(userId);
           if (user == null)
           {
             return null;
           }
           var departments = await departmentManager.GetUserDepartmentsAsync(userId);
           var departmentIds = departments.Select(x => x.DepartmentId).ToList();
           user.Profiles["departmentIds"] = string.Join(",", departmentIds);
           if (departments.Count() > 0)
           {
             var department = departments.FirstOrDefault();
             if (isMajor)
             {
               var temp = departments.Where(x => x.IsUserMajorDepartment == isMajor).FirstOrDefault();
               department = temp != null ? temp : null;
             }
             if (department != null)
             {
               user.Profiles["departmentName"] = department.Title;
               user.Profiles["departmentId"] = department.DepartmentId;
               user.Profiles["isMajor"] = isMajor;
             }
           }
           return user;
         }, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600) });
    }

    /// <summary>
    /// 判断是否为当前节点
    /// </summary>
    /// <param name="currentUserId"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    private bool IsCurrentNode(long currentUserId, ApprovalNodeEntity node)
    {
      return node.UserId == currentUserId && node.ActionType == ApprovalActionType.Pending;
    }

    /// <summary>
    /// 判断审批申请是否完结
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    private async Task<bool> IsApprovalFinished(int itemId)
    {
      var nodes = await context.ApprovalNodes.Where(x => x.ItemId == itemId).OrderBy(x => x.Seq).ToListAsync();
      if (nodes != null && nodes.Count > 1)
      {
        var maxSeq = nodes.Max(y => y.Seq);
        var lastNode = nodes.FirstOrDefault(
        x => x.Seq == maxSeq
        && x.NodeType != ApprovalFlowNodeType.Sub
        && x.NodeType != ApprovalFlowNodeType.Start
        && x.NextId == null);
        return lastNode?.ActionType == ApprovalActionType.Approved;
      }
      else
      {
        return false;
      }
    }

    private async Task<bool> IsFinal(long templateId)
    {
      var template = await context.ApprovalTemplates.FirstOrDefaultAsync(x => x.Id == templateId);
      if (template == null) return false;
      return template.IsFinal;
    }

    /********/

    /// <summary>
    /// 插入自定义流程全部节点
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="itemId"></param>
    /// <param name="currentNodeId"></param>
    /// <returns></returns>
    public async Task<ApprovalItemEntity> InsertNodes(List<ApprovalNodeModel> nodes, int itemId, int currentNodeId)
    {
      var maxSeq = context.ApprovalNodes.Where(x => x.ItemId == itemId).Max(x => x.Seq);
      int index = maxSeq + 1;
      for (int i = 0; i < nodes.Count(); i++)
      {
        var node = nodes[i];
        //判断节点是否不为Start节点
        if (node.NodeType != ApprovalFlowNodeType.Start)
        {
          await InsertNode(node, itemId, nodes, i, index);
          index++;
        }
      }
      return await context.ApprovalItems.Include(x => x.Nodes).FirstOrDefaultAsync(x => x.Id == itemId);
    }

    /// <summary>
    /// 插入单节点流程
    /// </summary>
    /// <param name="userIds"></param>
    /// <param name="itemId"></param>
    /// <param name="currentNodeId"></param>
    /// <returns></returns>
    public async Task<ApprovalNodeEntity> InsertNode(ApprovalNodeModel node, int itemId, List<ApprovalNodeModel> nodes, int currentIndex, int seq)
    {
      var currentNode = await GetInsertingCurrentNode(itemId);
      var userIds = node.Children.Count() == 0 ? new List<long> { node.UserId } : node.Children.Select(x => x.UserId).ToList();
      if (userIds.Count() == 0)
      {
        return null;
      }
      else
      {
        var subNodeList = new List<ApprovalNodeEntity>();
        if (userIds.Count() == 1)
        {
          var nodeEntity = new ApprovalNodeEntity
          {
            NodeType = node.NodeType,
            ActionType = ApprovalActionType.Created,
            ItemId = itemId,
            PreviousId = currentNode.Id,
            CreatedTime = DateTime.Now,
            UserId = userIds.FirstOrDefault(),
            LastUpdatedTime = DateTime.Now,
            NextId = null,
            Seq = seq
          };
          context.ApprovalNodes.Add(nodeEntity);
          currentNode.NextNode = nodeEntity;
        }
        if (userIds.Count() > 1)
        {
          //主节点
          var majorNode = new ApprovalNodeEntity
          {
            PreviousId = currentNode.Id,
            NodeType = ApprovalFlowNodeType.And,
            ActionType = ApprovalActionType.Created,
            UserId = 0,
            CreatedTime = DateTime.Now,
            LastUpdatedTime = DateTime.Now,
            ItemId = itemId,
            Seq = seq
          };
          context.ApprovalNodes.Add(majorNode);
          await context.SaveChangesAsync();
          //将当前节点的下一个节点指向主节点
          currentNode.NextNode = majorNode;
          //Sub节点
          foreach (var subUserId in userIds)
          {
            var subNode = new ApprovalNodeEntity
            {
              PreviousNode = majorNode,
              NextNode = null,
              NodeType = ApprovalFlowNodeType.Sub,
              ActionType = ApprovalActionType.Created,
              CreatedTime = DateTime.Now,
              LastUpdatedTime = DateTime.Now,
              ItemId = itemId,
              UserId = subUserId,
              Seq = seq
            };
            subNodeList.Add(subNode);
          }
          if (!isExistNextApprovalNode(nodes, currentIndex))
          {
            //会签完结目标节点
            //默认目标节点为综合管理部，部门level为6，公文流转人, 6 为艾伽伊的UserId
            var targetUserId = 6;
            var targetNode = new ApprovalNodeEntity
            {
              PreviousNode = majorNode,
              ActionType = ApprovalActionType.Created,
              UserId = targetUserId,
              CreatedTime = DateTime.Now,
              LastUpdatedTime = DateTime.Now,
              NodeType = ApprovalFlowNodeType.Approval,
              ItemId = itemId,
              Seq = seq
            };
            majorNode.NextNode = targetNode;
            context.ApprovalNodes.AddRange(subNodeList);
            context.ApprovalNodes.Add(targetNode);
          }
          else
          {
            context.ApprovalNodes.AddRange(subNodeList);
          }
        }
        //更新当前节点
        currentNode.LastUpdatedTime = DateTime.Now;
        context.ApprovalNodes.Update(currentNode);
        await context.SaveChangesAsync();
        return currentNode.NextNode;
      }
    }

    private bool isExistNextApprovalNode(List<ApprovalNodeModel> nodes, int currentIndex)
    {
      int nodesLength = nodes.Count();
      for (int i = currentIndex + 1; i < nodesLength; i++)
      {
        var node = nodes[i];
        if (node.NodeType == ApprovalFlowNodeType.And || node.NodeType == ApprovalFlowNodeType.Or)
        {
          return false;
        }
        if (node.NodeType == ApprovalFlowNodeType.Approval)
        {
          return true;
        }
      }
      return false;
    }

    private async Task<ApprovalNodeEntity> GetInsertingCurrentNode(int itemId)
    {
      var item = await context.ApprovalItems.Include(x => x.Nodes).FirstOrDefaultAsync(x => x.Id == itemId);
      var maxId = item.Nodes.Where(x => x.NodeType != ApprovalFlowNodeType.Sub).Max(x => x.Id);
      return item.Nodes.First(x => x.Id == maxId);
    }

    /// <summary>
    /// 获取当前用户指定审批的当前待审批节点
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<ApprovalNodeEntity> FindUserCurrentNode(int itemId, long userId)
    {
      var item = await context.ApprovalItems.Include(x => x.Nodes).FirstOrDefaultAsync(x => x.Id == itemId);
      var nodes = item.Nodes;
      var currentNode = nodes.Where(x => x.UserId == userId && x.ActionType == ApprovalActionType.Pending).FirstOrDefault();
      if (currentNode == null && item.Nodes?.Count() == 1)
      {
        currentNode = nodes.Where(x => x.UserId == userId && x.NodeType == ApprovalFlowNodeType.Start).FirstOrDefault();
      }
      return currentNode;
    }

    /// <summary>
    /// 获得申请目前所有参与的人员
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<long>> GetItemNodesAllUsers(int itemId)
    {
      return await context.ApprovalNodes.Where(x => x.ItemId == itemId).Select(x => x.UserId).Distinct().ToArrayAsync();
    }

    /// <summary>
    /// 插入转办节点
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<ApprovalNodeEntity> InsertTransNode(int nodeId, long userId, string comment, User user)
    {
      var currentNode = await context.ApprovalNodes.Include(x => x.NextNode).FirstOrDefaultAsync(x => x.Id == nodeId);
      var currentItem = await context.ApprovalItems.Include(x => x.Nodes).FirstOrDefaultAsync(x => x.Id == currentNode.ItemId);
      var nextNode = currentNode.NextNode;
      //获得后续节点
      var behindNodes = await context.ApprovalNodes.Where(x => x.ItemId == currentNode.ItemId && x.Seq > currentNode.Seq).ToArrayAsync();
      //创建旧节点用于转办后回到当前审批人
      var oldNode = new ApprovalNodeEntity
      {
        ItemId = currentItem.Id,
        LastUpdatedTime = DateTime.Now,
        NextId = nextNode?.Id,
        NodeType = ApprovalFlowNodeType.Approval,
        UserId = currentNode.UserId,
        ActionType = ApprovalActionType.Created,
        CreatedTime = DateTime.Now,
        Seq = currentNode.Seq + 2
      };
      context.ApprovalNodes.Add(oldNode);
      await context.SaveChangesAsync();
      //创建转办节点
      var newNode = new ApprovalNodeEntity
      {
        ItemId = currentItem.Id,
        LastUpdatedTime = DateTime.Now,
        PreviousId = currentNode.Id,
        NextNode = oldNode,
        NodeType = ApprovalFlowNodeType.Approval,
        UserId = userId,
        ActionType = ApprovalActionType.Pending,
        CreatedTime = DateTime.Now,
        Seq = currentNode.Seq + 1
      };
      context.ApprovalNodes.Add(newNode);
      oldNode.PreviousNode = newNode;
      context.ApprovalNodes.Update(oldNode);
      await context.SaveChangesAsync();
      //更新当前节点为审批同意
      currentNode.NextId = newNode.Id;
      currentNode.ActionType = ApprovalActionType.Approved;
      currentNode.Comments.Add(new BriefComment
      {
        Content = comment,
        CreatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        UserId = user.Id,
        UserAvatar = user.Profiles.ContainsKey("Avatar") ? user.Profiles["Avatar"].ToString() : "",
        UserFullName = user.Profiles.ContainsKey("FullName") ? user.Profiles["FullName"].ToString() : "暂无",
        UserName = user.Profiles.ContainsKey("Pinyin") ? user.Profiles["Pinyin"].ToString() : "暂无"
      });
      currentNode.Comments = currentNode.Comments.Select(x => x).ToList();
      currentNode.LastUpdatedTime = DateTime.Now;

      context.ApprovalNodes.Update(currentNode);
      if (nextNode != null)
      {
        nextNode.PreviousId = oldNode.Id;
        context.ApprovalNodes.Update(nextNode);
      }
      await context.SaveChangesAsync();

      //修改后续节点的顺序
      if (behindNodes != null && behindNodes.Count() > 0)
      {
        foreach (var node in behindNodes)
        {
          node.Seq += 2;
        }
        context.ApprovalNodes.UpdateRange(behindNodes);
        await context.SaveChangesAsync();
      }
      return newNode;
    }

    public async Task<ApprovalNodeEntity> GetNode(int nodeId)
    {
      var currentNode = await context.ApprovalNodes.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == nodeId);
      return currentNode;
    }

    public async Task<bool> IsUpdateCommentAsync(int nodeId, User user, string comment)
    {
      var node = await context.ApprovalNodes.FirstOrDefaultAsync(x => x.Id == nodeId);
      if (node == null) return false;
      var text = comment.Equals("<p><br></p>") ? "退回修改或补充材料" : comment;
      node.Comments.Add(new BriefComment
      {
        Content = text,
        CreatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
        UserId = user.Id,
        UserAvatar = user.Profiles.ContainsKey("Avatar") ? user.Profiles["Avatar"].ToString() : "",
        UserFullName = user.Profiles.ContainsKey("FullName") ? user.Profiles["FullName"].ToString() : "暂无",
        UserName = user.Profiles.ContainsKey("Pinyin") ? user.Profiles["Pinyin"].ToString() : "暂无"
      });
      node.Comments = node.Comments.Select(x => x).ToList();
      node.LastUpdatedTime = DateTime.Now;
      context.ApprovalNodes.Update(node);
      return (await context.SaveChangesAsync()) > 0;
    }

    public async Task<string> IsUpdateAsync(int itemId)
    {
      var entity = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == itemId);
      if (entity == null) return "";
      entity.IsUpdate = true;
      context.ApprovalItems.Update(entity);
      var r = await context.SaveChangesAsync();
      return entity.Title;
    }

    /// <summary>
    /// 获取有终稿文件的全部流程
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public async Task<(int, IEnumerable<ApprovalItemModel>)> GetVerifyItems(Dictionary<string, string> filters, int page = 1, int size = 20)
    {
      var query = GetItemQuery(filters);
      var entities = await query.ToListAsync();
      var total = entities.Where(x => x.Template.IsFinal && x.FinalFiles.Count() > 0).Count();
      var items = entities.Where(x => x.Template.IsFinal && x.FinalFiles.Count() > 0)
        .OrderByDescending(x => x.CreatedTime)
        .Skip(Math.Max(page - 1, 0) * size)
        .Take(size)
        .Select(x => new ApprovalItemModel
        {
          Id = x.Id,
          TemplateTitle = x.Template.Title,
          FinalFiles = x.FinalFiles,
          Content = x.Content,
          VerifiedFiles = x.VerifiedFiles,
          CreatedTime = x.CreatedTime,
          PublishType = x.PublishType,
          Title = x.Title,
          CreatorId = x.CreatorId,
          IsPublished = x.IsPublished,
          Status = x.Status
        }).ToList();

      foreach (var item in items)
      {
        var User = await userManager.GetBriefUserAsync(item.CreatorId.Value, new string[] { "public" });
        item.Creator = User;
      }

      return (total, items);
    }

    /// <summary>
    /// 校稿后发布文件
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async Task<ApprovalItemEntity> PublishApprovalItem(int itemId, string publishDepartment, string publishTitle, string publishType, List<string> purview)
    {
      var entity = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == itemId);
      if (entity.VerifiedFiles.Count() == 0)
      {
        entity.VerifiedFiles = entity.FinalFiles;
      }
      entity.IsPublished = true;
      entity.PublishType = publishType;
      entity.PublishDepartment = publishDepartment ?? "综合管理部";
      entity.PublishTitle = publishTitle;
      entity.PublishTime = DateTime.Now;
      if (purview != null && purview.Count > 0)
      {
        entity.Purview = purview;
      }
      else
      {
        entity.Purview = new List<string>();
      }
      context.Update(entity);
      var result = await context.SaveChangesAsync();
      return result > 0 ? entity : null;
    }

    //获得已经发布的所有申请
    private async Task<List<ApprovalItemModel>> ListPublishItemsAsync(long userId)
    {
      var list = await context.ApprovalItems.Include(e => e.Template)
        .Where(x => x.Status == ApprovalItemStatus.Approved && x.IsPublished)
        .OrderByDescending(x => x.CreatedTime)
        .Select(x => new ApprovalItemModel
        {
          Id = x.Id,
          TemplateTitle = x.Template.Title,
          FinalFiles = x.FinalFiles,
          Content = x.Content,
          VerifiedFiles = x.VerifiedFiles,
          CreatedTime = x.CreatedTime,
          PublishType = x.PublishType,
          Title = x.Title,
          CreatorId = x.CreatorId,
          Purview = x.Purview,
          PublishDepartment = x.PublishDepartment,
          PublishTitle = x.PublishTitle,
          PublishTime = x.PublishTime
        }).ToListAsync();
      list = list.Where(x => x.Purview == null || x.Purview.Count == 0 || x.Purview.Contains(userId.ToString())).ToList();
      return list;
    }

    //获得已经读过的集合
    public async Task<List<int>> ListReadItemsAsync(long userId)
    {
      return await context.ApprovalReadLogs.Where(x => x.UserId == userId).Select(x => x.ItemId).ToListAsync();
    }

    //获得已经发布的公告是否有新的
    public async Task<bool> IsNewPublishItemsAsync(long userId)
    {
      var list = await ListPublishItemsAsync(userId);
      //检索已读
      var readIds = await ListReadItemsAsync(userId);
      list = list.Where(x => !readIds.Contains(x.Id.Value)).ToList();
      return list.Count > 0;
    }

    //已读操作
    public async Task<bool> ReadItemAsync(int itemId, long userId)
    {
      var e = await context.ApprovalReadLogs.Where(x => x.ItemId == itemId && x.UserId == userId).FirstOrDefaultAsync();
      if (e != null) return true;
      var entity = new ApprovalReadLogEntity
      {
        ItemId = itemId,
        UserId = userId,
        CreatedTime = DateTime.Now
      };
      context.ApprovalReadLogs.Add(entity);
      return (await context.SaveChangesAsync()) > 0;
    }

    //构造首页只能查看不能下载的附近
    private List<AttachFile> GetReadOnlyList(List<AttachFile> files, string baseUrl)
    {
      if (files != null && files.Count > 0)
      {
        files.ForEach(o => o.Url = $"{baseUrl}/oo/readonly?path={baseUrl}{o.Url}");
        return files;
      }
      else
      {
        return new List<AttachFile>();
      }
    }

    //获取已经发布的公告通知
    public async Task<IEnumerable<ApprovalItemModel>> ListPublishItemsAsync(long userId, string baseUrl)
    {
      var list = await ListPublishItemsAsync(userId);
      var readIds = await ListReadItemsAsync(userId);
      foreach (var item in list)
      {
        var User = await userManager.GetBriefUserAsync(item.CreatorId.Value, new string[] { "public" });
        item.Creator = User;
        item.VerifiedFiles = GetReadOnlyList(item.VerifiedFiles, baseUrl);
        item.IsRead = readIds.Contains(item.Id.Value);
      }

      return list.OrderBy(x => x.IsRead);
    }

    /// <summary>
    /// 创建再次提交同类型申请
    /// </summary>
    /// <param name="itemId"></param>
    /// <returns></returns>
    public async Task<ApprovalItemEntity> CreateCopyItem(int itemId, long userId, string code)
    {
      var entity = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == itemId);
      var newContent = entity.Content;
      var userDepartments = await departmentManager.GetUserDepartmentsAsync(userId);

      var newEntity = new ApprovalItemEntity
      {
        Code = code,
        Title = entity.Title,
        Content = entity.Content,
        CreatedTime = DateTime.Now,
        CreatorId = userId,
        IsUpdate = true,
        IsPublished = false,
        Purview = new List<string>(),
        Status = ApprovalItemStatus.Draft,
        TemplateId = entity.TemplateId,
        LastUpdatedTime = DateTime.Now,
      };

      context.ApprovalItems.Add(newEntity);
      var result = await context.SaveChangesAsync();
      return result > 0 ? newEntity : null;
    }

    //撤回
    public async Task<List<NodeUserCode>> RecallApprovalAsync(int itemId)
    {
      var codes = new List<NodeUserCode>();
      var entity = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == itemId);
      if (entity == null) return codes;
      entity.Status = ApprovalItemStatus.Draft;
      entity.IsUpdate = true;
      context.ApprovalItems.Update(entity);
      //
      var nodes = await context.ApprovalNodes.Where(x => x.ItemId == itemId).ToArrayAsync();
      if (nodes != null && nodes.Count() > 0)
      {
        foreach (var node in nodes)
        {
          //返回已经发送消息的code,用于做已读操作
          if (!string.IsNullOrEmpty(node.ResponseCode))
          {
            codes.Add(new NodeUserCode { UserId = node.UserId, ResponseCode = node.ResponseCode });
          }
          node.ActionType = ApprovalActionType.Created;
          node.Comments = new List<BriefComment>();
          node.LastUpdatedTime = default(DateTime);
          node.IsRead = null;
          node.ResponseCode = "";
          context.ApprovalNodes.Update(node);
        }
      }
      var r = await context.SaveChangesAsync();
      return codes;
    }

    //补充实际返回时间 - 外出申请使用
    public async Task<bool> UpdateBackTimeAsync(int itemId, DateTime backTime)
    {
      var entity = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == itemId);
      if (entity == null) return false;
      if (entity.Content != null && entity.Content.ContainsKey("confirmDate"))
      {
        entity.Content["confirmDate"] = backTime.ToString("yyyy-MM-dd HH:mm:ss");
      }
      else
      {
        entity.Content.Add("confirmDate", backTime.ToString("yyyy-MM-dd HH:mm:ss"));
      }
      context.ApprovalItems.Update(entity);
      return (await context.SaveChangesAsync()) > 0;
    }

    ////构造审批的消息
    //public TemplateButtonCard GetTemplateButtonCard(string mainTitle, string mainDesc, string quoteUrl, string sourceDesc = "审批消息")
    //{
    //  var buttonText = sourceDesc.Equals("抄送消息") ? "未读" : "待审";
    //  var content = new TemplateButtonCard
    //  {
    //    Source = new HeaderInfo { Description = sourceDesc },
    //    MainTitle = new MainTitle { Title = mainTitle, Description = mainDesc },
    //    QuoteArea = new QuoteArea
    //    {
    //      Type = QuoteAreaType.Url,
    //      Url = quoteUrl,
    //      QuoteText = mainTitle
    //    },
    //    TaskId = MD5.Compute(DateTime.Now.ToString("yyyyMMddHHmmss")),
    //    ButtonList = new List<ButtonItem> {
    //      new ButtonItem        {
    //        Key = "approval",
    //        Text = buttonText,
    //        Type = ButtonItemType.Url,
    //        Url = quoteUrl
    //      }
    //    }
    //  };
    //  return content;
    //}

    ////构造文本通知消息
    //public TemplateNoticeCard GetTemplateNoticeCard(string mainTitle, string mainDesc, string quoteUrl, string sourceDesc = "抄送消息")
    //{
    //  var content = new TemplateNoticeCard
    //  {
    //    Source = new HeaderInfo { Description = sourceDesc },
    //    MainTitle = new MainTitle { Title = mainTitle, Description = mainDesc },
    //    QuoteArea = new QuoteArea
    //    {
    //      Type = QuoteAreaType.Url,
    //      Url = quoteUrl,
    //      QuoteText = mainTitle
    //    },
    //    Card = new CardAction
    //    {
    //      Type = CardActionType.Url,
    //      Url = quoteUrl
    //    },
    //    TaskId = MD5.Compute(DateTime.Now.ToString("yyyyMMddHHmmss"))
    //  };
    //  return content;
    //}

    //更新审批消息Code


    public async Task<string> UpdateResponseCodeAsync(int itemId, long userId, string responseCode, string sourceDesc)
    {
      var query = context.ApprovalNodes.Where(x => x.ItemId == itemId && x.UserId == userId);
      if (sourceDesc.Equals("抄送消息"))
      {
        query = query.Where(x => x.NodeType == ApprovalFlowNodeType.Cc);
      }
      else
      {
        query = query.Where(x => x.ActionType == ApprovalActionType.Pending);
      }
      var node = await query.FirstOrDefaultAsync();
      if (node == null) return "";
      var currentCode = node.ResponseCode;
      node.ResponseCode = responseCode;
      context.ApprovalNodes.Update(node);
      int r = await context.SaveChangesAsync();
      return currentCode;
    }

    //更新抄送已读
    public async Task<string> UpdateNodeCcAsync(int itemId, long userId)
    {
      var node = await context.ApprovalNodes.Where(x => x.ItemId == itemId && x.UserId == userId && x.NodeType == ApprovalFlowNodeType.Cc).FirstOrDefaultAsync();
      if (node == null) return "";
      if (!string.IsNullOrEmpty(node.ResponseCode) && node.IsRead == null)
      {
        node.IsRead = true;
        context.ApprovalNodes.Update(node);
        int r = await context.SaveChangesAsync();
        return node.ResponseCode;
      }
      return "";
    }

    //获取待审批和已经抄送节点同时ResponseCode不为空的数据
    public async Task<IEnumerable<ApprovalNodeEntity>> ListPendingNodesAsync()
    {
      return await context
        .ApprovalNodes
        .Where(x => (x.ActionType == ApprovalActionType.Pending && !string.IsNullOrEmpty(x.ResponseCode)))
        .ToArrayAsync();
    }

    public async Task<IEnumerable<ApprovalItemEntity>> ListApprovalItemAsync(ICollection<int> itemIds)
    {
      return await context.ApprovalItems.Where(x => itemIds.Contains(x.Id)).ToArrayAsync();
    }

    //找到流程中最后的审批人
    public async Task<IEnumerable<long>> GetFinallyUsersAsync(int itemId)
    {
      var userIds = new List<long>();
      var nodes = await context.ApprovalNodes.Where(x => x.ItemId == itemId).OrderBy(x => x.Seq).ToArrayAsync();
      var cc = nodes.Where(x => x.NodeType == ApprovalFlowNodeType.Cc).FirstOrDefault();
      if (cc == null)
      {
        //没有抄送
        var node = nodes.LastOrDefault();
        if (node != null && node.NodeType == ApprovalFlowNodeType.Approval)
        {
          userIds.Add(node.UserId);
        }
        else if (node != null && node.NodeType == ApprovalFlowNodeType.Sub)
        {
          var subUsers = nodes.Where(x => x.PreviousId == node.PreviousId).Select(x => x.UserId).ToArray();
          foreach (var subUser in subUsers)
          {
            userIds.Add(subUser);
          }
        }
      }
      else
      {
        var preNode = nodes.Where(x => x.Id == cc.PreviousId).FirstOrDefault();
        if (preNode != null && preNode.NodeType == ApprovalFlowNodeType.Approval)//单人审批
        {
          userIds.Add(preNode.UserId);
        }
        else if (preNode != null && (preNode.NodeType == ApprovalFlowNodeType.And || preNode.NodeType == ApprovalFlowNodeType.Or))
        {
          var subUsers = nodes.Where(x => x.PreviousId == preNode.Id).Select(x => x.UserId).ToArray();
          foreach (var subUser in subUsers)
          {
            userIds.Add(subUser);
          }
        }
      }

      return userIds;
    }

    //判断是否存在流程
    public async Task<bool> ExistNodeAsync(int itemId)
    {
      var nodeCount = await context.ApprovalNodes.Where(x => x.ItemId == itemId).CountAsync();
      return nodeCount > 0;
    }

    public async Task<string> ApprovalExportAsync(Dictionary<string, string> filters)
    {
      var path = Path.Combine(uploadOptions.Path, "report", "export");
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }
      var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
      path = Path.Combine(uploadOptions.Path, "report", "export", $"{fileName}.xlsx");
      FileInfo file = new FileInfo(path);
      if (file.Exists)
      {
        file.Delete();
        file = new FileInfo(path);
      }
      var query = GetItemQuery(filters, 0);
      var apps = await query.ToArrayAsync();
      if (apps != null && apps.Count() > 0)
      {
        DateTime start = DateTime.Parse(filters["startDate"]);
        DateTime end = DateTime.Parse(filters["endDate"]);
        //审批数据
        var approvalItemsGroupBy = apps.GroupBy(e => e.TemplateId).ToArray();

        using (OfficeOpenXml.ExcelPackage package = new OfficeOpenXml.ExcelPackage(file))
        {
          foreach (var approvalItems in approvalItemsGroupBy)
          {
            var template = await context.ApprovalTemplates.FirstOrDefaultAsync(x => x.Id == approvalItems.Key);
            //sheet
            var worksheet = package.Workbook.Worksheets.Add(template.Title);
            int column = 2;
            //表头
            worksheet.Cells[1, 1].Value = "申请人";
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            foreach (var field in template.Fields)
            {
              if (field.ValueType != "fileList")
              {
                worksheet.Cells[1, column].Value = field.Title;
                worksheet.Cells[1, column].Style.Font.Bold = true;
                column++;
              }
            }
            worksheet.Cells[1, column].Value = "";

            //内容
            int row = 2;
            foreach (var item in approvalItems)
            {
              var user = await userManager.GetBriefUserAsync(item.CreatorId);
              var userName = user!.Profiles["FullName"]?.ToString() ?? user.UserName;
              worksheet.Cells[row, 1].Value = userName;
              var i = 2;
              foreach (var field in template.Fields)
              {
                if (field.ValueType != "fileList")
                {
                  var a = field.Name.Equals("departments") ? "departments.0.title" : field.Name;
                  if (item.Content.ContainsKey(a))
                  {
                    var value = item.Content.ContainsKey(a) && item.Content[a] != null ? item.Content[a] : "";
                    worksheet.Cells[row, i].Value = value;
                    i++;
                  } else
                  {
                    var objectKeys = item.Content.Keys.Where(x => x.StartsWith(a));
                    var value = "";
                    foreach(var k in objectKeys)
                    {
                      value += item.Content[k] + " ";
                    }
                    worksheet.Cells[row, i].Value = value;
                    i++;
                  }
                }
              }
              worksheet.Cells[row, column].Value = "";
              row++;
            }
          }
          package.Save();
        }
      }
      else
      {
        return "";
      }
      return $"/uploads/report/export/{fileName}.xlsx";
    }
  }
}