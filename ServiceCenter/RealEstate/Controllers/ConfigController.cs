using CyberStone.Core.Controllers;
using CyberStone.Core.Managers;
using CyberStone.Core.Models;

namespace RealEstate.Controllers
{
  public class ConfigController(ConfigManager<GlobalSettings> configManager) :ConfigControllerBase<GlobalSettings>(configManager)
  {
  }
}
