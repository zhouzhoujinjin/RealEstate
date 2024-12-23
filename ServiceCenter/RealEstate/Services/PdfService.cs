using Approval;
using Approval.Models;
using CyberStone.Core.Models;
using CyberStone.Core.Utils;
using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.IO.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.Extensions.Options;
using System.Text;

namespace RealEstate.Services
{
  public class PdfService
  {
    private readonly UploadOptions uploadOptions;

    public PdfService(IOptions<UploadOptions> uploadOptionsAccessor)
    {
      uploadOptions = uploadOptionsAccessor.Value;
    }

    public PdfInfo GetExportInfo(IEnumerable<FormField> fields, IFieldsModel content, ICollection<ApprovalFlowNode> nodes, PdfInfo info)
    {
      //内容
      var data = new Dictionary<string, string>();
      foreach (var field in fields)
      {
        var fieldName = field.Name.Substring(0, 1).ToUpper() + field.Name.Substring(1);
        switch (field.ControlType)
        {
          case "single-picker":
          case "date-picker":
          case "datetime-picker":
          case "input":
          case "input-number":
          case "textarea":
            var v = content.GetType().GetProperty(fieldName).GetValue(content, null);
            if (field.ControlOptions != null)
            {
              if (field.ControlOptions.ContainsKey("currency"))
              {
                var r = float.TryParse(v.ToString(), out var f);
                if (r)
                {
                  v = Math.Round(f, 2).ToString(".00");
                }
              }
              if (field.ControlOptions.ContainsKey("suffix"))
              {
                v = string.IsNullOrEmpty(v.ToString()) ? "" : v.ToString() + field.ControlOptions["suffix"];
              }
            }

            if (v != null)
            {
              data.Add(field.Title, v.ToString());
            }
            break;
          case "multi-picker":
            var mp = content.GetType().GetProperty(fieldName).GetValue(content, null);
            if (mp != null)
            {
              data.Add(field.Title, string.Join(",", (List<string>)mp));
            }
            break;
          case "user":
          case "user-department-picker":
            var users = content.GetType().GetProperty(fieldName).GetValue(content, null);
            if (users != null)
            {
              data.Add(field.Title, string.Join(",", ((List<Client>)users).Select(x => x.Name)));
            }
            break;
          case "department":
            var depts = content.GetType().GetProperty(fieldName).GetValue(content, null);
            if (depts != null)
            {
              data.Add(field.Title, string.Join(",", ((List<Department>)depts).Select(x => x.Title)));
            }
            break;
          case "upload":
            var files = content.GetType().GetProperty(fieldName).GetValue(content, null);
            if (files != null)
            {
              data.Add(field.Title, string.Join(",", ((List<AttachFile>)files).Select(x => x.Title)));
            }
            break;
          case "title-value-list":
            var values = content.GetType().GetProperty(fieldName).GetValue(content, null);
            if (values != null)
            {
              data.Add(field.Title, string.Join("<br/>", values as IEnumerable<object>));
            }
            break;
          default:
            break;
        }
      }
      info.Content = data;
      //流程
      var flows = new List<FlowInfo>();
      if (nodes != null && nodes.Count > 1)
      {
        var list = nodes.ToArray();
        for (int i = 1; i < list.Length; i++)
        {
          var flow = list[i];
          //获取人员名称
          var userName = "";
          Type type = flow.GetType();
          if (type.Name.Equals("ApprovalFlowNode"))
          {
            userName = flow.User != null && flow.User.Profiles != null ? flow.User.Profiles["FullName"].ToString() : "未知";
          }
          else
          {
            var t = (LogicApprovalFlowNode)flow;
            var tmpName = new List<string>();
            foreach (var p in t.Children)
            {
              var m = (ApprovalFlowNode)p;
              tmpName.Add(m.User != null && m.User.Profiles != null ? m.User.Profiles["FullName"].ToString() : "未知");
            }
            userName = string.Join("，", tmpName);
          }
          var actionType = GetActionType(flow.ActionType);
          var dateTime = flow.LastUpdatedTime.Value.ToString("yyyy-MM-dd HH:mm:ss").Equals("0001-01-01 00:00:00") ? "" : flow.LastUpdatedTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
          var comments = GetComments(flow.ActionType, flow.Comments);
          if (flow.NodeType == ApprovalFlowNodeType.Cc)
          {
            flows.Add(new FlowInfo
            {
              UserName = $"抄送 {userName}",
              ActionType = "cc",
              DateTime = dateTime,
              Comments = comments
            });
          }
          else
          {
            flows.Add(new FlowInfo
            {
              UserName = userName,
              ActionType = actionType,
              DateTime = dateTime,
              Comments = comments
            });
          }
        }
      }
      info.Nodes = flows;
      return info;
    }

