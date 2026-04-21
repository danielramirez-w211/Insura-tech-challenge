package com.insuratech.domain.exceptions;

public class InvalidPolicyNumberException extends BusinessRuleException {
    public InvalidPolicyNumberException(String value) {
        super("InvalidPolicyNumber", "Invalid policy number format: " + value);
    }
}
