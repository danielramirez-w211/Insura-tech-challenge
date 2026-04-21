// Spec: SPEC-014 — travel-plan
// Origen: CreateTravelPolicyStrategy.cs → CreateTravelPolicyStrategy.java
package com.insuratech.application.policies.strategies;

import com.insuratech.application.policies.commands.CreatePolicyCommand;
import com.insuratech.domain.policies.Policy;
import com.insuratech.domain.policies.PolicyType;
import com.insuratech.domain.policies.travel.TravelPlanSelection;
import com.insuratech.domain.policies.vo.CoveragePeriod;
import com.insuratech.domain.policies.vo.InsuredPerson;
import com.insuratech.domain.policies.vo.PolicyNumber;
import org.springframework.stereotype.Component;

@Component
public class CreateTravelPolicyStrategy implements PolicyCreationStrategy {

    @Override
    public PolicyType type() { return PolicyType.TRAVEL; }

    @Override
    public boolean canHandle(CreatePolicyCommand cmd) {
        return cmd.tripType() != null;
    }

    @Override
    public Policy create(CreatePolicyCommand cmd, PolicyNumber policyNumber, InsuredPerson insured) {
        int travelers = cmd.numberOfTravelers() != null ? cmd.numberOfTravelers() : 1;
        var selection = new TravelPlanSelection(
            cmd.tripType(), cmd.travelDestinations(),
            cmd.coverageStartDate(), cmd.coverageEndDate(), travelers);
        var period = CoveragePeriod.of(cmd.coverageStartDate(), cmd.coverageEndDate());
        return Policy.createTravelPolicy(policyNumber, insured, period, selection, cmd.agentId(), cmd.cityId());
    }
}
