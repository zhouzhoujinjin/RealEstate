using Approval.Models;


namespace Approval.Entities
{
  public class ApprovalTemplateEntity
  {
    public long Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public string IconUrl { get; set; }
    public string Description { get; set; }
    public DepartmentsAndUsers Applicants { get; set; }

    public List<FormField> Fields { get; set; }

    public Workflow Workflow { get; set; }

    public TemplateGroup Group { get; set; }
    public int Seq { get; set; }
    public DateTime? LastUpdatedTime { get; set; }
    public bool IsVisible { get; set; }
    public bool IsFinal { get; set; }
    public bool IsCustomFlow { get; set; }

    public List<string> SummaryFields { get; set; } = new List<string>();
    public Dictionary<string, string>? JavascriptHooks { get; set; }
    public List<ConditionField> ConditionFields { get; set; }
    public List<long> DepartmentIds { get; set; } = new List<long>();

    public ICollection<ApprovalItemEntity> Items { get; set; }

    public ApprovalTemplateEntity()
    {
      Items = new List<ApprovalItemEntity>();
      Fields = new List<FormField>();
    }
  }
}
