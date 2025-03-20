namespace PlatformAPI.Services;

public record AppCreationRequest(string AppName, string Stack);
public record AppCreationResponse(string RepositoryUrl, string Status);

public record AppRegistrationRequest(string AppName, string repositoryUrl);
public record AppRegistrationResponse(string AppId, string TenantId, string SubscriptionId, string ClientId);

public record AppAddSecretsRequest(string AppName, string TenantId, string SubscriptionId, string ClientId);
public record AppAddSecretsResponse(string Status);

public record CreateApplicationRequest(string displayName);

public record CreateApplicationResponse(string id, string appId);

public record FederatedIdentityCredentialRequest(string name, string issuer, string subject, string[] audiences, string description);