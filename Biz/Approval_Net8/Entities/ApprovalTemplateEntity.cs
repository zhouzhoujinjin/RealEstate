using Approval.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Entities
{
  public class ApprovalTemplateEntity
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public string IconUrl { get; set; }
    public string Description { get; set; }
    public DepartmentsAndUsers Applicants { get; set; }

    public ICollection<FormField> Fields { get; set; }

    public Workflow Workflow { get; set; }

    public TemplateGroup Group { get; set; }
    public int Seq { get; set; }
    public DateTime? LastUpdatedTime { get; set; }
    public bool IsVisible { get; set; }
    public bool IsFinal { get; set; }
    public bool IsCustomFlow { get; set; }
    public List<ConditionField> ConditionFields { get; set; }
    public List<int> DepartmentIds { get; set; } = new List<int>();

    public ICollection<ApprovalItemEntity> Items { get; set; }

    public ApprovalTemplateEntity()
    {
      Items = new List<ApprovalItemEntity>();
      Fields = new List<FormField>();
    }
  }
}
