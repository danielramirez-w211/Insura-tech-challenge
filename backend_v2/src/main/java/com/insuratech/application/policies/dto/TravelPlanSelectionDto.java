package com.insuratech.application.policies.dto;

import java.math.BigDecimal;

public record TravelPlanSelectionDto(
    String tripType, String continent,
    int numberOfTravelers, BigDecimal calculatedPremium
) {}
