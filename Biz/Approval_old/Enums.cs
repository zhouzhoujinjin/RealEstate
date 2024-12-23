using System;
using System.Collections.Generic;
using System.Text;

namespace Approval
{
  public enum ApprovalActionType
  {
    Created = 1,
    Pending,
    Approved,
    Rejected,
    /// <summary>
    /// 评论，不会保存到数据库
    /// </summary>
    Comment
  }

  public enum ApprovalFlowNodeType
  {
    Start = 1,
    Approval,
    Cc,
    And,
    Or,
    Sub
  }

  public enum ApprovalItemStatus
  {
    Draft = 1,
    Approving,
    Approved,
    Rejected,
    Cancel,
    Upload
  }

  public enum LetterType
  {
    Lower,
    Upper
  }

  public enum TemplateGroup
  {
    /// <summary>
    /// 考勤类
    /// </summary>
    Present = 1,
    /// <summary>
    /// 财务类
    /// </summary>
    Finance,
    /// <summary>
    /// 行政类
    /// </summary>
    Official,
    /// <summary>
    /// 业务类
    /// </summary>
    Business
  }


  public static class PublishTypeCode
  {
    public const string Notification = "01";
    public const string Announcement = "02";
    public const string InBoxFile = "03";
    public const string OutBoxFile = "04";
  }

}
