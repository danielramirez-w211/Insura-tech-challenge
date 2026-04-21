package com.insuratech.application.auth.dto;

import java.time.Instant;

public record LoginResponse(
    String token,
    String role,
    String userId,
    String email,
    String firstName,
    String lastName,
    Instant expiresAt
) {}
