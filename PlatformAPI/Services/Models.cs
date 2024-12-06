namespace PlatformAPI.Services;

public record AppCreationRequest(string AppName, string Stack);
public record AppCreationResponse(string RepositoryUrl, string Status);