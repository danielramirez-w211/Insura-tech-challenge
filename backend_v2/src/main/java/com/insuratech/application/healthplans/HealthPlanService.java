package com.insuratech.application.healthplans;

import com.insuratech.application.healthplans.dto.HealthPlanCalculationDto;
import com.insuratech.application.healthplans.dto.HealthPlanDto;
import com.insuratech.domain.policies.health.HealthPlan;
import com.insuratech.domain.policies.health.HealthPlanCatalog;
import com.insuratech.domain.policies.health.HealthPlanPricingService;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.LocalDate;
import java.time.Period;
import java.util.List;

@Service
public class HealthPlanService {

    public List<HealthPlanDto> getAll() {
        return HealthPlanCatalog.getAll().stream()
            .map(p -> new HealthPlanDto(p.getId(), p.getName(), p.getBasePremium()))
            .toList();
    }

    public HealthPlanCalculationDto calculate(String planId, LocalDate birthDate) {
        HealthPlan plan = HealthPlanCatalog.findById(planId)
            .orElseThrow(() -> new IllegalArgumentException("Health plan not found: " + planId));

        int age = Period.between(birthDate, LocalDate.now()).getYears();
        BigDecimal finalAmount = HealthPlanPricingService.calculatePremium(plan, age);
        BigDecimal ageFactor = finalAmount.subtract(plan.getBasePremium());
        int ageFactorPct = ageFactor.compareTo(BigDecimal.ZERO) == 0 ? 0
            : ageFactor.multiply(BigDecimal.valueOf(100))
                .divide(plan.getBasePremium(), 0, RoundingMode.HALF_UP).intValue();
        BigDecimal monthlyPremium = finalAmount.divide(BigDecimal.valueOf(12), 0, RoundingMode.HALF_UP);

        return new HealthPlanCalculationDto(
            plan.getId(), plan.getName(), plan.getBasePremium(),
            ageFactorPct, ageFactor, finalAmount, age, monthlyPremium, 365);
    }
}
