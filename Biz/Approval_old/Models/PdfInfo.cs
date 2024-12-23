using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Models
{
  public class PdfInfo
  {
    public string ApprovalTitle { get; set; }
    public string CompanyName { get; set; }
    public string DateTime { get; set; }
    public string UserName { get; set; }
    public string DepartmentName { get; set; }
    public Dictionary<string, string> Content { get; set; }
    public List<FlowInfo> Nodes { get; set; }
    public int TemplateId { get; set; }
  }

  public class FlowInfo
  {
    public string UserName { get; set; }
    public string ActionType { get; set; }
    public string DateTime { get; set; }
    public string Comments { get; set; }
  }
}
