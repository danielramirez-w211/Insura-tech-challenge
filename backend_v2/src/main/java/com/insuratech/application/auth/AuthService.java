package com.insuratech.application.auth;

import com.insuratech.application.auth.commands.LoginCommand;
import com.insuratech.application.auth.dto.LoginResponse;
import com.insuratech.application.common.interfaces.IJwtService;
import com.insuratech.application.common.interfaces.IPasswordHasher;
import com.insuratech.domain.ports.UserRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

@Service
@RequiredArgsConstructor
public class AuthService {

    private final UserRepository userRepository;
    private final IJwtService jwtService;
    private final IPasswordHasher passwordHasher;

    public LoginResponse login(LoginCommand command) {
        var user = userRepository.findByEmail(command.email())
            .orElseThrow(() -> new org.springframework.security.authentication
                .BadCredentialsException("Invalid credentials."));

        if (!user.isActive())
            throw new org.springframework.security.authentication
                .DisabledException("Account is deactivated.");

        if (!passwordHasher.verify(command.password(), user.getPasswordHash()))
            throw new org.springframework.security.authentication
                .BadCredentialsException("Invalid credentials.");

        String token = jwtService.generateToken(user);

        return new LoginResponse(
            token,
            user.getRole().name(),
            user.getId(),
            user.getEmail(),
            user.getProfile().firstName(),
            user.getProfile().lastName(),
            jwtService.getExpiration()
        );
    }
}
