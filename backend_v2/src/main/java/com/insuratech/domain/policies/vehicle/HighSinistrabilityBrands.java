package com.insuratech.domain.policies.vehicle;

import java.util.Set;

public final class HighSinistrabilityBrands {

    private static final Set<String> HIGH_RISK_BRANDS = Set.of(
        "FERRARI", "LAMBORGHINI", "PORSCHE", "MASERATI", "BENTLEY"
    );

    private HighSinistrabilityBrands() {}

    public static boolean isHighRisk(String brand) {
        return brand != null && HIGH_RISK_BRANDS.contains(brand.toUpperCase());
    }
}
