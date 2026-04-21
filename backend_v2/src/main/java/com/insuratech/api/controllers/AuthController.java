// Spec: SPEC-013 — auth-user-system
package com.insuratech.api.controllers;

import com.insuratech.application.auth.AuthService;
import com.insuratech.application.auth.commands.LoginCommand;
import com.insuratech.application.auth.dto.LoginRequest;
import com.insuratech.application.auth.dto.LoginResponse;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/v1/auth")
@RequiredArgsConstructor
public class AuthController {

    private final AuthService authService;

    @PostMapping("/login")
    public ResponseEntity<LoginResponse> login(@Valid @RequestBody LoginRequest request) {
        var result = authService.login(new LoginCommand(request.email(), request.password()));
        return ResponseEntity.ok(result);
    }
}
