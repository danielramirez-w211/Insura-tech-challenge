package com.insuratech.domain.policies.home;

import lombok.Getter;

import java.math.BigDecimal;

@Getter
public class HomeQuotation {

    private final HomePropertyType propertyType;
    private final BigDecimal propertyValue;
    private final int constructionYear;
    private final String city;
    private final boolean hasSecuritySystem;

    public HomeQuotation(HomePropertyType propertyType, BigDecimal propertyValue,
                         int constructionYear, String city, boolean hasSecuritySystem) {
        this.propertyType = propertyType;
        this.propertyValue = propertyValue;
        this.constructionYear = constructionYear;
        this.city = city;
        this.hasSecuritySystem = hasSecuritySystem;
    }
}
