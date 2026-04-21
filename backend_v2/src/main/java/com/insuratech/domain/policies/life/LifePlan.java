package com.insuratech.domain.policies.life;

import lombok.Getter;

import java.math.BigDecimal;

@Getter
public class LifePlan {

    private final String id;
    private final String name;
    private final String description;
    private final BigDecimal basePremium;
    private final BigDecimal coverageAmount;
    private final boolean includesDisability;
    private final boolean includesCriticalIllness;

    public LifePlan(String id, String name, String description, BigDecimal basePremium,
                    BigDecimal coverageAmount, boolean includesDisability, boolean includesCriticalIllness) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.basePremium = basePremium;
        this.coverageAmount = coverageAmount;
        this.includesDisability = includesDisability;
        this.includesCriticalIllness = includesCriticalIllness;
    }
}
