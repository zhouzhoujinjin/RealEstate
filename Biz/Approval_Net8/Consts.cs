using System;
using System.Reflection.Emit;

namespace Approval
{
  public static class Consts
  {
    public const string SinglePicker = "single-picker";
    public const string Input = "input";
    public const string MultiPicker = "input";
    public const string File = "file";
    public const string Date = "date";
    public const string Time = "time";
    public const string Seafile = "seafile";

    public const string FlowNodeTypeApproval = "approval";
    public const string FlowNodeTypeStart = "start";
    public const string FlowNodeTypeCarbonCopy = "cc";
    public const string FlowNodeTypeCondition = "condition";

    public const string ApprovalAssigneeTypeUser = "director";
    public const string ApprovalAssigneeTypePosition = "position";

    public const string ApprovalLeave = "leave";

    public const string UserProfileWithDepartmentCacheKey = ":UserWithDept";

  }
}
