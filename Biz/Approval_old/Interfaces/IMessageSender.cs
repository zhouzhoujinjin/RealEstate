using System;
using System.Collections.Generic;
using System.Text;

namespace Approval.Interfaces
{
  public interface IMessageSender
  {
    public void Send(long userId, ApprovalActionType actionType, string title, string content, string url);
  }
}
