using Approval.Managers;
using Approval.Models;
using CyberStone.Core;
using CyberStone.Core.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Approval.Controllers
{
  public class FileController : ControllerBase
  {
    private FileManager fileManager;
    private ApprovalDbContext context;
    private UploadOptions uploadOptions;

    public FileController(FileManager fileManager, ApprovalDbContext context, IOptions<UploadOptions> uploadOptionsAccessor)
    {
      this.fileManager = fileManager;
      this.context = context;
      this.uploadOptions = uploadOptionsAccessor.Value;
    }
    [HttpPost("/wechat/approval/upload")]
    public AjaxResp<IEnumerable<AttachFile>> UploadAsync(string templateName, int itemId)
    {
      var savePath = Path.Combine(uploadOptions.AbsolutePath, $"approval/{templateName}/{itemId}");
      var result = new List<AttachFile>();
      foreach (var file in HttpContext.Request.Form.Files)
      {
        var filePath = UploadUtils.MoveFile(file, savePath);
        var attach = new AttachFile
        {
          Title = file.Name,
          Url = UploadUtils.GetUrl(filePath, uploadOptions)
        };
        result.Add(attach);
      }
      return new AjaxResp<IEnumerable<AttachFile>>
      {
        Data = result
      };
    }
    [HttpPost("/api/approval/upload")]
    public AjaxResp<AttachFile> UploadAsync(IFormFile file, [FromForm] string templateName, [FromForm] int itemId)
    {
      var savePath = Path.Combine(uploadOptions.AbsolutePath, $"approval/{templateName}/{itemId}");
      var filePath = UploadUtils.MoveFile(file, savePath);
      var attach = new AttachFile
      {
        Title = file.FileName,
        Url = UploadUtils.GetUrl(filePath, uploadOptions)
      };
      return new AjaxResp<AttachFile>
      {
        Data = attach
      };
    }

    [HttpPost("api/approval/final/{itemId}/upload")]
    public async Task<AjaxResp<List<AttachFile>>> UploadFinalFileAsync(IFormFile file, [FromRoute] int itemId)
    {
      if (itemId == 0 || file == null) return new AjaxResp<List<AttachFile>> { Data = null };
      var item = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == itemId);
      var url = fileManager.UploadFile(file, itemId.ToString());
      var finalFile = new AttachFile
      {
        Title = file.FileName,
        Url = url
      };
      if (item.FinalFiles == null) item.FinalFiles = new List<AttachFile>();
      item.FinalFiles.Add(finalFile);
      //只有需要转终稿状态才可以更改最终状态
      if (item.Status == ApprovalItemStatus.Upload)
      {
        item.Status = ApprovalItemStatus.Approved;
      }
      context.ApprovalItems.Update(item);
      await context.SaveChangesAsync();
      return new AjaxResp<List<AttachFile>>
      {
        Message = "上传成功",
        Data = item.FinalFiles
      };
    }

    [HttpDelete("api/approval/final/{itemId}/remove/{title}")]
    public async Task<AjaxResp<int>> RemoveFinalFileAsync([FromRoute] int itemId, [FromRoute] string title = "")
    {
      if (itemId == 0) return new AjaxResp<int> { Data = 0 };
      var item = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == itemId);
      if (string.IsNullOrEmpty(title))
      {
        item.FinalFiles = new List<AttachFile>();
      }
      else
      {
        var removeFile = item.FinalFiles.FirstOrDefault(x => x.Title == title);
        item.FinalFiles.Remove(removeFile);
      }
      context.ApprovalItems.Update(item);
      var result = await context.SaveChangesAsync();
      return new AjaxResp<int>
      {
        Data = result
      };
    }


    //to-do upload verified files
    [HttpPost("api/approval/verify/{itemId}/upload")]
    public async Task<AjaxResp<List<AttachFile>>> UploadVerifyFileAsync(IFormFile file, [FromRoute] int itemId)
    {
      if (itemId == 0 || file == null) return new AjaxResp<List<AttachFile>> { Data = null };
      var item = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == itemId);
      var url = fileManager.UploadFile(file, itemId.ToString());
      var finalFile = new AttachFile
      {
        Title = file.FileName,
        Url = url
      };
      if (item.VerifiedFiles == null) item.VerifiedFiles = new List<AttachFile>();
      item.VerifiedFiles.Add(finalFile);
      context.ApprovalItems.Update(item);
      await context.SaveChangesAsync();
      return new AjaxResp<List<AttachFile>>
      {
        Message = "上传成功",
        Data = item.VerifiedFiles
      };
    }

    [HttpDelete("api/approval/verify/{itemId}/remove/{title}")]
    public async Task<AjaxResp<int>> RemoveVerifyFileAsync([FromRoute] int itemId, [FromRoute] string title = "")
    {
      if (itemId == 0) return new AjaxResp<int> { Data = 0 };
      var item = await context.ApprovalItems.FirstOrDefaultAsync(x => x.Id == itemId);
      if (string.IsNullOrEmpty(title))
      {
        item.VerifiedFiles = new List<AttachFile>();
      }
      else
      {
        var removeFile = item.VerifiedFiles.FirstOrDefault(x => x.Title == title);
        item.VerifiedFiles.Remove(removeFile);
      }
      context.ApprovalItems.Update(item);
      var result = await context.SaveChangesAsync();
      return new AjaxResp<int>
      {
        Data = result
      };
    }

    public RedirectResult RedirectSeafile(int itemId, string fileId)
    {
      // 根据id找到item，然后找到 file的路径
      //return Redirect($"{options.BaseUrl}/repos")
      throw new NotImplementedException();
    }
  }
}
