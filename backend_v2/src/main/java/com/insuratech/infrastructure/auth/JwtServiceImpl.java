// Origen: JwtService.cs → JwtServiceImpl.java
package com.insuratech.infrastructure.auth;

import com.insuratech.application.common.interfaces.IJwtService;
import com.insuratech.domain.users.User;
import io.jsonwebtoken.Claims;
import io.jsonwebtoken.Jwts;
import io.jsonwebtoken.security.Keys;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import javax.crypto.SecretKey;
import java.nio.charset.StandardCharsets;
import java.time.Instant;
import java.time.temporal.ChronoUnit;
import java.util.Date;

@Service
public class JwtServiceImpl implements IJwtService {

    private final SecretKey key;
    private final String issuer;
    private final long expirationHours;

    public JwtServiceImpl(
            @Value("${jwt.secret}") String secret,
            @Value("${jwt.issuer:InsuraTech}") String issuer,
            @Value("${jwt.expiration-ms:86400000}") long expirationMs) {
        this.key = Keys.hmacShaKeyFor(secret.getBytes(StandardCharsets.UTF_8));
        this.issuer = issuer;
        this.expirationHours = expirationMs / 3_600_000;
    }

    @Override
    public String generateToken(User user) {
        Instant now = Instant.now();
        return Jwts.builder()
            .subject(user.getId())
            .issuer(issuer)
            .claim("email",     user.getEmail())
            .claim("role",      user.getRole().name())
            .claim("firstName", user.getProfile().firstName())
            .claim("lastName",  user.getProfile().lastName())
            .issuedAt(Date.from(now))
            .expiration(Date.from(now.plus(expirationHours, ChronoUnit.HOURS)))
            .signWith(key)
            .compact();
    }

    @Override
    public Instant getExpiration() {
        return Instant.now().plus(expirationHours, ChronoUnit.HOURS);
    }

    @Override
    public String extractUserId(String token) {
        return parseClaims(token).getSubject();
    }

    @Override
    public String extractEmail(String token) {
        return parseClaims(token).get("email", String.class);
    }

    @Override
    public boolean isTokenValid(String token) {
        try {
            parseClaims(token);
            return true;
        } catch (Exception e) {
            return false;
        }
    }

    private Claims parseClaims(String token) {
        return Jwts.parser().verifyWith(key).build()
            .parseSignedClaims(token).getPayload();
    }
}
