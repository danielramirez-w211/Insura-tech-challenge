package com.insuratech.domain.exceptions;

public class ClaimAlreadyClosedException extends BusinessRuleException {
    public ClaimAlreadyClosedException() {
        super("ClaimAlreadyClosed", "The claim is already closed and cannot be modified.");
    }
}
