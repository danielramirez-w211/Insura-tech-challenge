package com.insuratech.application.lifeplans;

import com.insuratech.application.lifeplans.dto.LifePlanCalculationDto;
import com.insuratech.application.lifeplans.dto.LifePlanDto;
import com.insuratech.domain.policies.life.LifePlan;
import com.insuratech.domain.policies.life.LifePlanCatalog;
import com.insuratech.domain.policies.life.LifePlanPricingService;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.LocalDate;
import java.time.Period;
import java.util.List;

@Service
public class LifePlanService {

    public List<LifePlanDto> getAll() {
        return LifePlanCatalog.getAll().stream()
            .map(this::toDto)
            .toList();
    }

    public LifePlanCalculationDto calculate(String planId, LocalDate birthDate) {
        LifePlan plan = LifePlanCatalog.findById(planId)
            .orElseThrow(() -> new IllegalArgumentException("Life plan not found: " + planId));

        int age = Period.between(birthDate, LocalDate.now()).getYears();
        BigDecimal annualPremium = LifePlanPricingService.calculatePremium(plan, age, false);
        BigDecimal monthlyPremium = annualPremium.divide(BigDecimal.valueOf(12), 0, RoundingMode.HALF_UP);
        BigDecimal coverage = plan.getCoverageAmount();

        return new LifePlanCalculationDto(
            plan.getId(), plan.getName(), age,
            annualPremium, monthlyPremium, 365,
            coverage,
            coverage.multiply(new BigDecimal("0.05")).setScale(0, RoundingMode.HALF_UP),
            coverage.multiply(new BigDecimal("0.03")).setScale(0, RoundingMode.HALF_UP),
            coverage.multiply(new BigDecimal("0.02")).setScale(0, RoundingMode.HALF_UP)
        );
    }

    private LifePlanDto toDto(LifePlan p) {
        BigDecimal monthly = p.getBasePremium().divide(BigDecimal.valueOf(12), 0, RoundingMode.HALF_UP);
        BigDecimal coverage = p.getCoverageAmount();
        return new LifePlanDto(
            p.getId(), p.getName(),
            p.getBasePremium(), monthly, coverage,
            coverage.multiply(new BigDecimal("0.05")).setScale(0, RoundingMode.HALF_UP),
            coverage.multiply(new BigDecimal("0.03")).setScale(0, RoundingMode.HALF_UP),
            coverage.multiply(new BigDecimal("0.02")).setScale(0, RoundingMode.HALF_UP)
        );
    }
}
