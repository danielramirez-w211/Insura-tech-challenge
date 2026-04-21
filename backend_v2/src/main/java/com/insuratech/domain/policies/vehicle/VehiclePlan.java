package com.insuratech.domain.policies.vehicle;

import lombok.Getter;

import java.math.BigDecimal;

@Getter
public class VehiclePlan {

    private final String id;
    private final String name;
    private final String description;
    private final BigDecimal technicalRate;
    private final boolean includesTheft;
    private final boolean includesRoadAssistance;
    private final boolean includesLiability;

    public VehiclePlan(String id, String name, String description, BigDecimal technicalRate,
                       boolean includesTheft, boolean includesRoadAssistance, boolean includesLiability) {
        this.id = id;
        this.name = name;
        this.description = description;
        this.technicalRate = technicalRate;
        this.includesTheft = includesTheft;
        this.includesRoadAssistance = includesRoadAssistance;
        this.includesLiability = includesLiability;
    }
}
