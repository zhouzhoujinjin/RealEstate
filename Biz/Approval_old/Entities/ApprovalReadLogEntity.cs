using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Entities
{
  public class ApprovalReadLogEntity
  {
    public int Id { get; set; }
    public int ItemId { get; set; }
    public long UserId { get; set; }
    public DateTime CreatedTime { get; set; }
  }
}
