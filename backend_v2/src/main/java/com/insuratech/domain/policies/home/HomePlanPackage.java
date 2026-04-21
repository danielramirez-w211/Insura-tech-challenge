package com.insuratech.domain.policies.home;

import lombok.Getter;

import java.math.BigDecimal;
import java.util.List;

@Getter
public class HomePlanPackage {

    private final String id;
    private final String name;
    private final String description;
    private final BigDecimal baseRate;
    private final List<HomeCoverage> includedCoverages;

    public HomePlanPackage(String id, String name, String description,
                           BigDecimal baseRate, List<HomeCoverage> includedCoverages) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.baseRate = baseRate;
        this.includedCoverages = List.copyOf(includedCoverages);
    }
}
