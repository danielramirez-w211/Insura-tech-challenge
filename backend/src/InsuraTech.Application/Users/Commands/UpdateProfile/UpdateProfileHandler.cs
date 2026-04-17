using InsuraTech.Application.Users.DTOs;
using InsuraTech.Domain.Users;
using MediatR;

namespace InsuraTech.Application.Users.Commands.UpdateProfile;

public sealed class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, UserResponse>
{
    private readonly IUserRepository _users;

    public UpdateProfileHandler(IUserRepository users) => _users = users;

    public async Task<UserResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"Usuario '{request.UserId}' no encontrado.");

        DateOnly.TryParse(request.BirthDate, out var birthDate);

        var updatedProfile = new UserProfile(
            FirstName:      request.FirstName,
            LastName:       request.LastName,
            Nationality:    request.Nationality,
            BirthDate:      birthDate,
            YearsInCompany: request.YearsInCompany,
            PhotoUrl:       request.PhotoUrl,
            OfficeLocation: request.OfficeLocation,
            WorkSchedule:   request.WorkSchedule);

        user.UpdateProfile(updatedProfile);
        await _users.UpdateAsync(user, cancellationToken);

        return user.ToResponse();
    }
}
