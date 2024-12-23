using PureCode.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Models
{
  public class DepartmentsAndUsers
  {
    public List<Department> Departments { get; set; }
    public List<BriefUser> Users { get; set; }

    public DepartmentsAndUsers()
    {
      Departments = new List<Department>();
      Users = new List<BriefUser>();
    }
  }
}
