// Origen: PolicyRepository.cs → MongoPolicyRepository.java
package com.insuratech.infrastructure.persistence.repositories;

import com.insuratech.domain.policies.Policy;
import com.insuratech.domain.policies.PolicyStatus;
import com.insuratech.domain.policies.PolicyType;
import com.insuratech.domain.policies.vo.PolicyNumber;
import com.insuratech.domain.ports.PolicyRepository;
import com.insuratech.infrastructure.persistence.springdata.PolicySpringRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.data.mongodb.core.MongoTemplate;
import org.springframework.data.mongodb.core.query.Criteria;
import org.springframework.data.mongodb.core.query.Query;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.Optional;

@Component
@RequiredArgsConstructor
public class MongoPolicyRepository implements PolicyRepository {

    private final PolicySpringRepository springRepo;
    private final MongoTemplate mongoTemplate;

    @Override
    public Policy save(Policy policy) {
        return springRepo.save(policy);
    }

    @Override
    public Optional<Policy> findById(String id) {
        return springRepo.findById(id).filter(p -> !p.isDeleted());
    }

    @Override
    public Optional<Policy> findByPolicyNumber(PolicyNumber policyNumber) {
        var query = Query.query(
            Criteria.where("policyNumber.value").is(policyNumber.getValue())
                    .and("deleted").is(false));
        return Optional.ofNullable(mongoTemplate.findOne(query, Policy.class));
    }

    @Override
    public List<Policy> findByAgentId(String agentId) {
        return springRepo.findByAgentIdAndDeletedFalse(agentId);
    }

    @Override
    public List<Policy> findByStatus(PolicyStatus status) {
        return springRepo.findByStatusAndDeletedFalse(status);
    }

    @Override
    public List<Policy> findByType(PolicyType type) {
        return springRepo.findByPolicyTypeAndDeletedFalse(type);
    }

    @Override
    public List<Policy> findByInsuredPersonDocument(String documentNumber) {
        var query = Query.query(
            Criteria.where("insuredPerson.documentNumber").is(documentNumber)
                    .and("deleted").is(false));
        return mongoTemplate.find(query, Policy.class);
    }

    @Override
    public List<ClientSummaryProjection> findClientSummaries(String agentId) {
        return findByAgentId(agentId).stream()
            .map(p -> new ClientSummaryProjection(
                p.getId(),
                p.getPolicyNumber().getValue(),
                p.getInsuredPerson().getFullName(),
                p.getInsuredPerson().getDocumentNumber(),
                p.getStatus(),
                p.getPolicyType()))
            .toList();
    }

    @Override
    public void deleteById(String id) {
        findById(id).ifPresent(p -> {
            p.setDeleted(true);
            springRepo.save(p);
        });
    }

    @Override
    public long countByStatus(PolicyStatus status) {
        return springRepo.countByStatusAndDeletedFalse(status);
    }
}
