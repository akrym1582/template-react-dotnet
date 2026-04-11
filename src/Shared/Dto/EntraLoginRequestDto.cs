namespace Shared.Dto;

/// <summary>
/// JWT token obtained from Azure Entra ID on the client side.
/// The API validates this token and issues a cookie session.
/// </summary>
public record EntraLoginRequestDto(string IdToken);
