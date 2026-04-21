package com.insuratech.application.homeplans;

import com.insuratech.application.homeplans.dto.HomePlanPackageDto;
import com.insuratech.application.homeplans.dto.HomeQuotationDto;
import com.insuratech.domain.policies.home.*;
import jakarta.validation.constraints.NotNull;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.time.Year;
import java.util.List;

@Service
public class HomePlanService {

    public List<HomePlanPackageDto> getAll() {
        return HomePlanPackageCatalog.getAll().stream()
            .map(p -> new HomePlanPackageDto(p.getId(), p.getName(),
                p.getIncludedCoverages().stream().map(HomeCoverage::name).toList()))
            .toList();
    }

    public HomeQuotationDto calculate(
            BigDecimal propertyValue, int constructionYear, int stratum,
            int occupants, @NotNull HomePropertyType propertyType,
            List<String> selectedCoverages, boolean hasSecuritySystem) {

        HomePlanPackage pkg = resolvePackage(selectedCoverages);
        HomeQuotation quotation = new HomeQuotation(
            propertyType, propertyValue, constructionYear, null, hasSecuritySystem);

        BigDecimal basePremium = propertyValue.multiply(pkg.getBaseRate());
        BigDecimal finalPremium = HomePricingService.calculatePremium(pkg, quotation);

        int propertyAge = Year.now().getValue() - constructionYear;
        List<String> appliedMultipliers = buildMultipliers(propertyType, propertyAge, hasSecuritySystem);

        return new HomeQuotationDto(
            propertyValue, constructionYear, propertyAge, stratum, occupants,
            propertyType.name(), basePremium, selectedCoverages, appliedMultipliers, finalPremium);
    }

    private HomePlanPackage resolvePackage(List<String> selectedCoverages) {
        if (selectedCoverages == null || selectedCoverages.isEmpty())
            return HomePlanPackageCatalog.getAll().get(0);
        return HomePlanPackageCatalog.getAll().stream()
            .filter(p -> selectedCoverages.contains(p.getId()))
            .findFirst()
            .orElse(HomePlanPackageCatalog.getAll().get(0));
    }

    private List<String> buildMultipliers(HomePropertyType type, int age, boolean security) {
        List<String> list = new java.util.ArrayList<>();
        if (type == HomePropertyType.COMMERCIAL) list.add("COMMERCIAL_SURCHARGE_30%");
        if (type == HomePropertyType.RURAL)      list.add("RURAL_SURCHARGE_20%");
        if (age >= 20)                           list.add("AGE_SURCHARGE");
        if (security)                            list.add("SECURITY_DISCOUNT_10%");
        return list;
    }
}
