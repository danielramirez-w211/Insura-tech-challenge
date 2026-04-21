package com.insuratech.infrastructure.persistence.springdata;

import com.insuratech.domain.policies.Policy;
import com.insuratech.domain.policies.PolicyStatus;
import com.insuratech.domain.policies.PolicyType;
import org.springframework.data.mongodb.repository.MongoRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface PolicySpringRepository extends MongoRepository<Policy, String> {
    List<Policy> findByStatusAndDeletedFalse(PolicyStatus status);
    List<Policy> findByPolicyTypeAndDeletedFalse(PolicyType type);
    List<Policy> findByAgentIdAndDeletedFalse(String agentId);
    long countByStatusAndDeletedFalse(PolicyStatus status);
}
