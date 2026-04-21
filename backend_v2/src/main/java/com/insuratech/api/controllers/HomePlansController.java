// Spec: SPEC-016 — planes de hogar
package com.insuratech.api.controllers;

import com.insuratech.application.homeplans.HomePlanService;
import com.insuratech.application.homeplans.dto.HomePlanPackageDto;
import com.insuratech.application.homeplans.dto.HomeQuotationDto;
import com.insuratech.domain.policies.home.HomePropertyType;
import jakarta.validation.Valid;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Positive;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.math.BigDecimal;
import java.util.List;

@RestController
@RequestMapping("/v1/home-plans")
@RequiredArgsConstructor
public class HomePlansController {

    private final HomePlanService homePlanService;

    @GetMapping
    public ResponseEntity<List<HomePlanPackageDto>> getAll() {
        return ResponseEntity.ok(homePlanService.getAll());
    }

    @PostMapping("/calculate")
    public ResponseEntity<HomeQuotationDto> calculate(@Valid @RequestBody CalculateHomeRequest body) {
        return ResponseEntity.ok(homePlanService.calculate(
            body.propertyValue(), body.constructionYear(), body.stratum(),
            body.occupants(), body.propertyType(), body.selectedCoverages(),
            body.hasSecuritySystem() != null && body.hasSecuritySystem()));
    }

    record CalculateHomeRequest(
        @NotNull @Positive BigDecimal propertyValue,
        int constructionYear,
        int stratum,
        int occupants,
        @NotNull HomePropertyType propertyType,
        List<String> selectedCoverages,
        Boolean hasSecuritySystem
    ) {}
}
