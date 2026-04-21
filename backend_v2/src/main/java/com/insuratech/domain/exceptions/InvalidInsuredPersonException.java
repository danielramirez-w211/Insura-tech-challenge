package com.insuratech.domain.exceptions;

public class InvalidInsuredPersonException extends BusinessRuleException {
    public InvalidInsuredPersonException(String reason) {
        super("InvalidInsuredPerson", "Invalid insured person data: " + reason);
    }
}
