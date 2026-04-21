package com.insuratech.domain.exceptions;

public class PolicyNotFoundException extends DomainException {
    public PolicyNotFoundException(String policyId) {
        super("Policy not found with id: " + policyId);
    }
}
