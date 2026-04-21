// Spec: SPEC-010 — vehicle-quotation
// Origen: CreateVehiclePolicyStrategy.cs → CreateVehiclePolicyStrategy.java
package com.insuratech.application.policies.strategies;

import com.insuratech.application.policies.commands.CreatePolicyCommand;
import com.insuratech.domain.policies.Policy;
import com.insuratech.domain.policies.PolicyType;
import com.insuratech.domain.policies.vehicle.VehiclePlanCatalog;
import com.insuratech.domain.policies.vehicle.VehiclePlanSelection;
import com.insuratech.domain.policies.vehicle.VehicleQuotation;
import com.insuratech.domain.policies.vo.CoveragePeriod;
import com.insuratech.domain.policies.vo.InsuredPerson;
import com.insuratech.domain.policies.vo.PolicyNumber;
import org.springframework.stereotype.Component;

@Component
public class CreateVehiclePolicyStrategy implements PolicyCreationStrategy {

    @Override
    public PolicyType type() { return PolicyType.VEHICLE; }

    @Override
    public boolean canHandle(CreatePolicyCommand cmd) {
        return cmd.vehiclePlanId() != null && !cmd.vehiclePlanId().isBlank();
    }

    @Override
    public Policy create(CreatePolicyCommand cmd, PolicyNumber policyNumber, InsuredPerson insured) {
        var plan = VehiclePlanCatalog.findById(cmd.vehiclePlanId())
            .orElseThrow(() -> new IllegalArgumentException("Vehicle plan not found: " + cmd.vehiclePlanId()));
        var quotation = new VehicleQuotation(
            cmd.vehicleBrand(), cmd.vehicleModel(), cmd.vehicleYear(), cmd.vehicleCommercialValue());
        var selection = new VehiclePlanSelection(plan, quotation);
        var period = CoveragePeriod.of(cmd.coverageStartDate(), cmd.coverageEndDate());
        return Policy.createVehiclePolicy(policyNumber, insured, period, selection, cmd.agentId(), cmd.cityId());
    }
}
