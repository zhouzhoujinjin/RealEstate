using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Models
{
  public class FormField
  {
    
    public string Title { get; set; }
    
    public string Name { get; set; }
    
    public string ValueType { get; set; }
    
    public string ControlType { get; set; }
    
    public bool Required { get; set; }
    
    public Dictionary<string, object> ControlOptions { get; set; }
  }

  public class BlockField: FormField
  {
    public int MinCount { get; set; }
    public int MaxCount { get; set; }

    public ICollection<FormField> Children { get; set; }
  }

  public class BackTimeForm
  {
    public DateTime BackTime { get; set; }
  }

  public class OvertimeFinishDate
  {
    public int ItemId { get; set; }
    public DateTime FinishDate { get; set; }
  }
}
