package com.insuratech.application.policies.dto;

import java.math.BigDecimal;

public record VehiclePlanSelectionDto(
    String planId, String planName,
    String vehicleBrand, int vehicleYear,
    BigDecimal commercialValue, BigDecimal technicalRate,
    boolean hasBrandSurcharge, BigDecimal calculatedPremium
) {}
