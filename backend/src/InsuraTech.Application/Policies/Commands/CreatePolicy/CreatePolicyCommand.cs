using InsuraTech.Application.Policies.DTOs;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.TravelPlan;
using MediatR;

namespace InsuraTech.Application.Policies.Commands.CreatePolicy
{
    public sealed record CreatePolicyCommand : IRequest<PolicyResponse>
    {
        public string IdempotencyKey { get; init; } = null!;
        public PolicyType Type { get; init; }

        public string InsuredFirstName { get; init; } = null!;
        public string InsuredLastName { get; init; } = null!;

        public string InsuredFullName {get; init;} = null!;

        public string InsuredDocumentType {get; init;} = null!;

        public string InsuredDocumentId { get; init; } = null!;
        public DateOnly InsuredBirthDate { get; init; }
        public string InsuredGender { get; init; } = null!;
        public string InsuredAddress { get; init; } = null!;
        public string InsuredCityName { get; init; } = null!;
        public string InsuredPostalCode { get; init; } = null!;
        public string InsuredDepartment { get; init; } = null!;
        public DateOnly CoverageStartDate { get; init; }
        public DateOnly CoverageEndDate { get; init; }
        public decimal MonthlyPremium { get; init; }
        public decimal InsuredAmount { get; init; }
        /// <summary>Solo para pólizas de tipo Health.</summary>
        public string? HealthPlanId { get; init; }
        /// <summary>Solo para pólizas de tipo Life.</summary>
        public string? LifePlanId { get; init; }
        /// <summary>Solo para pólizas de tipo Travel.</summary>
        public TripType? TripType { get; init; }
        /// <summary>Solo para pólizas de tipo Travel Internacional.</summary>
        public Continent? Continent { get; init; }
        /// <summary>Solo para pólizas de tipo Travel.</summary>
        public int? DurationDays { get; init; }
        /// <summary>Solo para pólizas de tipo Vehicle.</summary>
        public string? VehiclePlanId { get; init; }
        /// <summary>Valor comercial del vehículo (COP). Solo para pólizas Vehicle.</summary>
        public decimal? VehicleCommercialValue { get; init; }
        /// <summary>Año de fabricación del vehículo. Solo para pólizas Vehicle.</summary>
        public int? VehicleYear { get; init; }
        /// <summary>Marca del vehículo. Solo para pólizas Vehicle.</summary>
        public string? VehicleBrand { get; init; }
    }
}
