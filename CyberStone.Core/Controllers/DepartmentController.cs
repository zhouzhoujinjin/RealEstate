using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CyberStone.Core.Managers;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using System.Threading.Tasks;

namespace CyberStone.Core.Controllers
{
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = ClaimNames.ApiPermission)]
  [Route("/api/departments")]
  [ApiController]
  public partial class DepartmentController : ControllerBase
  {
    private readonly DepartmentManager _departmentManager;

    public DepartmentController(DepartmentManager departmentManager)
    {
      _departmentManager = departmentManager;
    }

    [HttpGet("/api/departments", Name = "部门列表")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<Department>> GetDepartments()
    {
      var department = await _departmentManager.GetDepartmentTreeAsync();
      return new AjaxResp<Department>
      {
        Data = department
      };
    }

    [HttpGet("/api/departments/{departmentId}", Name = "部门信息")]
    [UserLog(UserLogLevel.Classified)]
    public async Task<AjaxResp<Department>> GetDepartment(long departmentId)
    {
      var model = await _departmentManager.GetDepartmentWithUsersAsync(departmentId);

      return new AjaxResp<Department>
      {
        Data = model
      };
    }
  }
}