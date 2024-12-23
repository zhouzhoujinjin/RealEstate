using Microsoft.AspNetCore.Mvc;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyberStone.Core.Controllers
{
  public partial class DepartmentController
  {
    [HttpGet("/api/admin/departments", Name = "部门树")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<Department>> GetAdminDepartments()
    {
      var department = await _departmentManager.GetDepartmentTreeAsync();
      return new AjaxResp<Department>
      {
        Data = department
      };
    }

    [HttpPost("/api/admin/departments", Name = "添加部门")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<Department>> AddBaseDepartment([FromBody] Department department)
    {
      var creatorId = HttpContext.GetUserId();
      var currentDepartment = await _departmentManager.GetDepartmentTreeAsync();
      var result = await _departmentManager.CreateAsync(department.Title, creatorId, currentDepartment?.Id);
      return new AjaxResp<Department>
      {
        Data = result
      };
    }

    [HttpPost("/api/admin/departments/struct", Name = "更新部门结构")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp> UpdateDepartmentStruct([FromBody] Dictionary<long, IEnumerable<long>> structs)
    {
      await _departmentManager.UpdateStructsAsync(structs);
      return new AjaxResp { };
    }

    [HttpPut("/api/admin/departments/{departmentId}", Name = "更新部门")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp> UpdateDepartment(long departmentId, [FromBody] Department department)
    {
      await _departmentManager.UpdateDepartmentAsync(departmentId, department);
      return new AjaxResp
      {
        Message = "操作成功"
      };
    }

    [HttpDelete("/api/admin/departments/{departmentId}", Name = "删除部门")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp> DeleteDepartment(int departmentId)
    {
      await _departmentManager.DeleteAsync(departmentId);
      return new AjaxResp();
    }
  }
}