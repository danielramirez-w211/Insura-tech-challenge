package com.insuratech.domain.ports;

import com.insuratech.domain.policies.Policy;
import com.insuratech.domain.policies.PolicyStatus;
import com.insuratech.domain.policies.PolicyType;
import com.insuratech.domain.policies.vo.PolicyNumber;

import java.util.List;
import java.util.Optional;

public interface PolicyRepository {

    Policy save(Policy policy);

    Optional<Policy> findById(String id);

    Optional<Policy> findByPolicyNumber(PolicyNumber policyNumber);

    List<Policy> findByAgentId(String agentId);

    List<Policy> findByStatus(PolicyStatus status);

    List<Policy> findByType(PolicyType type);

    List<Policy> findByInsuredPersonDocument(String documentNumber);

    List<ClientSummaryProjection> findClientSummaries(String agentId);

    void deleteById(String id);

    long countByStatus(PolicyStatus status);

    record ClientSummaryProjection(
        String policyId,
        String policyNumber,
        String insuredFullName,
        String insuredDocument,
        PolicyStatus status,
        PolicyType type
    ) {}
}
