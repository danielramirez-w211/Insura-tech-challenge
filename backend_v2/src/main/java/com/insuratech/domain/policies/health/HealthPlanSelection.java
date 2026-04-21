package com.insuratech.domain.policies.health;

import lombok.Getter;

import java.math.BigDecimal;

@Getter
public class HealthPlanSelection {

    private final HealthPlan plan;
    private final BigDecimal calculatedPremium;
    private final int insuredAge;

    public HealthPlanSelection(HealthPlan plan, int insuredAge) {
        this.plan = plan;
        this.insuredAge = insuredAge;
        this.calculatedPremium = HealthPlanPricingService.calculatePremium(plan, insuredAge);
    }
}
