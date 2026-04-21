// Spec: SPEC-011 — planes de salud
package com.insuratech.api.controllers;

import com.insuratech.application.healthplans.HealthPlanService;
import com.insuratech.application.healthplans.dto.HealthPlanCalculationDto;
import com.insuratech.application.healthplans.dto.HealthPlanDto;
import lombok.RequiredArgsConstructor;
import org.springframework.format.annotation.DateTimeFormat;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDate;
import java.util.List;

@RestController
@RequestMapping("/v1/health-plans")
@RequiredArgsConstructor
public class HealthPlansController {

    private final HealthPlanService healthPlanService;

    @GetMapping
    public ResponseEntity<List<HealthPlanDto>> getAll() {
        return ResponseEntity.ok(healthPlanService.getAll());
    }

    @GetMapping("/calculate")
    public ResponseEntity<HealthPlanCalculationDto> calculate(
            @RequestParam String planId,
            @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) LocalDate birthDate) {
        return ResponseEntity.ok(healthPlanService.calculate(planId, birthDate));
    }
}
