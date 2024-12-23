using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Approval.Models
{
  public class AttachFile
  {
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonPropertyName("url")]
    public long Size { get; set; }

  }

  public class SeafileAttach: AttachFile
  {
    [JsonPropertyName("repoId")]
    public string RepoId { get; set; }
    [JsonPropertyName("path")]
    public string FilePath { get; set; }
    [JsonPropertyName("fileName")]
    public string FileName { get; set; }
    [JsonPropertyName("repoName")]
    public string RepoName { get; set; }
    [JsonPropertyName("fileId")]
    public string FileId { get; internal set; }
  }

  public class AttachFileWithId : AttachFile
  {
    public int Id { get; set; }
  }

  public class AttachFileWithType : AttachFile
  {
    public string FileType { get; set; }
  }

}
