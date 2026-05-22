using System;

namespace AIFinancePlatform.Application.DTOs.Categories;

public record CategoryDto(
    Guid Id,
    string Name,
    string Icon,
    string ColorHex,
    bool IsDefault
);
