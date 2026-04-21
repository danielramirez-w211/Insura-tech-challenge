// Spec: SPEC-008 — life-plan
// Origen: CreateLifePolicyStrategy.cs → CreateLifePolicyStrategy.java
package com.insuratech.application.policies.strategies;

import com.insuratech.application.policies.commands.CreatePolicyCommand;
import com.insuratech.domain.policies.Policy;
import com.insuratech.domain.policies.PolicyType;
import com.insuratech.domain.policies.life.LifePlanCatalog;
import com.insuratech.domain.policies.life.LifePlanSelection;
import com.insuratech.domain.policies.vo.CoveragePeriod;
import com.insuratech.domain.policies.vo.InsuredPerson;
import com.insuratech.domain.policies.vo.PolicyNumber;
import org.springframework.stereotype.Component;

@Component
public class CreateLifePolicyStrategy implements PolicyCreationStrategy {

    @Override
    public PolicyType type() { return PolicyType.LIFE; }

    @Override
    public boolean canHandle(CreatePolicyCommand cmd) {
        return cmd.lifePlanId() != null && !cmd.lifePlanId().isBlank();
    }

    @Override
    public Policy create(CreatePolicyCommand cmd, PolicyNumber policyNumber, InsuredPerson insured) {
        var plan = LifePlanCatalog.findById(cmd.lifePlanId())
            .orElseThrow(() -> new IllegalArgumentException("Life plan not found: " + cmd.lifePlanId()));
        boolean smoker = Boolean.TRUE.equals(cmd.smoker());
        var selection = new LifePlanSelection(plan, insured.getAge(), smoker);
        var period = CoveragePeriod.of(cmd.coverageStartDate(), cmd.coverageEndDate());
        return Policy.createLifePolicy(policyNumber, insured, period, selection, cmd.agentId(), cmd.cityId());
    }
}
