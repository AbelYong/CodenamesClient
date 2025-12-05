using CodenamesGame.ModerationService;
using CodenamesGame.Network.Proxies.Interfaces;
using CodenamesGame.Network.Proxies.Wrappers;
using System;
using System.ServiceModel;

namespace CodenamesGame.Network
{
    public class ModerationOperation
    {
        private readonly IModerationProxy _proxy;

        public ModerationOperation() : this (new ModerationProxy()) { }

        public ModerationOperation(IModerationProxy proxy)
        {
            _proxy = proxy;
        }

        public CommunicationRequest ReportPlayer(Guid reporterUserID, Guid reportedUserID, string reason)
        {
            return _proxy.ReportPlayer(reporterUserID, reportedUserID, reason);
        }
    }
}