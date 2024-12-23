using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Models
{
  public class ApprovalTemplateModel
  {
    
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Title { get; set; }
    
    public string IconUrl { get; set; }
    
    public string Description { get; set; }
    
    public TemplateGroup GroupCode { get; set; }
    
    public string GroupTitle { get; set; }

    
    public IEnumerable<FormField> Fields { get; set; }

    
    public bool IsCustomFlow { get; set; }
    
    public List<ConditionField> ConditionFields { get; set; }

    
    public Workflow Workflow { get; set; }
    
    public List<int> DepartmentIds { get; set; } = new List<int>();
  }
}
