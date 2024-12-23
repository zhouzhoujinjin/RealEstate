using Approval.Models;

namespace Approval.Interface
{
  public interface IApprovalHook
  {
    public Task<Dictionary<string, string>> ExecuteAsync(Dictionary<string, string> formData, Type dataType);
  }
}