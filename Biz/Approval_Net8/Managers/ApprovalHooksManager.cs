using Approval.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Approval.Managers
{

  public class ApprovalHooksManager
  {
    private IEnumerable<IApprovalHook> services;

    public ApprovalHooksManager(IServiceProvider provider)
    {
      services = provider.GetServices<IApprovalHook>();
    }

    public async Task<Dictionary<string, string>> ExecuteAsync(string typeName, Dictionary<string, string> formData, Type dataType)
    {
      var instance = services.Where(x => x.GetType().FullName == typeName).FirstOrDefault();
      if (instance != null)
      {
        try
        {
          var result = await instance.ExecuteAsync(formData, dataType);
          return result;
        } catch
        {

        }
      }
      return formData;
    }
  }
}
