package com.insuratech.domain.policies.life;

import java.math.BigDecimal;
import java.util.List;
import java.util.Optional;

public final class LifePlanCatalog {

    private static final List<LifePlan> PLANS = List.of(
        new LifePlan("LIFE-BASIC", "Basic Life", "Basic life insurance",
            new BigDecimal("80000"), new BigDecimal("50000000"), false, false),
        new LifePlan("LIFE-STANDARD", "Standard Life", "Life insurance with disability coverage",
            new BigDecimal("160000"), new BigDecimal("150000000"), true, false),
        new LifePlan("LIFE-PREMIUM", "Premium Life", "Full life insurance with critical illness",
            new BigDecimal("280000"), new BigDecimal("300000000"), true, true)
    );

    private LifePlanCatalog() {}

    public static List<LifePlan> getAll() { return PLANS; }

    public static Optional<LifePlan> findById(String id) {
        return PLANS.stream().filter(p -> p.getId().equals(id)).findFirst();
    }
}
