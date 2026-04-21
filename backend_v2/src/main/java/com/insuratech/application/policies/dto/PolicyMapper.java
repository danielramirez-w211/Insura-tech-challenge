// Spec: SPEC-001..016 — Todas las pólizas
// Origen: PolicyMappingExtensions.cs → PolicyMapper.java
// Paquete: com.insuratech.application.policies.dto
package com.insuratech.application.policies.dto;

import com.insuratech.domain.policies.Policy;
import com.insuratech.domain.ports.PolicyRepository.ClientSummaryProjection;

public final class PolicyMapper {

    private PolicyMapper() {}

    public static PolicyResponse toResponse(Policy p) {
        return new PolicyResponse(
            p.getId(),
            p.getPolicyNumber().getValue(),
            p.getPolicyType().name(),
            p.getStatus().name(),
            p.getInsuredPerson().getFirstName(),
            p.getInsuredPerson().getLastName(),
            p.getInsuredPerson().getDocumentType().name(),
            p.getInsuredPerson().getDocumentNumber(),
            p.getInsuredPerson().getAge(),
            p.getInsuredPerson().getEmail(),
            p.getCoveragePeriod().getStartDate(),
            p.getCoveragePeriod().getEndDate(),
            p.getPremium(),
            p.getInsuredAmount(),
            p.getRemainingInsuredAmount(),
            p.getAgentId(),
            p.getCreatedAt(),
            p.getUpdatedAt(),
            p.getStatusHistory().stream()
                .map(h -> new PolicyStatusHistoryResponse(h.getStatus(), h.getReason(), h.getChangedAt()))
                .toList(),
            toHealthDto(p),
            toVehicleDto(p),
            toTravelDto(p)
        );
    }

    public static ClientSummaryResponse toSummary(ClientSummaryProjection proj) {
        return new ClientSummaryResponse(
            proj.policyId(), proj.policyNumber(),
            proj.insuredFullName(), proj.insuredDocument(),
            proj.status().name(), proj.type().name()
        );
    }

    private static HealthPlanSelectionDto toHealthDto(Policy p) {
        var sel = p.getHealthPlanSelection();
        if (sel == null) return null;
        return new HealthPlanSelectionDto(
            sel.getPlan().getId(), sel.getPlan().getName(),
            sel.getPlan().getBasePremium(), sel.getCalculatedPremium()
        );
    }

    private static VehiclePlanSelectionDto toVehicleDto(Policy p) {
        var sel = p.getVehiclePlanSelection();
        if (sel == null) return null;
        return new VehiclePlanSelectionDto(
            sel.getPlan().getId(), sel.getPlan().getName(),
            sel.getQuotation().getBrand(), sel.getQuotation().getYear(),
            sel.getQuotation().getVehicleValue(), sel.getPlan().getTechnicalRate(),
            sel.getQuotation().isHighRiskBrand(), sel.getCalculatedPremium()
        );
    }

    private static TravelPlanSelectionDto toTravelDto(Policy p) {
        var sel = p.getTravelPlanSelection();
        if (sel == null) return null;
        String continent = sel.getDestinations().isEmpty()
            ? null : sel.getDestinations().get(0).name();
        return new TravelPlanSelectionDto(
            sel.getTripType().name(), continent,
            sel.getNumberOfTravelers(), sel.getCalculatedPremium()
        );
    }
}
