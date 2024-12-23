using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Approval.Models
{
  public class BriefComment
  {
    [JsonPropertyName("createdTime")]
    public string CreatedTime { get; set; } = "";
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
    [JsonPropertyName("userId")]
    public long UserId { get; set; }
    [JsonPropertyName("userAvatar")]
    public string UserAvatar { get; set; } = "";
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = "";
    [JsonPropertyName("userFullName")]
    public string UserFullName { get; set; } = "";
  }
}
