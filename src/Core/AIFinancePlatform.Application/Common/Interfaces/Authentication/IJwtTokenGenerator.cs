using AIFinancePlatform.Domain.Entities;

namespace AIFinancePlatform.Application.Common.Interfaces.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
