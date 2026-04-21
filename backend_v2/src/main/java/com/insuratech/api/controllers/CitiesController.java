// Spec: SPEC-010..016 — ciudades disponibles para asegurados
package com.insuratech.api.controllers;

import com.insuratech.application.cities.CityService;
import com.insuratech.application.cities.dto.CityResponse;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;

@RestController
@RequestMapping("/v1/cities")
@RequiredArgsConstructor
public class CitiesController {

    private final CityService cityService;

    @GetMapping
    public ResponseEntity<List<CityResponse>> getAll() {
        return ResponseEntity.ok(cityService.getAll());
    }
}
