package com.insuratech.domain.exceptions;

public class PolicyAlreadyActiveException extends BusinessRuleException {
    public PolicyAlreadyActiveException() {
        super("PolicyAlreadyActive", "The policy is already in active status.");
    }
}
