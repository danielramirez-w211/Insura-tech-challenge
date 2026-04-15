using InsuraTech.Application.Common.Interfaces;
using InsuraTech.Domain.Policies.HomePlan;
using InsuraTech.Application.Policies.DTOs;
using InsuraTech.Domain.Interfaces;
using InsuraTech.Domain.Policies;
using InsuraTech.Domain.Policies.ValueObjects;
using MediatR;

namespace InsuraTech.Application.Policies.Commands.CreatePolicy
{
    public sealed class CreatePolicyHandler : IRequestHandler<CreatePolicyCommand, PolicyResponse>
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IUnitOfWork       _unitOfWork;
        private readonly ITrmService       _trmService;

        public CreatePolicyHandler(
            IPolicyRepository policyRepository,
            IUnitOfWork unitOfWork,
            ITrmService trmService)
        {
            _policyRepository = policyRepository;
            _unitOfWork       = unitOfWork;
            _trmService       = trmService;
        }

        public async Task<PolicyResponse> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
        {
            var existing = await _policyRepository
                .GetByIdempotencyKeyAsync(request.IdempotencyKey, cancellationToken);

            if (existing is not null)
                return existing.ToResponse();

            var sequence     = await _policyRepository.GetNextSequenceAsync(cancellationToken);
            var policyNumber = PolicyNumber.Create(DateTime.UtcNow.Year, sequence);

            var insured  = InsuredPerson.Create(
                request.InsuredFirstName, request.InsuredLastName,
                request.InsuredDocumentType, request.InsuredDocumentId,
                request.InsuredBirthDate,
                request.InsuredGender, request.InsuredAddress,
                request.InsuredCityName, request.InsuredPostalCode,
                request.InsuredDepartment);

            Policy policy;

            if (request.Type == PolicyType.Vehicle
                && !string.IsNullOrWhiteSpace(request.VehiclePlanId)
                && request.VehicleCommercialValue.HasValue
                && request.VehicleYear.HasValue
                && !string.IsNullOrWhiteSpace(request.VehicleBrand))
            {
                var coverage = CoveragePeriod.Create(request.CoverageStartDate, request.CoverageEndDate);
                policy = Policy.CreateVehiclePolicy(
                    policyNumber, insured, coverage,
                    request.VehiclePlanId,
                    request.VehicleCommercialValue.Value,
                    request.VehicleYear.Value,
                    request.VehicleBrand,
                    DateOnly.FromDateTime(DateTime.UtcNow));
            }
            else if (request.Type == PolicyType.Life && !string.IsNullOrWhiteSpace(request.LifePlanId))
            {
                var coverage = CoveragePeriod.Create(request.CoverageStartDate, request.CoverageEndDate);
                policy = Policy.CreateLifePolicy(
                    policyNumber, insured, coverage,
                    request.LifePlanId,
                    DateOnly.FromDateTime(DateTime.UtcNow));
            }
            else if (request.Type == PolicyType.Health && !string.IsNullOrWhiteSpace(request.HealthPlanId))
            {
                var coverage = CoveragePeriod.Create(request.CoverageStartDate, request.CoverageEndDate);
                policy = Policy.CreateHealthPolicy(
                    policyNumber, insured, coverage,
                    request.MonthlyPremium, request.HealthPlanId,
                    DateOnly.FromDateTime(DateTime.UtcNow));
            }
            else if (request.Type == PolicyType.Home
                && request.HomePropertyValue.HasValue
                && request.HomeConstructionYear.HasValue
                && request.HomeStratum.HasValue
                && request.HomeOccupants.HasValue
                && !string.IsNullOrWhiteSpace(request.HomePropertyType)
                && request.HomeSelectedCoverages?.Count > 0)
            {
                var coverage     = CoveragePeriod.Create(request.CoverageStartDate, request.CoverageEndDate);
                var propertyType = Enum.Parse<HomePropertyType>(request.HomePropertyType, ignoreCase: true);
                var coverages    = request.HomeSelectedCoverages
                    .Select(c => Enum.Parse<HomeCoverage>(c, ignoreCase: true))
                    .ToList()
                    .AsReadOnly();

                policy = Policy.CreateHomePolicy(
                    policyNumber, insured, coverage,
                    request.HomePlanPackageId,
                    request.HomePropertyValue.Value,
                    request.HomeConstructionYear.Value,
                    request.HomeStratum.Value,
                    request.HomeOccupants.Value,
                    propertyType, coverages,
                    DateOnly.FromDateTime(DateTime.UtcNow));
            }
            else if (request.Type == PolicyType.Travel && request.TripType.HasValue && request.DurationDays.HasValue)
            {
                // Para Travel se usa el constructor directo para evitar la validación de mínimo 30 días
                // (un viaje de 1-29 días es válido en Travel aunque no lo sea en otros productos)
                var coverage = new CoveragePeriod(request.CoverageStartDate, request.CoverageEndDate);

                decimal? trmValue = null;
                DateOnly? trmDate = null;

                if (request.TripType == Domain.Policies.TravelPlan.TripType.Internacional)
                {
                    var trm  = await _trmService.GetCurrentTrmAsync(cancellationToken);
                    trmValue = trm.ValueCop;
                    trmDate  = trm.Date;
                }

                policy = Policy.CreateTravelPolicy(
                    policyNumber, insured, coverage,
                    request.TripType.Value, request.Continent,
                    request.DurationDays.Value, trmValue, trmDate);
            }
            else
            {
                var coverage = CoveragePeriod.Create(request.CoverageStartDate, request.CoverageEndDate);
                policy = Policy.Create(
                    policyNumber, request.Type, insured, coverage,
                    request.MonthlyPremium, request.InsuredAmount);
            }

            await _policyRepository.AddAsync(policy, cancellationToken);
            _policyRepository.SetIdempotencyKey(policy, request.IdempotencyKey);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return policy.ToResponse();
        }
    }
}
