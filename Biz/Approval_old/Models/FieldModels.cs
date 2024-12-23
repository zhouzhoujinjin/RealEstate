using Approval.Managers;
using PureCode.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Models
{
  public interface IFieldsModel
  {
    public List<Department> Departments { get; }
    public string DepartmentIds { get; set; }
    public List<SeafileFileInfo> Attachments { get; }
  }
  //请假
  public class LeaveModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocTitle { get; set; }
    public string LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Days { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //付款申请
  public class PaymentModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocTitle { get; set; }
    public string PaymentType { get; set; }
    public string Amount { get; set; }
    public string PaymentWay { get; set; }
    public string Organization { get; set; }
    public string Bank { get; set; }
    public string AccountNumber { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //公司发文
  public class CompanyDocumentModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocType { get; set; }
    public DateTime DocIssueDate { get; set; }
    public string DocTitle { get; set; }
    public string DocAbstract { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
    public List<Client> DocTargets { get; set; }
  }
  //加班
  public class OvertimeModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Days { get; set; }
    public string Description { get; set; }
    public DateTime? FinishDate { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //采购申请
  public class PurchaseModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocTitle { get; set; }
    public string PurchaseType { get; set; }
    public string Amount { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //项目发文
  public class ProjectDocumentModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocType { get; set; }
    public DateTime DocIssueDate { get; set; }
    public string DocTitle { get; set; }
    public string DocAbstract { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
    public List<Department> DocTargets { get; set; }
  }
  //公司用印
  public class CompanySealModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public List<string> SealItem { get; set; }
    public List<string> UseType { get; set; }
    public List<string> SealType { get; set; }
    public string DocTitle { get; set; }
    public int DocPage { get; set; }
    public int SealCount { get; set; }
    public DateTime UseDate { get; set; }
    public string Description { get; set; }

    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //集团用印
  public class CorpSealModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public List<string> UseType { get; set; }
    public string DocTitle { get; set; }
    public List<string> SealType { get; set; }
    public int DocPage { get; set; }
    public int SealCount { get; set; }
    public DateTime UseDate { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //产城甲方合同预审
  public class ChanchengContractModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string ContractType { get; set; }
    public string ContractTitle { get; set; }
    public string Stage { get; set; }
    public string Funds { get; set; }
    public string Signer { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string Approver { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //产城招标事项
  public class ChanchenBidModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string ProjectTitle { get; set; }
    public List<string> Stage { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //产城请付款预审
  public class ChanchengPaymentModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string ReviewType { get; set; }
    public string ContractType { get; set; }
    public string ContractTitle { get; set; }
    public string ContractNo { get; set; }
    public string Stage { get; set; }
    public string Funds { get; set; }
    public string ProviderTitle { get; set; }
    public string Finished { get; set; }
    public string ContractNode { get; set; }
    public string LastFunds { get; set; }
    public string LastPercent { get; set; }
    public string PayType { get; set; }
    public string PayStage { get; set; }
    public string ThisFunds { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //合同评审
  public class ContractModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string ContractType { get; set; }
    public string ContractTitle { get; set; }
    public string Funds { get; set; }
    public string Signer { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //补卡
  public class ClockModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string ClockType { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //出差
  public class BusinessTripModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocTitle { get; set; }
    public string Reason { get; set; }
    public string Vehicle { get; set; }
    public string SealType { get; set; }
    public string StartCity { get; set; }
    public string EndCity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Times { get; set; }
    public string Days { get; set; }
    public string Description { get; set; }
    public string Partner { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //项目留宿
  public class ProjectBestowModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocTitle { get; set; }
    public DateTime BestowDate { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //报销申请
  public class FeeModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocTitle { get; set; }
    public string ApplyType { get; set; }
    public string PaymentType { get; set; }
    public string Amount { get; set; }
    public string ApplyAmount { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //收文会签
  public class CompanyReceiveModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocType { get; set; }
    public DateTime DocIssueDate { get; set; }
    public string DocTitle { get; set; }
    public string DocSource { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //费用申请
  public class FeeApplyModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocTitle { get; set; }
    public string ApplyType { get; set; }
    public string PaymentType { get; set; }
    public string Amount { get; set; }
    public string ApplyAmount { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //招标项目采购需求审批
  public class InviteBidsModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocTitle { get; set; }
    public DateTime PresentationDate { get; set; }
    public string Organization { get; set; }
    public string ContractName { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //外出申请
  public class OutModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public DateTime StartDate { get; set; }
    public string ReturnDate { get; set; }
    public string Description { get; set; }
    public DateTime? ConfirmDate { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //绩效考核结果审批
  public class PerformanceModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocTitle { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }

  public class MarketExpansionModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocType { get; set; }
    public DateTime DocIssueDate { get; set; }
    public string DocTitle { get; set; }
    public string DocAbstract { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }


  //医药谷甲方合同预审
  public class YiYaoGuContractModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string ContractType { get; set; }
    public string ContractTitle { get; set; }
    public string Funds { get; set; }
    public string Signer { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string Approver { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //医药谷招标事项
  public class YiYaoGuBidModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string ProjectTitle { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //医药谷请付款预审
  public class YiYaoGuPaymentModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string ReviewType { get; set; }
    public string ContractType { get; set; }
    public string ContractTitle { get; set; }
    public string ContractNo { get; set; }
    public string Funds { get; set; }
    public string ProviderTitle { get; set; }
    public string Finished { get; set; }
    public string ContractNode { get; set; }
    public string LastFunds { get; set; }
    public string LastPercent { get; set; }
    public string PayType { get; set; }
    public string PayStage { get; set; }
    public string ThisFunds { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //医药谷招标项目采购需求审批
  public class YiYaoGuInviteBidsModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string DocTitle { get; set; }
    public DateTime PresentationDate { get; set; }
    public string Organization { get; set; }
    public string ContractName { get; set; }
    public string Description { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //人员异动
  public class PersonnelChangeModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string Type { get; set; }
    public string UserName { get; set; }
    public string PositionName { get; set; }
    public string BeforePositionName { get; set; }
    public string LaterPositionName { get; set; }
    public DateTime EntryDate { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string ChangeReason { get; set; }
    public string Remark { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }
  //用车申请
  public class VehicleApplicationModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public string Driver { get; set; }
    public DateTime ApplicationTime { get; set; }
    public DateTime ReturnTime { get; set; }
    public string Account { get; set; }
    public string Remark { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }

  //人员编制申请
  public class PreparationApplicationModel : IFieldsModel
  {
    public List<Department> Departments { get; set; }
    public string DepartmentIds { get; set; }
    public DateTime ApplicationTime { get; set; }
    public string NumberPeople { get; set; }
    public string ApplicantDepartment { get; set; }
    public string Remark { get; set; }
    public List<SeafileFileInfo> Attachments { get; set; }
  }

}
