package com.insuratech.application.common.exceptions;

public class NotFoundException extends RuntimeException {
    public NotFoundException(String message) { super(message); }
    public NotFoundException(String entity, String id) {
        super(entity + " '" + id + "' was not found.");
    }
}
