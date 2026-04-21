package com.insuratech.application.vehicleplans.dto;

import java.math.BigDecimal;
import java.util.List;

public record VehicleQuotationDto(
    BigDecimal commercialValue,
    int vehicleYear,
    String brand,
    int vehicleAge,
    String ageCategory,
    BigDecimal technicalRate,
    boolean hasBrandSurcharge,
    BigDecimal baseMonthlyPremium,
    List<VehiclePlanOptionDto> plans
) {}
