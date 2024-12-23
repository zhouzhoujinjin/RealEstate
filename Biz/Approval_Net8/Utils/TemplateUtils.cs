using Approval.Models;

namespace Approval.Utils
{
  public static class TemplateUtils
  {

    public static Type GetTemplateModelType(string templateName)
    {
      return templateName switch
      {
        "leave" => typeof(LeaveModel),
        "payment" => typeof(PaymentModel),
        "company-document" => typeof(CompanyDocumentModel),
        "overtime" => typeof(OvertimeModel),
        "purchase" => typeof(PurchaseModel),
        "project-document" => typeof(ProjectDocumentModel),
        "company-seal" => typeof(CompanySealModel),
        "corp-seal" => typeof(CorpSealModel),
        "contract" => typeof(ContractModel),
        "clock" => typeof(ClockModel),
        "business-trip" => typeof(BusinessTripModel),
        "project-bestow" => typeof(ProjectBestowModel),
        "fee" => typeof(FeeModel),
        "fee-back" => typeof(FeeBackModel),
        "company-receive" => typeof(CompanyReceiveModel),
        "fee-apply" => typeof(FeeApplyModel),
        "invite-bids" => typeof(InviteBidsModel),
        "out" => typeof(OutModel),
        "market-expansion" => typeof(MarketExpansionModel),
        "personnel-change" => typeof(PersonnelChangeModel),
        "vehicle-application" => typeof(VehicleApplicationModel),
        "apply-license" => typeof(LicenseModel),
        "project-income" => typeof(ProjectIncomeModel),
        "project-outcome" => typeof(ProjectOutcomeModel),

        _ => throw new NotImplementedException(),
      };
    }
  }
}
