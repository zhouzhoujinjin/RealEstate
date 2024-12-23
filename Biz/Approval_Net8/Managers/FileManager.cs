using Approval.Models;
using CyberStone.Core.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Approval.Managers
{
  public class FileManager
  {
    private readonly UploadOptions uploadOptions;

    public FileManager(IOptions<UploadOptions> uploadOptionsAccessor)
    {
      uploadOptions = uploadOptionsAccessor.Value;
    }

    public string UploadFile(IFormFile file, string dirName)
    {
      var rootPath = Path.Combine(uploadOptions.AbsolutePath, dirName);
      var fileName = UploadUtils.MoveFile(file, rootPath, false);
      if (string.IsNullOrEmpty(fileName)) return null;
      fileName = UploadUtils.GetUrl(fileName, rootPath, uploadOptions.WebRoot + "/" + dirName);
      return fileName;
    }

    public AttachFile UploadOrderFile(IFormFile file, string dirName)
    {
      var name = file.FileName;
      var size = file.Length;
      var rootPath = Path.Combine(uploadOptions.AbsolutePath, dirName);
      var fileName = UploadUtils.MoveFile(file, rootPath, false);
      if (string.IsNullOrEmpty(fileName)) return null;
      fileName = UploadUtils.GetUrl(fileName, rootPath, uploadOptions.WebRoot + "/" + dirName);
      return new AttachFile
      {
        Title = name,
        Url = fileName,
        Size = size
      };
    }
  }
}