    public string CreateApprovalPdf(PdfInfo info)
    {

      //产城和医药谷流程，不显示申请人部门
      long[] templateIds = { 10, 11, 12, 20, 24, 25, 26, 27 };
      //医药谷流程 用另一个模板
      long[] templateIdsYiYaoGu = { 24, 25, 26, 27 };

      //构造动态数据
      var sb = new StringBuilder();
      foreach (var kv in info.Content)
      {
        string c = kv.Value;
        if (kv.Key == "附件")
        {
          var f = kv.Value.Split(",");
          if (f.Length > 20)
          {
            c = string.Join(", ", f.Take(20)) + $"<br/>等 {f.Length - 20} 项";
          }
          else
          {
            c = string.Join(", ", f);
          }
        }
        sb.Append($"<tr><td style='height: 2em;'> {kv.Key} </td><td>{c}</td></tr>");
      }
      //流程
      var flow = new StringBuilder();
      for (int i = 0; i < info.Nodes.Count; i++)
      {
        var node = info.Nodes[i];
        if (i == 0)
        {
          if (node.ActionType.Equals("cc"))
          {
            flow.Append($"<tr><td rowspan='{info.Nodes.Count}'> 审批流程 </td><td style='height: 1.5em;' ><div>{node.UserName} {node.DateTime}</div></td></tr>");
          }
          else
          {
            flow.Append($"<tr><td rowspan='{info.Nodes.Count}'> 审批流程 </td><td style='height: 3em;' ><div> {node.Comments} </div><div>{node.UserName} {node.ActionType} {node.DateTime}</div></td></tr>");
          }
        }
        else
        {
          if (node.ActionType.Equals("cc"))
          {
            flow.Append($"<tr><td style='height: 1.5em;'><div>{node.UserName} {node.DateTime}</div></td></tr>");
          }
          else
          {
            flow.Append($"<tr><td style='height: 3em;'><div> {node.Comments} </div><div>{node.UserName} {node.ActionType}  {node.DateTime}</div></td></tr>");
          }
        }
      }
      //申请人部门
      var departmentName = new StringBuilder();

      //产城 和 医药谷流程 不显示申请人部门
      if (!templateIds.Contains(info.TemplateId))
      {
        departmentName.Append($"<tr><td rowspan='{0}'> 申请人部门 </td><td style='height: 1.5em;' ><div> {info.DepartmentName} </div></td></tr>");
      }

      string content;
      //医药谷流程 模板
      if (templateIdsYiYaoGu.Contains(info.TemplateId))
      {
        string date = $"<tr><td rowspan='{0}'> 申请时间 </td><td style='height: 1.5em;' ><div> {DateTime.Parse(info.DateTime).ToString("yyyy-MM-dd")} </div></td></tr>";
        content = File.ReadAllText("templates/student-export-yiyaogu.html");
        //替换数据
        content = content.Replace("{{approvalTitle}}", info.ApprovalTitle)
          //.Replace("{{companyName}}", info.CompanyName)
          .Replace("{{dateTime}}", date)
          .Replace("{{userName}}", info.UserName)
          .Replace("{{departmentName}}", departmentName.ToString())
          .Replace("{{content}}", sb.ToString())
          .Replace("{{flows}}", flow.ToString());
      }
      else
      {
        content = File.ReadAllText("templates/student-export.html");
        //替换数据
        content = content.Replace("{{approvalTitle}}", info.ApprovalTitle)
          .Replace("{{companyName}}", info.CompanyName)
          .Replace("{{dateTime}}", info.DateTime)
          .Replace("{{userName}}", info.UserName)
          .Replace("{{departmentName}}", departmentName.ToString())
          .Replace("{{content}}", sb.ToString())
          .Replace("{{flows}}", flow.ToString());
      }


      //生成临时html
      var fontProvider = new DefaultFontProvider(true, true, false);
      fontProvider.AddFont(FontProgramFactory.CreateFont("templates/simsun.ttf"));
      fontProvider.AddFont(FontProgramFactory.CreateFont("templates/simhei.ttf"));
      //
      var path = Path.Combine(uploadOptions.Path, "approval");
      if (!Directory.Exists(path))
      {
        Directory.CreateDirectory(path);
      }
      var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
      path = Path.Combine(uploadOptions.Path, "approval", $"{fileName}.pdf");

      var properties = new ConverterProperties();
      properties.SetFontProvider(fontProvider);
      properties.SetBaseUri("./");
      var elements = HtmlConverter.ConvertToElements(content, properties);
      var pdf = new PdfDocument(new PdfWriter(path));
      pdf.SetTagged();
      var document = new iText.Layout.Document(pdf);
      document.SetMargins(45, 36, 45, 36);
      foreach (var element in elements)
      {
        document.Add((IBlockElement)element);
      }
      document.Close();

      return $"/uploads/approval/{fileName}.pdf";
    }

    private string GetActionType(ApprovalActionType actionType)
    {
      var text = string.Empty;
      switch (actionType)
      {
        case ApprovalActionType.Created:
          text = "已创建";
          break;
        case ApprovalActionType.Pending:
          text = "待审批";
          break;
        case ApprovalActionType.Approved:
          text = "已同意";
          break;
        case ApprovalActionType.Rejected:
          text = "已拒绝";
          break;
        case ApprovalActionType.Comment:
          text = "评论";
          break;
        default:
          break;
      }
      return text;
    }
    private string GetComments(ApprovalActionType actionType, List<BriefComment> comments)
    {
      var text = string.Empty;
      switch (actionType)
      {
        case ApprovalActionType.Created:
          text = "未审批";
          break;
        case ApprovalActionType.Pending:
          text = "审批";
          break;
        case ApprovalActionType.Approved:
          text = "同意";
          break;
        case ApprovalActionType.Rejected:
          text = "拒绝";
          break;
        case ApprovalActionType.Comment:
          text = "评论";
          break;
        default:
          break;
      }
      //if (comments != null && comments.Count > 0)
      //{
      //  text = string.Join(",", comments.Select(x => x.Content).ToArray());
      //}
      return text;
    }
  }
}
