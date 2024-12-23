using CyberStone.Core.Models;

namespace Approval.Models
{
  public class DepartmentsAndUsers
  {
    public List<Department> Departments { get; set; }
    public List<User> Users { get; set; }

    public DepartmentsAndUsers()
    {
      Departments = new List<Department>();
      Users = new List<User>();
    }
  }
}
