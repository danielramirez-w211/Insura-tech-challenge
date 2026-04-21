package com.insuratech.domain.exceptions;

public class ClaimNotFoundException extends DomainException {
    public ClaimNotFoundException(String claimId) {
        super("Claim not found with id: " + claimId);
    }
}
