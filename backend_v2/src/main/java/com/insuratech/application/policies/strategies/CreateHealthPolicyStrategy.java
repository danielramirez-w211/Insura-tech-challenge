// Spec: SPEC-007 — health-plan
// Origen: CreateHealthPolicyStrategy.cs → CreateHealthPolicyStrategy.java
package com.insuratech.application.policies.strategies;

import com.insuratech.application.policies.commands.CreatePolicyCommand;
import com.insuratech.domain.policies.Policy;
import com.insuratech.domain.policies.PolicyType;
import com.insuratech.domain.policies.health.HealthPlanCatalog;
import com.insuratech.domain.policies.health.HealthPlanSelection;
import com.insuratech.domain.policies.vo.CoveragePeriod;
import com.insuratech.domain.policies.vo.InsuredPerson;
import com.insuratech.domain.policies.vo.PolicyNumber;
import org.springframework.stereotype.Component;

@Component
public class CreateHealthPolicyStrategy implements PolicyCreationStrategy {

    @Override
    public PolicyType type() { return PolicyType.HEALTH; }

    @Override
    public boolean canHandle(CreatePolicyCommand cmd) {
        return cmd.healthPlanId() != null && !cmd.healthPlanId().isBlank();
    }

    @Override
    public Policy create(CreatePolicyCommand cmd, PolicyNumber policyNumber, InsuredPerson insured) {
        var plan = HealthPlanCatalog.findById(cmd.healthPlanId())
            .orElseThrow(() -> new IllegalArgumentException("Health plan not found: " + cmd.healthPlanId()));
        var selection = new HealthPlanSelection(plan, insured.getAge());
        var period = CoveragePeriod.of(cmd.coverageStartDate(), cmd.coverageEndDate());
        return Policy.createHealthPolicy(policyNumber, insured, period, selection, cmd.agentId(), cmd.cityId());
    }
}
