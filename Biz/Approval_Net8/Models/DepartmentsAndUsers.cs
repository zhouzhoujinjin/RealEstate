using CyberStone.Core.Models;
using System.Text.Json.Serialization;

namespace Approval.Models
{
  public class DepartmentsAndUsers
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }
    [JsonPropertyName("users")]
    public List<User> Users { get; set; }

    public DepartmentsAndUsers()
    {
      Departments = new List<Department>();
      Users = new List<User>();
    }
  }
}
