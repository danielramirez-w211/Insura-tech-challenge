package com.insuratech.domain.policies.vehicle;

import java.math.BigDecimal;
import java.math.RoundingMode;

public final class VehiclePricingService {

    private VehiclePricingService() {}

    public static BigDecimal calculatePremium(VehiclePlan plan, BigDecimal vehicleValue, String brand) {
        BigDecimal base = vehicleValue.multiply(plan.getTechnicalRate());
        BigDecimal brandSurcharge = HighSinistrabilityBrands.isHighRisk(brand)
            ? base.multiply(new BigDecimal("0.15"))
            : BigDecimal.ZERO;
        return base.add(brandSurcharge).setScale(0, RoundingMode.HALF_UP);
    }
}
