using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstate.Models
{
  public class PublishInfo
  {
    public string PublishType { get; set; }
    public string PublishDepartment { get; set; }
    public string PublishTitle { get; set; }
    public List<string> Purview { get; set; }
  }

  public class NoticeApproval
  {
    public int NoticeId { get; set; }
    public string Comment { get; set; }
  }

  public class TransInfo
  {
    public int UserId { get; set; }
    public string Comment { get; set; }
  }

  public class WriteCheckInfo
  {
    public string Type { get; set; }
    public string Op { get; set; }
    public string Content { get; set; }
  }

  public class NoticePublish
  {
    public int NoticeId { get; set; }
    public DateTime PublishTime { get; set; }
  }

}
