package com.insuratech.domain.policies.health;

import java.math.BigDecimal;
import java.math.RoundingMode;

public final class HealthPlanPricingService {

    private HealthPlanPricingService() {}

    public static BigDecimal calculatePremium(HealthPlan plan, int age) {
        BigDecimal base = plan.getBasePremium();
        BigDecimal ageFactor = getAgeFactor(age);
        return base.multiply(BigDecimal.ONE.add(ageFactor)).setScale(0, RoundingMode.HALF_UP);
    }

    private static BigDecimal getAgeFactor(int age) {
        if (age < 30) return BigDecimal.ZERO;
        if (age < 50) return new BigDecimal("0.04");
        return new BigDecimal("0.08");
    }
}
