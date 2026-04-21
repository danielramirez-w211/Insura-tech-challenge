package com.insuratech.domain.policies.life;

import lombok.Getter;

import java.math.BigDecimal;

@Getter
public class LifePlanSelection {

    private final LifePlan plan;
    private final BigDecimal calculatedPremium;
    private final int insuredAge;
    private final boolean smoker;

    public LifePlanSelection(LifePlan plan, int insuredAge, boolean smoker) {
        this.plan = plan;
        this.insuredAge = insuredAge;
        this.smoker = smoker;
        this.calculatedPremium = LifePlanPricingService.calculatePremium(plan, insuredAge, smoker);
    }
}
