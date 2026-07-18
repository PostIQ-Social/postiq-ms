namespace User.Application.Response
{
    /// <summary>
    /// Returned after a successful registration. Carries the new user's id, the
    /// auth id issued by the Identity service and the user's own referral code.
    /// </summary>
    public record RegisterUserResponse(long UserId, Guid AuthId, string ReferralCode);
}
