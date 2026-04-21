package com.insuratech.domain.policies.life;

import java.math.BigDecimal;
import java.math.RoundingMode;

public final class LifePlanPricingService {

    private LifePlanPricingService() {}

    public static BigDecimal calculatePremium(LifePlan plan, int age, boolean smoker) {
        BigDecimal base = plan.getBasePremium();
        BigDecimal ageSurcharge = getAgeSurcharge(age);
        BigDecimal smokerSurcharge = smoker ? new BigDecimal("0.25") : BigDecimal.ZERO;
        return base.multiply(BigDecimal.ONE.add(ageSurcharge).add(smokerSurcharge))
                   .setScale(0, RoundingMode.HALF_UP);
    }

    private static BigDecimal getAgeSurcharge(int age) {
        if (age < 35) return BigDecimal.ZERO;
        if (age < 45) return new BigDecimal("0.10");
        if (age < 55) return new BigDecimal("0.20");
        return new BigDecimal("0.35");
    }
}
