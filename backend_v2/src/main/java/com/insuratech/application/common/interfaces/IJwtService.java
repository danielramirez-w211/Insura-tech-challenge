package com.insuratech.application.common.interfaces;

import com.insuratech.domain.users.User;
import java.time.Instant;

public interface IJwtService {
    String generateToken(User user);
    Instant getExpiration();
    String extractUserId(String token);
    String extractEmail(String token);
    boolean isTokenValid(String token);
}
