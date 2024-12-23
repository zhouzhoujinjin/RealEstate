using Approval.Managers;
using Approval.Models;
using Approval.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PureCode.Managers;
using PureCode.Utils;
using SeafileClient.Responses;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Approval.Controllers
{
  [Route("/seafile")]
  public class OnlyOfficeController : Controller
  {
    private SeafileManager seafileManager;
    private ApprovalManager approvalManager;
    private UserManager userManager;
    private SeafileOptions seafileOptions;

    public OnlyOfficeController(
      SeafileManager seafileManager, 
      ApprovalManager approvalManager, 
      UserManager userManager,
      IOptions<SeafileOptions> seafileOptionsAccessor)
    {
      this.seafileManager = seafileManager;
      this.approvalManager = approvalManager;
      this.userManager = userManager;
      seafileOptions = seafileOptionsAccessor.Value;
    }

    [HttpGet("{appName}/{id}/{fileId}")]
    public IActionResult Index(string appName, int id, string fileId)
    {
      dynamic d = new ExpandoObject();
      d.AppName = appName;
      d.OnlyOfficeApiJsUrl = seafileOptions.OnlyOfficeApiJsUrl;
      d.Id = id;
      d.FileId = fileId ;
      return View(d);
    }

    [HttpGet("/oo/readonly")]
    public IActionResult ReadOnly(string path)
    {
      dynamic d = new ExpandoObject();
      d.FileType = path.Split(".").LastOrDefault();
      d.Key = Guid.NewGuid().ToString();
      d.OnlyOfficeApiJsUrl = seafileOptions.OnlyOfficeApiJsUrl;
      d.Path = path;
      d.Title = "只读文档";
      d.DocumentType = d.FileType switch
      {
        "docx" => "word",
        "xlsx" => "cell",
        "pptx" => "slide",
        "pdf" => "word",
        "doc" => "word",
        "xls" => "cell",
        "ppt" => "slide",
        _ => throw new ArgumentException("Invalid file type")
      };
      return View(d);
    }

    [HttpGet("/wechat/seafile/approval/{id}/{fileId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<AjaxResponse<OnlyOffice>> ApprovalAsync(int id, string fileId)
    {
      var item = await approvalManager.GetItem(id);
      var type = TemplateUtils.GetTemplateModelType(item.Template.Name);
      var file = (DotSplittedKeyDictionaryToObjectConverter.Parse(item.Content, type) as IFieldsModel).Attachments.Where(x => x.FileId == fileId).FirstOrDefault();
      if (file != null)
      {
        var user = await userManager.GetBriefUserAsync(HttpContext.GetUserId());
        var model = (await seafileManager.GetFileEditorUrlAsync(file.RepoId, file.FilePath));
        if (model.OnlyOffice == null)
        {
          return new AjaxResponse<OnlyOffice>
          {
            Data = new OnlyOffice { DocUrl = model.Url, DocumentType = "file", DocTitle = file.Title, CanEdit = item.Status != ApprovalItemStatus.Approved }
          };
        }
        else
        {
          model.OnlyOffice.DocTitle = file.Title;
          model.OnlyOffice.UserName = user.Profiles["FullName"].ToString();
        }
        return new AjaxResponse<OnlyOffice>
        {
          Data = model.OnlyOffice
        };
      } else
      {
        return new AjaxResponse<OnlyOffice>
        {
          Code = 404
        }; ;
      }
    }
  }
}
