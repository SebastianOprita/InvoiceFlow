namespace InvoiceFlow.BuildingBlocks.Authorization.Claims;

public static class InvoiceFlowClaimTypes
{
    public const string PrincipalType = "principal_type";
    public const string AuthMode = "auth_mode";

    public const string UserId = "user_id";
    public const string EmailAddress = "email_address";
    public const string TenantId = "tenant_id";
    public const string Permissions = "permissions";

    public const string ActorPrincipalType = "actor_principal_type";
    public const string ActorUserId = "actor_user_id";
    public const string ActorEmailAddress = "actor_email_address";

    public const string ImpersonationSessionId = "impersonation_session_id";
    public const string ImpersonationStartedAt = "impersonation_started_at";
    public const string ImpersonationReason = "impersonation_reason";
}
