package com.insuratech.application.travelplans.dto;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.Instant;

public record TravelPlanCalculationDto(
    String tripType,
    String continent,
    int durationDays,
    BigDecimal basePriceUsd,
    BigDecimal basePriceCop,
    BigDecimal dailyIncrementCop,
    BigDecimal totalPriceCop,
    BigDecimal trmUsed,
    LocalDate trmDate,
    Instant calculatedAt
) {}
