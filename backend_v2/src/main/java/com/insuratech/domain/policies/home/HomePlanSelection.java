package com.insuratech.domain.policies.home;

import lombok.Getter;

import java.math.BigDecimal;

@Getter
public class HomePlanSelection {

    private final HomePlanPackage planPackage;
    private final HomeQuotation quotation;
    private final BigDecimal calculatedPremium;

    public HomePlanSelection(HomePlanPackage planPackage, HomeQuotation quotation) {
        this.planPackage = planPackage;
        this.quotation = quotation;
        this.calculatedPremium = HomePricingService.calculatePremium(planPackage, quotation);
    }
}
