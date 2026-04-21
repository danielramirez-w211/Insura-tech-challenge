// Spec: SPEC-010 — planes vehiculares
package com.insuratech.api.controllers;

import com.insuratech.application.vehicleplans.VehiclePlanService;
import com.insuratech.application.vehicleplans.dto.VehiclePlanDto;
import com.insuratech.application.vehicleplans.dto.VehicleQuotationDto;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.math.BigDecimal;
import java.util.List;

@RestController
@RequestMapping("/v1/vehicle-plans")
@RequiredArgsConstructor
public class VehiclePlansController {

    private final VehiclePlanService vehiclePlanService;

    @GetMapping
    public ResponseEntity<List<VehiclePlanDto>> getAll() {
        return ResponseEntity.ok(vehiclePlanService.getAll());
    }

    @GetMapping("/calculate")
    public ResponseEntity<VehicleQuotationDto> calculate(
            @RequestParam BigDecimal commercialValue,
            @RequestParam int vehicleYear,
            @RequestParam String brand) {
        return ResponseEntity.ok(vehiclePlanService.calculate(commercialValue, vehicleYear, brand));
    }
}
