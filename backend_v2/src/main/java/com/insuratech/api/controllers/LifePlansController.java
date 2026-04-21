// Spec: SPEC-012 — planes de vida
package com.insuratech.api.controllers;

import com.insuratech.application.lifeplans.LifePlanService;
import com.insuratech.application.lifeplans.dto.LifePlanCalculationDto;
import com.insuratech.application.lifeplans.dto.LifePlanDto;
import lombok.RequiredArgsConstructor;
import org.springframework.format.annotation.DateTimeFormat;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.time.LocalDate;
import java.util.List;

@RestController
@RequestMapping("/v1/life-plans")
@RequiredArgsConstructor
public class LifePlansController {

    private final LifePlanService lifePlanService;

    @GetMapping
    public ResponseEntity<List<LifePlanDto>> getAll() {
        return ResponseEntity.ok(lifePlanService.getAll());
    }

    @GetMapping("/calculate")
    public ResponseEntity<LifePlanCalculationDto> calculate(
            @RequestParam String planId,
            @RequestParam @DateTimeFormat(iso = DateTimeFormat.ISO.DATE) LocalDate birthDate) {
        return ResponseEntity.ok(lifePlanService.calculate(planId, birthDate));
    }
}
