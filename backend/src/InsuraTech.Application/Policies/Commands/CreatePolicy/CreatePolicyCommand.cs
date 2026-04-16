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

        // ── Home (SPEC-012) ───────────────────────────────────────────────────
        /// <summary>ID del paquete base seleccionado (null si es cobertura personalizada). Solo para pólizas Home.</summary>
        public string? HomePlanPackageId { get; init; }
        /// <summary>Valor comercial del predio (COP). Solo para pólizas Home.</summary>
        public decimal? HomePropertyValue { get; init; }
        /// <summary>Año de construcción del inmueble. Solo para pólizas Home.</summary>
        public int? HomeConstructionYear { get; init; }
        /// <summary>Estrato socioeconómico (1-6). Solo para pólizas Home.</summary>
        public int? HomeStratum { get; init; }
        /// <summary>Número de habitantes. Solo para pólizas Home.</summary>
        public int? HomeOccupants { get; init; }
        /// <summary>Tipo de inmueble (House, Apartment, CommercialPremises). Solo para pólizas Home.</summary>
        public string? HomePropertyType { get; init; }
        /// <summary>Coberturas seleccionadas como strings del enum HomeCoverage. Solo para pólizas Home.</summary>
        public IReadOnlyList<string>? HomeSelectedCoverages { get; init; }

        /// <summary>Id del asesor autenticado. Se inyecta desde el controller vía JWT — no viene en el body.</summary>
        public Guid? CreatedByAdvisorId { get; init; }
    }
}
