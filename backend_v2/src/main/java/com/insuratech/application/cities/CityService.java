package com.insuratech.application.cities;

import com.insuratech.application.cities.dto.CityResponse;
import com.insuratech.domain.ports.CityRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
@RequiredArgsConstructor
public class CityService {

    private final CityRepository cityRepository;

    public List<CityResponse> getAll() {
        return cityRepository.findAll().stream()
            .map(c -> new CityResponse(c.id(), c.name(), c.department(), c.country()))
            .toList();
    }
}
