using Approval.Models;
using System;
using System.Collections.Generic;
using System.Text;

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
        "chancheng-contract" => typeof(ChanchengContractModel),
        "chancheng-bid" => typeof(ChanchenBidModel),
        "chancheng-payment" => typeof(ChanchengPaymentModel),
        "contract" => typeof(ContractModel),
        "clock" => typeof(ClockModel),
        "business-trip" => typeof(BusinessTripModel),
        "project-bestow" => typeof(ProjectBestowModel),
        "fee" => typeof(FeeModel),
        "company-receive" => typeof(CompanyReceiveModel),
        "fee-apply" => typeof(FeeApplyModel),
        "invite-bids" => typeof(InviteBidsModel),
        "out" => typeof(OutModel),
        "performance" => typeof(PerformanceModel),
        "market-expansion" => typeof(MarketExpansionModel),
        "yiyaogu-contract" => typeof(YiYaoGuContractModel),
        "yiyaogu-bid" => typeof(YiYaoGuBidModel),
        "yiyaogu-payment" => typeof(YiYaoGuPaymentModel),
        "yiyaogu-invite-bids" => typeof(YiYaoGuInviteBidsModel),
        "personnel-change" => typeof(PersonnelChangeModel),
        "vehicle-application" => typeof(VehicleApplicationModel),
        "preparation-application" => typeof(PreparationApplicationModel),
        _ => throw new NotImplementedException(),
      };
    }
  }
}
