// Spec: SPEC-015 — planes de viaje
package com.insuratech.api.controllers;

import com.insuratech.application.travelplans.TravelPlanService;
import com.insuratech.application.travelplans.dto.TravelPlanCalculationDto;
import com.insuratech.domain.policies.travel.Continent;
import com.insuratech.domain.policies.travel.TripType;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/v1/travel-plans")
@RequiredArgsConstructor
public class TravelPlansController {

    private final TravelPlanService travelPlanService;

    @GetMapping("/calculate")
    public ResponseEntity<TravelPlanCalculationDto> calculate(
            @RequestParam TripType tripType,
            @RequestParam(required = false) Continent continent,
            @RequestParam int durationDays) {
        return ResponseEntity.ok(travelPlanService.calculate(tripType, continent, durationDays));
    }
}
