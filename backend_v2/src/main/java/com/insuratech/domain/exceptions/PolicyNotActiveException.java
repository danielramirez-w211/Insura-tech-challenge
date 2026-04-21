package com.insuratech.domain.exceptions;

public class PolicyNotActiveException extends BusinessRuleException {
    public PolicyNotActiveException() {
        super("PolicyNotActive", "Operation requires the policy to be in active status.");
    }
}
