package com.insuratech.domain.exceptions;

public class InvalidClaimTransitionException extends BusinessRuleException {
    public InvalidClaimTransitionException(String from, String to) {
        super("InvalidClaimTransition",
            String.format("Cannot transition claim from %s to %s.", from, to));
    }
}
