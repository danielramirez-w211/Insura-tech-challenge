namespace InsuraTech.Application.Policies.Commands.ActivatePolicy;

using InsuraTech.Application.Policies.DTOs;
using MediatR;

public sealed record ActivatePolicyCommand : IRequest<PolicyResponse>
{
    public Guid PolicyId { get; init; }
}