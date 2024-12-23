using Approval.Managers;
using Approval.Models;
using CyberStone.Core;
using CyberStone.Core.Managers;
using Microsoft.AspNetCore.Mvc;

namespace Letian.Controllers
{
  [Route("api/admin/approval")]
  [ApiController]
  public class AdminApprovalController : ControllerBase
  {
    private ApprovalManager approvalManager;
    private ValueSpaceMap valueSpaceMap;
    private TemplateManager templateManager;

    public AdminApprovalController(ApprovalManager approvalManager, ValueSpaceMap valueSpaceMap, TemplateManager templateManager)
    {
      this.approvalManager = approvalManager;
      this.valueSpaceMap = valueSpaceMap;
      this.templateManager = templateManager;
    }

    [HttpGet("templates", Name = "管理员 - 审批 - 获取审批模板列表")]
    public async Task<AjaxResp<IEnumerable<ApprovalTemplateModel>>> GetTemplates()
    {
      var entities = await templateManager.GetAllTemplates();
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

    [HttpGet("templates/{name}", Name = "管理员 - 审批 - 获取审批模板")]
    public async Task<AjaxResp<ApprovalTemplateModel>> GetTemplateAsync(string name)
    {
      var template = await templateManager.GetTemplateByName(name);
      var result = new ApprovalTemplateModel
      {
        Id = template.Id,
        Name = template.Name,
        Title = template.Title,
        IconUrl = template.IconUrl,
        Description = template.Description,
        GroupCode = template.Group,
        Fields = template.Fields,
        ConditionFields = template.ConditionFields,
        GroupTitle = valueSpaceMap.GetTitleByNameKey("approvalTemplateGroup", template.Group.ToString("d")),
        Workflow = template.Workflow
      };
      return new AjaxResp<ApprovalTemplateModel> { Data = result };
    }

    [HttpPut("templates/{name}", Name = "管理员 - 审批 -修改模板")]
    public async Task<AjaxResp<Workflow>> UpdateTemplateAsync([FromRoute] string name, [FromBody] Workflow workflow)
    {
      await templateManager.UpdateTemplateWorkflow(name, workflow);
      return new AjaxResp<Workflow>
      {
        Data = workflow
      };
    }
  }
}
