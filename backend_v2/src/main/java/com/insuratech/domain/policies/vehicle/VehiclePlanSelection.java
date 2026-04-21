package com.insuratech.domain.policies.vehicle;

import lombok.Getter;

import java.math.BigDecimal;

@Getter
public class VehiclePlanSelection {

    private final VehiclePlan plan;
    private final VehicleQuotation quotation;
    private final BigDecimal calculatedPremium;

    public VehiclePlanSelection(VehiclePlan plan, VehicleQuotation quotation) {
        this.plan = plan;
        this.quotation = quotation;
        this.calculatedPremium = VehiclePricingService.calculatePremium(
            plan, quotation.getVehicleValue(), quotation.getBrand());
    }
}
