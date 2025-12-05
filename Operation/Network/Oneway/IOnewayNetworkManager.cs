using CodenamesGame.Domain.POCO;
using System;

namespace CodenamesClient.Operation.Network.Oneway
{
    public interface IOnewayNetworkManager
    {
        // Authentication Service
        CodenamesGame.AuthenticationService.LoginRequest Authenticate(string username, string password);
        void BeginPasswordReset(string username, string email);
        CodenamesGame.AuthenticationService.ResetResult CompletePasswordReset(string username, string code, string newPassword);
        
        // User Service
        CodenamesGame.UserService.SignInRequest SignIn(UserDM user, PlayerDM player);
        PlayerDM GetPlayer(Guid userID);
        CodenamesGame.UserService.CommunicationRequest UpdateProfile(PlayerDM player);

        // Email Service
        CodenamesGame.EmailService.CommunicationRequest SendVerificationEmail(string email);
        CodenamesGame.EmailService.ConfirmEmailRequest SendVerificationCode(string email, string code);

        // Moderation Service
        CodenamesGame.ModerationService.CommunicationRequest ReportPlayer(Guid reporterUserID, Guid reportedUserID, string reason);
    }
}
