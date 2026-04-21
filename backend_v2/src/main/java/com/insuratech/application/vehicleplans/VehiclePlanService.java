package com.insuratech.application.vehicleplans;

import com.insuratech.application.vehicleplans.dto.VehiclePlanDto;
import com.insuratech.application.vehicleplans.dto.VehiclePlanOptionDto;
import com.insuratech.application.vehicleplans.dto.VehicleQuotationDto;
import com.insuratech.domain.policies.vehicle.HighSinistrabilityBrands;
import com.insuratech.domain.policies.vehicle.VehiclePlan;
import com.insuratech.domain.policies.vehicle.VehiclePlanCatalog;
import com.insuratech.domain.policies.vehicle.VehiclePricingService;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.Year;
import java.util.ArrayList;
import java.util.List;

@Service
public class VehiclePlanService {

    public List<VehiclePlanDto> getAll() {
        return VehiclePlanCatalog.getAll().stream()
            .map(p -> new VehiclePlanDto(p.getId(), p.getName(), p.getTechnicalRate(),
                buildCoverages(p), buildAssistances(p)))
            .toList();
    }

    public VehicleQuotationDto calculate(BigDecimal commercialValue, int vehicleYear, String brand) {
        int vehicleAge = Year.now().getValue() - vehicleYear;
        String ageCategory = vehicleAge < 3 ? "NEW" : vehicleAge < 8 ? "STANDARD" : "OLD";
        BigDecimal technicalRate = VehiclePlanCatalog.getAll().get(0).getTechnicalRate();
        boolean hasBrandSurcharge = HighSinistrabilityBrands.isHighRisk(brand);
        BigDecimal basePremium = VehiclePricingService.calculatePremium(
            VehiclePlanCatalog.getAll().get(0), commercialValue, brand);

        List<VehiclePlanOptionDto> plans = VehiclePlanCatalog.getAll().stream()
            .map(p -> {
                BigDecimal monthly = VehiclePricingService.calculatePremium(p, commercialValue, brand);
                BigDecimal annual = monthly.multiply(BigDecimal.valueOf(12))
                    .multiply(new BigDecimal("0.90")).setScale(0, RoundingMode.HALF_UP);
                return new VehiclePlanOptionDto(p.getId(), p.getName(), monthly, annual,
                    buildCoverages(p), buildAssistances(p));
            })
            .toList();

        return new VehicleQuotationDto(
            commercialValue, vehicleYear, brand, vehicleAge, ageCategory,
            technicalRate, hasBrandSurcharge, basePremium, plans);
    }

    private List<String> buildCoverages(VehiclePlan p) {
        List<String> list = new ArrayList<>();
        if (p.isIncludesLiability())      list.add("CIVIL_LIABILITY");
        if (p.isIncludesTheft())          list.add("THEFT");
        if (p.isIncludesRoadAssistance()) list.add("ROAD_ASSISTANCE");
        return list;
    }

    private List<String> buildAssistances(VehiclePlan p) {
        List<String> list = new ArrayList<>();
        if (p.isIncludesRoadAssistance()) list.add("TOWING");
        return list;
    }
}
