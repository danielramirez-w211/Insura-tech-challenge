package com.insuratech.domain.policies.health;

import lombok.Getter;

import java.math.BigDecimal;

@Getter
public class HealthPlan {

    private final String id;
    private final String name;
    private final String description;
    private final BigDecimal basePremium;
    private final BigDecimal coverageLimit;
    private final boolean includesDental;
    private final boolean includesVision;
    private final boolean includesMental;

    public HealthPlan(String id, String name, String description, BigDecimal basePremium,
                      BigDecimal coverageLimit, boolean includesDental,
                      boolean includesVision, boolean includesMental) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.basePremium = basePremium;
        this.coverageLimit = coverageLimit;
        this.includesDental = includesDental;
        this.includesVision = includesVision;
        this.includesMental = includesMental;
    }
}
