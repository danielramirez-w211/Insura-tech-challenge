// Origen: CityRepository.cs → MongoCityRepository.java
package com.insuratech.infrastructure.persistence.repositories;

import com.insuratech.domain.ports.CityRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.MongoTemplate;
import org.springframework.data.mongodb.core.mapping.Document;
import org.springframework.data.mongodb.core.query.Criteria;
import org.springframework.data.mongodb.core.query.Query;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.Optional;

@Component
@RequiredArgsConstructor
public class MongoCityRepository implements CityRepository {

    private final MongoTemplate mongoTemplate;

    @Override
    public Optional<CityInfo> findById(String id) {
        var doc = mongoTemplate.findById(id, CityDocument.class);
        return Optional.ofNullable(doc).map(d -> new CityInfo(d.id(), d.name(), d.department(), d.country()));
    }

    @Override
    public List<CityInfo> findAll() {
        return mongoTemplate.findAll(CityDocument.class).stream()
            .map(d -> new CityInfo(d.id(), d.name(), d.department(), d.country()))
            .toList();
    }

    @Override
    public List<CityInfo> findByDepartment(String department) {
        var query = Query.query(Criteria.where("department").is(department));
        return mongoTemplate.find(query, CityDocument.class).stream()
            .map(d -> new CityInfo(d.id(), d.name(), d.department(), d.country()))
            .toList();
    }

    @Document(collection = "cities")
    record CityDocument(@Id String id, String name, String department, String country) {}
}
