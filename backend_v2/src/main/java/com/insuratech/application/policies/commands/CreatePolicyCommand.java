package com.insuratech.application.policies.commands;

import com.insuratech.domain.policies.PolicyType;
import com.insuratech.domain.policies.enums.DocumentType;
import com.insuratech.domain.policies.travel.Continent;
import com.insuratech.domain.policies.travel.TripType;
import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.List;

public record CreatePolicyCommand(
    @NotNull PolicyType type,
    @NotBlank String insuredFirstName,
    @NotBlank String insuredLastName,
    @NotNull DocumentType insuredDocumentType,
    @NotBlank String insuredDocumentId,
    @NotNull LocalDate insuredBirthDate,
    @NotBlank String insuredEmail,
    String insuredPhone,
    @NotNull LocalDate coverageStartDate,
    @NotNull LocalDate coverageEndDate,
    BigDecimal monthlyPremium,
    BigDecimal insuredAmount,
    String agentId,
    String cityId,
    // Health
    String healthPlanId,
    // Life
    String lifePlanId,
    Boolean smoker,
    // Vehicle
    String vehiclePlanId,
    BigDecimal vehicleCommercialValue,
    Integer vehicleYear,
    String vehicleBrand,
    String vehicleModel,
    // Home
    String homePlanPackageId,
    BigDecimal homePropertyValue,
    Integer homeConstructionYear,
    String homePropertyType,
    Boolean homeHasSecuritySystem,
    // Travel
    TripType tripType,
    List<Continent> travelDestinations,
    Integer numberOfTravelers
) {}
