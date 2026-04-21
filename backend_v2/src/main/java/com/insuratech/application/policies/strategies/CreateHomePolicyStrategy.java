// Spec: SPEC-012 — home-plan
// Origen: CreateHomePolicyStrategy.cs → CreateHomePolicyStrategy.java
package com.insuratech.application.policies.strategies;

import com.insuratech.application.policies.commands.CreatePolicyCommand;
import com.insuratech.domain.policies.Policy;
import com.insuratech.domain.policies.PolicyType;
import com.insuratech.domain.policies.home.HomePlanPackageCatalog;
import com.insuratech.domain.policies.home.HomePlanSelection;
import com.insuratech.domain.policies.home.HomePropertyType;
import com.insuratech.domain.policies.home.HomeQuotation;
import com.insuratech.domain.policies.vo.CoveragePeriod;
import com.insuratech.domain.policies.vo.InsuredPerson;
import com.insuratech.domain.policies.vo.PolicyNumber;
import org.springframework.stereotype.Component;

@Component
public class CreateHomePolicyStrategy implements PolicyCreationStrategy {

    @Override
    public PolicyType type() { return PolicyType.HOME; }

    @Override
    public boolean canHandle(CreatePolicyCommand cmd) {
        return cmd.homePlanPackageId() != null && !cmd.homePlanPackageId().isBlank();
    }

    @Override
    public Policy create(CreatePolicyCommand cmd, PolicyNumber policyNumber, InsuredPerson insured) {
        var pkg = HomePlanPackageCatalog.findById(cmd.homePlanPackageId())
            .orElseThrow(() -> new IllegalArgumentException("Home plan not found: " + cmd.homePlanPackageId()));
        var propertyType = HomePropertyType.valueOf(cmd.homePropertyType());
        var quotation = new HomeQuotation(
            propertyType, cmd.homePropertyValue(),
            cmd.homeConstructionYear(), cmd.cityId(),
            Boolean.TRUE.equals(cmd.homeHasSecuritySystem()));
        var selection = new HomePlanSelection(pkg, quotation);
        var period = CoveragePeriod.of(cmd.coverageStartDate(), cmd.coverageEndDate());
        return Policy.createHomePolicy(policyNumber, insured, period, selection, cmd.agentId(), cmd.cityId());
    }
}
