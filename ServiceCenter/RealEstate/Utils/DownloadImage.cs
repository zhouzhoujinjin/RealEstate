namespace RealEstate.Utils
{
  public class DownloadImage
  {
    //下载微信临时素材到本地，返回相对路径
    //public static async Task<List<string>> DownloadImageAsync(WeChatWorkApi api, string appName, string savePath, string webPath, ICollection<string> MediaIds)
    //{
    //  if (!Directory.Exists(savePath))
    //  {
    //    Directory.CreateDirectory(savePath);
    //  }
    //  var list = new List<string>();
    //  foreach (var item in MediaIds)
    //  {
    //    var resp = await api.DownloadMediaAsync(appName, item);
    //    if (resp.Stream != null)
    //    {
    //      var fileName = $"{DateTimeOffset.Now.Ticks}.jpg";
    //      var filePath = Path.Combine(savePath, fileName);
    //      using (var stream = new FileStream(filePath, FileMode.Create))
    //      {
    //        stream.Write(resp.Stream);
    //        stream.Flush();
    //      }
    //      var url = UploadUtils.GetUrl(filePath, savePath, webPath);
    //      list.Add(url);
    //    }
    //  }
    //  return list;
    //}
  }
}
