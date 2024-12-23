using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Approval.Models
{
  public class AttachFile
  {
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
    [JsonPropertyName("size")]
    public long Size { get; set; }

  }

  public class AttachFileWithId : AttachFile
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }
  }

  public class AttachFileWithType : AttachFile
  {
    [JsonPropertyName("fileType")]
    public string FileType { get; set; } = string.Empty;
  }

}
