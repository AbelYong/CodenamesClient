using CodenamesGame.Domain.POCO;
using CodenamesGame.EmailService;
using CodenamesGame.Network;
using System;

namespace CodenamesClient.Operation.Network.Oneway
{
    public class OnewayNetworkManager : IOnewayNetworkManager
    {
        private static readonly Lazy<OnewayNetworkManager> _instance = new Lazy<OnewayNetworkManager>(() => new OnewayNetworkManager());
        private readonly AuthenticationOperation _authenticationOperation;
        private readonly UserOperation _userOperation;
        private readonly EmailOperation _emailOperation;
        private readonly ModerationOperation _moderationOperation;

        public static OnewayNetworkManager Instance
        {
            get => _instance.Value;
        }

        private OnewayNetworkManager()
        {
            _authenticationOperation = new AuthenticationOperation();
            _userOperation = new UserOperation();
            _emailOperation = new EmailOperation();
            _moderationOperation = new ModerationOperation();
        }

        // Authentication Service
        public CodenamesGame.AuthenticationService.AuthenticationRequest Authenticate(string username, string password)
        {
            return _authenticationOperation.Authenticate(username, password);
        }

        public CodenamesGame.AuthenticationService.CommunicationRequest CompletePasswordReset(string email, string code, string newPassword)
        {
            return _authenticationOperation.CompletePasswordReset(email, code, newPassword);
        }

        public CodenamesGame.AuthenticationService.CommunicationRequest UpdatePassword(string username, string currentPassword, string newPassword)
        {
            return _authenticationOperation.UpdatePassword(username, currentPassword, newPassword);
        }

        // User service
        public CodenamesGame.UserService.SignInRequest SignIn(UserDM user, PlayerDM player)
        {
            return _userOperation.SignIn(user, player);
        }

        public PlayerDM GetPlayer(Guid userID)
        {
            return _userOperation.GetPlayer(userID);
        }

        public CodenamesGame.UserService.CommunicationRequest UpdateProfile(PlayerDM player)
        {
            return _userOperation.UpdateProfile(player);
        }

        //Email Service
        public CodenamesGame.EmailService.CommunicationRequest SendVerificationEmail(string email, EmailType emailType)
        {
            return _emailOperation.SendVerificationEmail(email, emailType);
        }

        public CodenamesGame.EmailService.ConfirmEmailRequest SendVerificationCode(string email, string code, EmailType emailType)
        {
            return _emailOperation.SendVerificationCode(email, code, emailType);
        }

        public CodenamesGame.ModerationService.CommunicationRequest ReportPlayer(Guid reporterUserID, Guid reportedUserID, string reason)
        {
            return _moderationOperation.ReportPlayer(reporterUserID, reportedUserID, reason);
        }
    }
}
