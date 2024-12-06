namespace PlatformAPI.Services;

public record AppCreationRequest(string AppName, string Stack);
public record AppCreationResponse(string RepositoryUrl, string Status, string AzureAppClientId);

public record CreateApplicationRequest(string displayName);

public record CreateApplicationResponse(string id, string appId);

public record FederatedIdentityCredentialRequest(string name, string issuer, string subject, string[] audiences, string description);