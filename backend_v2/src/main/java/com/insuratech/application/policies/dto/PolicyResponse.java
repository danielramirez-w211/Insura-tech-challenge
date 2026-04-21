package com.insuratech.application.policies.dto;

import java.math.BigDecimal;
import java.time.Instant;
import java.time.LocalDate;
import java.util.List;

public record PolicyResponse(
    String id,
    String policyNumber,
    String type,
    String status,
    String insuredFirstName,
    String insuredLastName,
    String insuredDocumentType,
    String insuredDocumentId,
    int insuredAge,
    String insuredEmail,
    LocalDate coverageStartDate,
    LocalDate coverageEndDate,
    BigDecimal premium,
    BigDecimal insuredAmount,
    BigDecimal remainingInsuredAmount,
    String agentId,
    Instant createdAt,
    Instant updatedAt,
    List<PolicyStatusHistoryResponse> statusHistory,
    HealthPlanSelectionDto healthPlan,
    VehiclePlanSelectionDto vehiclePlan,
    TravelPlanSelectionDto travelPlan
) {}
