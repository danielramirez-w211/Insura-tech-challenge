package com.insuratech.domain.policies.health;

import java.math.BigDecimal;
import java.util.List;
import java.util.Optional;

public final class HealthPlanCatalog {

    private static final List<HealthPlan> PLANS = List.of(
        new HealthPlan("HEALTH-BASIC", "Basic Health", "Basic coverage for general consultations",
            new BigDecimal("150000"), new BigDecimal("10000000"), false, false, false),
        new HealthPlan("HEALTH-STANDARD", "Standard Health", "Standard coverage with dental and vision",
            new BigDecimal("280000"), new BigDecimal("25000000"), true, true, false),
        new HealthPlan("HEALTH-PREMIUM", "Premium Health", "Full coverage including mental health",
            new BigDecimal("420000"), new BigDecimal("50000000"), true, true, true),
        new HealthPlan("HEALTH-ELITE", "Elite Health", "Maximum coverage with no deductibles",
            new BigDecimal("650000"), new BigDecimal("100000000"), true, true, true)
    );

    private HealthPlanCatalog() {}

    public static List<HealthPlan> getAll() {
        return PLANS;
    }

    public static Optional<HealthPlan> findById(String id) {
        return PLANS.stream().filter(p -> p.getId().equals(id)).findFirst();
    }
}
