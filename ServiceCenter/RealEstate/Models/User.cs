using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealEstate.Models
{
  public class IssueSendUser
  {
    public ICollection<long> Leaders { get; set; }
    public ICollection<long> Notifiers { get; set; }
    public bool Supervisor { get; set; }
  }
}
