using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodenamesGame.ModerationService;

namespace CodenamesGame.Network.Proxies.Interfaces
{
    public interface IModerationProxy
    {
        CommunicationRequest ReportPlayer(Guid reporterUserID, Guid reportedUserID, string reason);
    }
}
