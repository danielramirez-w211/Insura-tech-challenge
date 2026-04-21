// Origen: BcryptPasswordHasher.cs → BcryptPasswordHasherImpl.java
package com.insuratech.infrastructure.auth;

import com.insuratech.application.common.interfaces.IPasswordHasher;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.stereotype.Service;

@Service
public class BcryptPasswordHasherImpl implements IPasswordHasher {

    private final BCryptPasswordEncoder encoder = new BCryptPasswordEncoder();

    @Override
    public String hash(String rawPassword) {
        return encoder.encode(rawPassword);
    }

    @Override
    public boolean verify(String rawPassword, String hash) {
        return encoder.matches(rawPassword, hash);
    }
}
