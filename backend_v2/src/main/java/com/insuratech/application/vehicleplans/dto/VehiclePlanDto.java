package com.insuratech.application.vehicleplans.dto;

import java.math.BigDecimal;
import java.util.List;

public record VehiclePlanDto(
    String planId,
    String planName,
    BigDecimal priceMultiplier,
    List<String> coverages,
    List<String> assistances
) {}
