package com.insuratech.domain.policies.vehicle;

import lombok.Getter;

import java.math.BigDecimal;

@Getter
public class VehicleQuotation {

    private final String brand;
    private final String model;
    private final int year;
    private final BigDecimal vehicleValue;
    private final boolean isHighRiskBrand;

    public VehicleQuotation(String brand, String model, int year, BigDecimal vehicleValue) {
        this.brand = brand;
        this.model = model;
        this.year = year;
        this.vehicleValue = vehicleValue;
        this.isHighRiskBrand = HighSinistrabilityBrands.isHighRisk(brand);
    }
}
