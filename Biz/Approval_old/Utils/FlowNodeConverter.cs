using Approval.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Approval.Utils
{
  public class FlowNodeJsonConverter : JsonConverter
  {
    public override bool CanWrite => false;
    public override bool CanConvert(Type objectType)
    {
      return objectType == typeof(FlowNode);
    }

    public override bool CanRead => true;

    public override object ReadJson(JsonReader reader,
        Type objectType, object existingValue,
        JsonSerializer serializer)
    {
      var profession = default(FlowNode);
      try
      {
        var jsonObject = JObject.Load(reader);
        var value = jsonObject["type"];
        switch (value.Value<string>())
        {
          case "approval":
          case "approver":
            profession = new ApprovalNode();
            break;

          case "start":
            profession = new StartNode();
            break;

          case "cc":
            profession = new CarbonCopyNode();
            break;
        }
        if(profession != null)
        {
          serializer.Populate(jsonObject.CreateReader(), profession);
        }
      } catch (Exception e) {
        
      }
      return profession;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }
  }
}
