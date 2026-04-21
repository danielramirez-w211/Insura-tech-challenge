package com.insuratech.domain.ports;

import java.util.List;
import java.util.Optional;

public interface CityRepository {

    Optional<CityInfo> findById(String id);

    List<CityInfo> findAll();

    List<CityInfo> findByDepartment(String department);

    record CityInfo(String id, String name, String department, String country) {}
}
