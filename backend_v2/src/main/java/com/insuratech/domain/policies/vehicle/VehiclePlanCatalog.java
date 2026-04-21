package com.insuratech.domain.policies.vehicle;

import java.math.BigDecimal;
import java.util.List;
import java.util.Optional;

public final class VehiclePlanCatalog {

    private static final List<VehiclePlan> PLANS = List.of(
        new VehiclePlan("VEHICLE-BASIC", "Basic Vehicle", "Third party liability only",
            new BigDecimal("0.02"), false, false, true),
        new VehiclePlan("VEHICLE-STANDARD", "Standard Vehicle", "Comprehensive without theft",
            new BigDecimal("0.035"), false, true, true),
        new VehiclePlan("VEHICLE-PREMIUM", "Premium Vehicle", "Full comprehensive coverage",
            new BigDecimal("0.05"), true, true, true)
    );

    private VehiclePlanCatalog() {}

    public static List<VehiclePlan> getAll() { return PLANS; }

    public static Optional<VehiclePlan> findById(String id) {
        return PLANS.stream().filter(p -> p.getId().equals(id)).findFirst();
    }
}
