using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Models
{
  public class SeafileOptions
  {
    public string BaseUrl { get; set; }
    public string ApprovalUserName { get; set; }
    public string ApprovalPassword { get; set; }
    public string OnlyOfficeApiJsUrl { get; set; }
  }
}
