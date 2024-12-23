using Approval.Models;
using CyberStone.Core.Models;
using System.Text.Json.Serialization;

namespace Approval.Models
{
  public interface IFieldsModel
  {
    public List<Department> Departments { get; }
    public string DepartmentIds { get; set; }
    public List<AttachFile> Attachments { get; }
  }

  //请假
  public class LeaveModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("docTitle")]
    public string DocTitle { get; set; }

    [JsonPropertyName("leaveType")]
    public string LeaveType { get; set; }

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("days")]
    public string Days { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //付款申请
  public class PaymentModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("docTitle")]
    public string DocTitle { get; set; }

    [JsonPropertyName("paymentType")]
    public string PaymentType { get; set; }

    [JsonPropertyName("amount")]
    public string Amount { get; set; }

    [JsonPropertyName("paymentWay")]
    public string PaymentWay { get; set; }

    [JsonPropertyName("organization")]
    public string Organization { get; set; }

    [JsonPropertyName("bank")]
    public string Bank { get; set; }

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  public class Client
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Avatar { get; set; }
  }

  //公司发文
  public class CompanyDocumentModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("docType")]
    public string DocType { get; set; }

    [JsonPropertyName("docIssueDate")]
    public DateTime DocIssueDate { get; set; }

    [JsonPropertyName("docTitle")]
    public string DocTitle { get; set; }

    [JsonPropertyName("docAbstract")]
    public string DocAbstract { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }

    [JsonPropertyName("docTargets")]
    public List<Client> DocTargets { get; set; }
  }

  //加班
  public class OvertimeModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("days")]
    public string Days { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("finishDate")]
    public DateTime? FinishDate { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //采购申请
  public class PurchaseModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("")]
    public string DocTitle { get; set; }

    [JsonPropertyName("")]
    public string PurchaseType { get; set; }

    [JsonPropertyName("")]
    public string Amount { get; set; }

    [JsonPropertyName("")]
    public string Description { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //项目发文
  public class ProjectDocumentModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("docType")]
    public string DocType { get; set; }

    [JsonPropertyName("docIssueDate")]
    public DateTime DocIssueDate { get; set; }

    [JsonPropertyName("docTitle")]
    public string DocTitle { get; set; }

    [JsonPropertyName("docAbstract")]
    public string DocAbstract { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }

    [JsonPropertyName("docTargets")]
    public List<Department> DocTargets { get; set; }
  }

  //公司用印
  public class CompanySealModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("sealItem")]
    public List<string> SealItem { get; set; }

    [JsonPropertyName("useType")]
    public List<string> UseType { get; set; }

    [JsonPropertyName("sealType")]
    public List<string> SealType { get; set; }

    [JsonPropertyName("docTitle")]
    public string DocTitle { get; set; }

    [JsonPropertyName("docPage")]
    public int DocPage { get; set; }

    [JsonPropertyName("sealCount")]
    public int SealCount { get; set; }

    [JsonPropertyName("useDate")]
    public DateTime UseDate { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //集团用印
  public class CorpSealModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("useType")]
    public List<string> UseType { get; set; }

    [JsonPropertyName("docTitle")]
    public string DocTitle { get; set; }

    [JsonPropertyName("sealType")]
    public List<string> SealType { get; set; }

    [JsonPropertyName("docPage")]
    public int DocPage { get; set; }

    [JsonPropertyName("sealCount")]
    public int SealCount { get; set; }

    [JsonPropertyName("useDate")]
    public DateTime UseDate { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //合同评审
  public class ContractModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("contractType")]
    public string ContractType { get; set; }

    [JsonPropertyName("contractTitle")]
    public string ContractTitle { get; set; }

    [JsonPropertyName("funds")]
    public string Funds { get; set; }

    [JsonPropertyName("signer")]
    public string Signer { get; set; }

    [JsonPropertyName("approvalDate")]
    public DateTime? ApprovalDate { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //补卡
  public class ClockModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("clockType")]
    public string ClockType { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //出差
  public class BusinessTripModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("docTitle")]
    public string DocTitle { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; }

    [JsonPropertyName("vehicle")]
    public string Vehicle { get; set; }

    [JsonPropertyName("sealType")]
    public string SealType { get; set; }

    [JsonPropertyName("startCity")]
    public string StartCity { get; set; }

    [JsonPropertyName("endCity")]
    public string EndCity { get; set; }

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("times")]
    public string Times { get; set; }

    [JsonPropertyName("days")]
    public string Days { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("partner")]
    public string Partner { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //项目留宿
  public class ProjectBestowModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("docTitle")]
    public string DocTitle { get; set; }

    [JsonPropertyName("bestowDate")]
    public DateTime BestowDate { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //报销申请

  public class FeeItem
  {
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("fapiao")]
    public string Fapiao { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    public override string ToString()
    {
      return $"{Title}\t{Fapiao}\t{Value}";
    }
  }

  public class FeeBackModel : IFieldsModel
  {
      [JsonPropertyName("departments")]
      public List<Department> Departments { get; set; }

      [JsonPropertyName("departmentIds")]
      public string DepartmentIds { get; set; }

      [JsonPropertyName("docTitle")]
      public string DocTitle { get; set; }
      [JsonPropertyName("date")]
      public string Date { get; set; }

      [JsonPropertyName("amount")]
      public string Amount { get; set; }
      [JsonPropertyName("description")]
      public string Description { get; set; }

      [JsonPropertyName("attachments")]
      public List<AttachFile> Attachments { get; set; }
  }
  public class FeeModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("docTitle")]
    public string DocTitle { get; set; }

    [JsonPropertyName("startDate")]
    public string StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; }

    [JsonPropertyName("applyType")]
    public string ApplyType { get; set; }

    [JsonPropertyName("paymentType")]
    public string PaymentType { get; set; }

    [JsonPropertyName("items")]
    public List<FeeItem> Items { get; set; }

    [JsonPropertyName("amount")]
    public string Amount { get; set; }

    [JsonPropertyName("applyAmount")]
    public string ApplyAmount { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //收文会签
  public class CompanyReceiveModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("")]
    public string DocType { get; set; }

    [JsonPropertyName("")]
    public DateTime DocIssueDate { get; set; }

    [JsonPropertyName("")]
    public string DocTitle { get; set; }

    [JsonPropertyName("")]
    public string DocSource { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //费用申请
  public class FeeApplyModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("")]
    public string DocTitle { get; set; }

    [JsonPropertyName("")]
    public string ApplyType { get; set; }

    [JsonPropertyName("")]
    public string PaymentType { get; set; }

    [JsonPropertyName("")]
    public string Amount { get; set; }

    [JsonPropertyName("")]
    public string ApplyAmount { get; set; }

    [JsonPropertyName("")]
    public string Description { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //招标项目采购需求审批
  public class InviteBidsModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("docTitle")]
    public string DocTitle { get; set; }

    [JsonPropertyName("")]
    public DateTime PresentationDate { get; set; }

    [JsonPropertyName("")]
    public string Organization { get; set; }

    [JsonPropertyName("")]
    public string ContractName { get; set; }

    [JsonPropertyName("")]
    public string Description { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //外出申请
  public class OutModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("")]
    public string ReturnDate { get; set; }

    [JsonPropertyName("")]
    public string Description { get; set; }

    [JsonPropertyName("")]
    public DateTime? ConfirmDate { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  public class MarketExpansionModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("docType")]
    public string DocType { get; set; }

    [JsonPropertyName("docIssueDate")]
    public DateTime DocIssueDate { get; set; }

    [JsonPropertyName("docTitle")]
    public string DocTitle { get; set; }

    [JsonPropertyName("docAbstract")]
    public string DocAbstract { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //人员异动
  public class PersonnelChangeModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("userName")]
    public string UserName { get; set; }

    [JsonPropertyName("positionName")]
    public string PositionName { get; set; }

    [JsonPropertyName("beforePositionName")]
    public string BeforePositionName { get; set; }

    [JsonPropertyName("laterPositionName")]
    public string LaterPositionName { get; set; }

    [JsonPropertyName("entryDate")]
    public DateTime EntryDate { get; set; }

    [JsonPropertyName("effectiveDate")]
    public DateTime EffectiveDate { get; set; }

    [JsonPropertyName("changeReason")]
    public string ChangeReason { get; set; }

    [JsonPropertyName("remark")]
    public string Remark { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  //用车申请
  public class VehicleApplicationModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("driver")]
    public string Driver { get; set; }

    [JsonPropertyName("applicationTime")]
    public DateTime ApplicationTime { get; set; }

    [JsonPropertyName("returnTime")]
    public DateTime ReturnTime { get; set; }

    [JsonPropertyName("account")]
    public string Account { get; set; }

    [JsonPropertyName("remark")]
    public string Remark { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  public class LicenseModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; }

    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; }

    [JsonPropertyName("features")]
    public List<string> Features { get; set; }

    [JsonPropertyName("machineId")]
    public int MachineId { get; set; }

    [JsonPropertyName("leftDays")]
    public int LeftDays { get; set; }

    [JsonPropertyName("licenseCode")]
    public string LicenseCode { get; set; }

    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  public class ProjectIncomeModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; }
    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; }
    [JsonPropertyName("customerIdentityCode")]
    public string CustomerIdentityCode { get; set; }
    [JsonPropertyName("customerAddress")]
    public string CustomerAddress { get; set; }
    [JsonPropertyName("customerContact")]
    public string CustomerContact { get; set; }
    [JsonPropertyName("customerPhoneNumber")]
    public string CustomerPhoneNumber { get; set; }
    [JsonPropertyName("customerBank")]
    public string CustomerBank { get; set; }
    [JsonPropertyName("customerBankAccount")]
    public string CustomerBankAccount { get; set; }
    [JsonPropertyName("fapiaoType")]
    public string FapiaoType { get; set; }
    [JsonPropertyName("printDate")]
    public string PrintDate {  get; set; }
    [JsonPropertyName("step")]
    public string Step { get; set; }
    [JsonPropertyName("amount")]
    public float Amount { get; set; }
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("remark")]
    public string? Remark { get; set; }
    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }

  public class ProjectOutcomeModel : IFieldsModel
  {
    [JsonPropertyName("departments")]
    public List<Department> Departments { get; set; }

    [JsonPropertyName("departmentIds")]
    public string DepartmentIds { get; set; }

    [JsonPropertyName("projectName")]
    public string ProjectName { get; set; }
    [JsonPropertyName("supplierIdentityCode")]
    public string SupplierIdentityCode { get; set; }
    [JsonPropertyName("supplierName")]
    public string SupplierName { get; set; }
    [JsonPropertyName("supplierContact")]
    public string SupplierContact { get; set; }
    [JsonPropertyName("supplierPhoneNumber")]
    public string SupplierPhoneNumber { get; set; }
    [JsonPropertyName("supplierBank")]
    public string SupplierBank { get; set; }
    [JsonPropertyName("supplierBankAccount")]
    public string SupplierBankAccount { get; set; }
    [JsonPropertyName("fapiaoType")]
    public string FapiaoType { get; set; }
    [JsonPropertyName("printDate")]
    public string ApplyDate { get; set; }
    [JsonPropertyName("step")]
    public string Step { get; set; }
    [JsonPropertyName("amount")]
    public float Amount { get; set; }
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("remark")]
    public string? Remark { get; set; }
    [JsonPropertyName("attachments")]
    public List<AttachFile> Attachments { get; set; }
  }
}