package com.insuratech.domain.policies.home;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.Year;

public final class HomePricingService {

    private HomePricingService() {}

    public static BigDecimal calculatePremium(HomePlanPackage pkg, HomeQuotation quotation) {
        BigDecimal base = quotation.getPropertyValue().multiply(pkg.getBaseRate());

        // Step 1: property type surcharge
        base = base.multiply(getPropertyTypeFactor(quotation.getPropertyType()));

        // Step 2: age of construction surcharge
        int age = Year.now().getValue() - quotation.getConstructionYear();
        base = base.multiply(getAgeFactor(age));

        // Step 3: security system discount
        if (quotation.isHasSecuritySystem()) {
            base = base.multiply(new BigDecimal("0.90"));
        }

        return base.setScale(0, RoundingMode.HALF_UP);
    }

    private static BigDecimal getPropertyTypeFactor(HomePropertyType type) {
        return switch (type) {
            case APARTMENT  -> new BigDecimal("0.90");
            case HOUSE      -> BigDecimal.ONE;
            case COMMERCIAL -> new BigDecimal("1.30");
            case RURAL      -> new BigDecimal("1.20");
        };
    }

    private static BigDecimal getAgeFactor(int ageYears) {
        if (ageYears < 10) return BigDecimal.ONE;
        if (ageYears < 20) return new BigDecimal("1.05");
        if (ageYears < 30) return new BigDecimal("1.10");
        return new BigDecimal("1.20");
    }
}
