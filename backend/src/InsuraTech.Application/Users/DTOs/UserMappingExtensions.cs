using InsuraTech.Domain.Users;

namespace InsuraTech.Application.Users.DTOs;

public static class UserMappingExtensions
{
    public static UserResponse ToResponse(this User user) =>
        new()
        {
            Id          = user.Id,
            Email       = user.Email,
            Role        = user.Role.ToString(),
            IsActive    = user.IsActive,
            AdvisorCode = user.AdvisorCode,
            LeaderId    = user.LeaderId,
            Profile     = user.Profile.ToResponse()
        };

    public static UserProfileResponse ToResponse(this UserProfile profile) =>
        new()
        {
            FirstName      = profile.FirstName,
            LastName       = profile.LastName,
            Nationality    = profile.Nationality,
            BirthDate      = profile.BirthDate == DateOnly.MinValue
                                ? string.Empty
                                : profile.BirthDate.ToString("yyyy-MM-dd"),
            YearsInCompany = profile.YearsInCompany,
            PhotoUrl       = profile.PhotoUrl,
            OfficeLocation = profile.OfficeLocation,
            WorkSchedule   = profile.WorkSchedule,
        };
}
