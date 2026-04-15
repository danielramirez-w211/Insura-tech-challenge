namespace InsuraTech.Application.Auth.DTOs;

public sealed record LoginResponse(
    string   Token,
    string   Role,
    string   UserId,
    string   Email,
    string   FirstName,
    string   LastName,
    string?  AdvisorCode,
    DateTime ExpiresAt);
