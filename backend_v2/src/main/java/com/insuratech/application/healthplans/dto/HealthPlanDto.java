package com.insuratech.application.healthplans.dto;

import java.math.BigDecimal;

public record HealthPlanDto(String planId, String planName, BigDecimal baseAmount) {}
