// Spec: SPEC-001..016 — Gestión de pólizas
// Origen: CreatePolicyHandler, ActivatePolicyHandler, CancelPolicyHandler, etc. → PolicyService.java
package com.insuratech.application.policies;

import com.insuratech.application.common.exceptions.NotFoundException;
import com.insuratech.application.policies.commands.*;
import com.insuratech.application.policies.dto.ClientSummaryResponse;
import com.insuratech.application.policies.dto.PolicyMapper;
import com.insuratech.application.policies.dto.PolicyResponse;
import com.insuratech.application.policies.strategies.PolicyCreationStrategy;
import com.insuratech.domain.policies.Policy;
import com.insuratech.domain.policies.vo.CoveragePeriod;
import com.insuratech.domain.policies.vo.InsuredPerson;
import com.insuratech.domain.policies.vo.PolicyNumber;
import com.insuratech.domain.ports.PolicyRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDate;
import java.util.List;

@Service
@RequiredArgsConstructor
public class PolicyService {

    private final PolicyRepository policyRepository;
    private final List<PolicyCreationStrategy> strategies;

    @Transactional
    public PolicyResponse create(CreatePolicyCommand cmd) {
        var policyNumber = PolicyNumber.generate(LocalDate.now().getYear(),
            policyRepository.countByStatus(com.insuratech.domain.policies.PolicyStatus.DRAFT) + 1);

        var insured = InsuredPerson.of(
            cmd.insuredFirstName(), cmd.insuredLastName(),
            cmd.insuredDocumentType(), cmd.insuredDocumentId(),
            cmd.insuredBirthDate(), cmd.insuredEmail(), cmd.insuredPhone());

        Policy policy = strategies.stream()
            .filter(s -> s.canHandle(cmd))
            .findFirst()
            .map(s -> s.create(cmd, policyNumber, insured))
            .orElseGet(() -> {
                var period = CoveragePeriod.of(cmd.coverageStartDate(), cmd.coverageEndDate());
                return Policy.create(policyNumber, cmd.type(), insured, period,
                    cmd.monthlyPremium(), cmd.insuredAmount(), cmd.agentId(), cmd.cityId());
            });

        policyRepository.save(policy);
        return PolicyMapper.toResponse(policy);
    }

    @Transactional
    public PolicyResponse activate(ActivatePolicyCommand cmd) {
        var policy = findOrThrow(cmd.policyId());
        policy.activate(cmd.activatedBy());
        policyRepository.save(policy);
        return PolicyMapper.toResponse(policy);
    }

    @Transactional
    public PolicyResponse suspend(SuspendPolicyCommand cmd) {
        var policy = findOrThrow(cmd.policyId());
        policy.suspend(cmd.suspendedBy(), cmd.reason());
        policyRepository.save(policy);
        return PolicyMapper.toResponse(policy);
    }

    @Transactional
    public PolicyResponse cancel(CancelPolicyCommand cmd) {
        var policy = findOrThrow(cmd.policyId());
        policy.cancel(cmd.cancelledBy(), cmd.reason());
        policyRepository.save(policy);
        return PolicyMapper.toResponse(policy);
    }

    @Transactional
    public PolicyResponse renew(RenewPolicyCommand cmd) {
        var policy = findOrThrow(cmd.policyId());
        policy.renew(cmd.newEndDate(), cmd.renewedBy());
        policyRepository.save(policy);
        return PolicyMapper.toResponse(policy);
    }

    public PolicyResponse getById(String id) {
        return PolicyMapper.toResponse(findOrThrow(id));
    }

    public List<PolicyResponse> getAll() {
        return policyRepository.findByStatus(com.insuratech.domain.policies.PolicyStatus.ACTIVE)
            .stream().map(PolicyMapper::toResponse).toList();
    }

    public List<ClientSummaryResponse> getClientSummaries(String agentId) {
        return policyRepository.findClientSummaries(agentId)
            .stream().map(PolicyMapper::toSummary).toList();
    }

    private Policy findOrThrow(String id) {
        return policyRepository.findById(id)
            .orElseThrow(() -> new NotFoundException("Policy", id));
    }
}
