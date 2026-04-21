package com.insuratech.domain.exceptions;

public class UserNotFoundException extends DomainException {
    public UserNotFoundException(String userId) {
        super("User not found with id: " + userId);
    }
}
