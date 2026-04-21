package com.insuratech.application.users.dto;
import java.time.Instant;
public record UserResponse(
    String id, String email, String role,
    String firstName, String lastName, String phone, String city,
    boolean active, Instant createdAt
) {}
