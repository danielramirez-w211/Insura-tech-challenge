package com.insuratech.application.policies.strategies;

import com.insuratech.application.policies.commands.CreatePolicyCommand;
import com.insuratech.domain.policies.Policy;
import com.insuratech.domain.policies.PolicyType;
import com.insuratech.domain.policies.vo.InsuredPerson;
import com.insuratech.domain.policies.vo.PolicyNumber;

public interface PolicyCreationStrategy {
    PolicyType type();
    boolean canHandle(CreatePolicyCommand command);
    Policy create(CreatePolicyCommand command, PolicyNumber policyNumber, InsuredPerson insured);
}
