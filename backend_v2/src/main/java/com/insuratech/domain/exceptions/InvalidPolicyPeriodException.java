package com.insuratech.domain.exceptions;

public class InvalidPolicyPeriodException extends BusinessRuleException {
    public InvalidPolicyPeriodException() {
        super("InvalidPolicyPeriod", "Policy end date must be after start date.");
    }
}
