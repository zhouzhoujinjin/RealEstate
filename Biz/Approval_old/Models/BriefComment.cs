using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Models
{
  public class BriefComment
  {
    public string CreatedTime { get; set; }
    public string Content { get; set; }
    public long UserId { get; set; }
    public string UserAvatar { get; set; }
    public string UserName { get; set; }
    public string UserFullName { get; set; }
  }
}
