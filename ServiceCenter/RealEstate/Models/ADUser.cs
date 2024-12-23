using System.Collections.Generic;
using System;

namespace RealEstate.Models
{
  public class ADUser
  {
    public Guid ObjectGUID { get; set; }

    public string sAMAccountName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;

    public DateTime WhenCreated { get; set; }

    public List<string> MemberOf { get; set; } = new List<string>();
  }
}
