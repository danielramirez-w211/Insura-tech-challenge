package com.insuratech.domain.policies.home;

import java.math.BigDecimal;
import java.util.List;
import java.util.Optional;

public final class HomePlanPackageCatalog {

    private static final List<HomePlanPackage> PACKAGES = List.of(
        new HomePlanPackage("HOME-BASIC", "Basic Home",
            "Essential fire and theft protection",
            new BigDecimal("0.003"),
            List.of(HomeCoverage.FIRE, HomeCoverage.THEFT)),
        new HomePlanPackage("HOME-STANDARD", "Standard Home",
            "Standard home protection",
            new BigDecimal("0.005"),
            List.of(HomeCoverage.FIRE, HomeCoverage.FLOOD, HomeCoverage.THEFT,
                    HomeCoverage.VANDALISM, HomeCoverage.LIGHTNING)),
        new HomePlanPackage("HOME-PREMIUM", "Premium Home",
            "Comprehensive home coverage",
            new BigDecimal("0.008"),
            List.of(HomeCoverage.FIRE, HomeCoverage.FLOOD, HomeCoverage.EARTHQUAKE,
                    HomeCoverage.THEFT, HomeCoverage.VANDALISM, HomeCoverage.EXPLOSION,
                    HomeCoverage.LIGHTNING, HomeCoverage.WINDSTORM, HomeCoverage.CIVIL_LIABILITY,
                    HomeCoverage.GLASS_BREAKAGE, HomeCoverage.ELECTRICAL_DAMAGE))
    );

    private HomePlanPackageCatalog() {}

    public static List<HomePlanPackage> getAll() { return PACKAGES; }

    public static Optional<HomePlanPackage> findById(String id) {
        return PACKAGES.stream().filter(p -> p.getId().equals(id)).findFirst();
    }
}
