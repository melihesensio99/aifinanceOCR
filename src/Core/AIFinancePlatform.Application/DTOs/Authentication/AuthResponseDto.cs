using System;

using System.Text.Json.Serialization;

namespace AIFinancePlatform.Application.DTOs.Authentication;

public record AuthResponseDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    string Token,
    [property: JsonIgnore] string RefreshToken
);
