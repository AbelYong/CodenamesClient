using CodenamesGame.Domain.POCO;
using CodenamesGame.EmailService;
using System;

namespace CodenamesClient.Operation.Network.Oneway
{
    public interface IOnewayNetworkManager
    {
        // Authentication Service
        CodenamesGame.AuthenticationService.AuthenticationRequest Authenticate(string username, string password);
        CodenamesGame.AuthenticationService.CommunicationRequest CompletePasswordReset(string email, string code, string newPassword);
        CodenamesGame.AuthenticationService.CommunicationRequest UpdatePassword(string username, string currentPassword, string newPassword);

        // User Service
        CodenamesGame.UserService.SignInRequest SignIn(UserDM user, PlayerDM player);
        PlayerDM GetPlayer(Guid userID);
        CodenamesGame.UserService.CommunicationRequest UpdateProfile(PlayerDM player);

        // Email Service
        CodenamesGame.EmailService.CommunicationRequest SendVerificationEmail(string email, EmailType emailType);
        CodenamesGame.EmailService.ConfirmEmailRequest SendVerificationCode(string email, string code, EmailType emailType);

        // Moderation Service
        CodenamesGame.ModerationService.CommunicationRequest ReportPlayer(Guid reporterUserID, Guid reportedUserID, string reason);
    }
}
