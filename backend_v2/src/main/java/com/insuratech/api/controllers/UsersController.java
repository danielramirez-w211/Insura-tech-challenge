// Spec: SPEC-013 — auth-user-system
package com.insuratech.api.controllers;

import com.insuratech.application.users.UserService;
import com.insuratech.application.users.commands.*;
import com.insuratech.application.users.dto.UserResponse;
import jakarta.validation.Valid;
import jakarta.validation.constraints.NotBlank;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.servlet.support.ServletUriComponentsBuilder;

import java.util.List;

@RestController
@RequestMapping("/v1/users")
@RequiredArgsConstructor
public class UsersController {

    private final UserService userService;

    private String currentUserId() {
        return (String) SecurityContextHolder.getContext().getAuthentication().getPrincipal();
    }

    @GetMapping("/me")
    public ResponseEntity<UserResponse> getMe() {
        return ResponseEntity.ok(userService.getById(currentUserId()));
    }

    @PutMapping("/me/profile")
    public ResponseEntity<UserResponse> updateMyProfile(@Valid @RequestBody UpdateProfileRequest body) {
        var cmd = new UpdateProfileCommand(currentUserId(), body.firstName(), body.lastName(),
            body.phone(), body.city());
        return ResponseEntity.ok(userService.updateProfile(cmd));
    }

    @PostMapping("/leaders")
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<UserResponse> createLeader(@Valid @RequestBody CreateLeaderCommand cmd) {
        var result = userService.createLeader(cmd);
        var location = ServletUriComponentsBuilder.fromCurrentContextPath()
            .path("/v1/users/me").build().toUri();
        return ResponseEntity.created(location).body(result);
    }

    @GetMapping("/leaders")
    @PreAuthorize("hasRole('ADMIN')")
    public ResponseEntity<List<UserResponse>> getLeaders() {
        return ResponseEntity.ok(userService.getLeaders());
    }

    @PostMapping("/advisors")
    @PreAuthorize("hasRole('LEADER')")
    public ResponseEntity<UserResponse> createAdvisor(@Valid @RequestBody CreateAdvisorCommand body) {
        var cmd = new CreateAdvisorCommand(body.email(), body.password(),
            body.firstName(), body.lastName(), body.phone(), body.city(), currentUserId());
        var result = userService.createAdvisor(cmd);
        var location = ServletUriComponentsBuilder.fromCurrentContextPath()
            .path("/v1/users/me").build().toUri();
        return ResponseEntity.created(location).body(result);
    }

    @GetMapping("/my-advisors")
    @PreAuthorize("hasRole('LEADER')")
    public ResponseEntity<List<UserResponse>> getMyAdvisors() {
        return ResponseEntity.ok(userService.getAdvisors());
    }

    @PatchMapping("/{id}/status")
    @PreAuthorize("hasAnyRole('ADMIN','LEADER')")
    public ResponseEntity<UserResponse> updateStatus(
            @PathVariable String id,
            @Valid @RequestBody UpdateStatusRequest body) {
        var cmd = new UpdateUserStatusCommand(id, body.active(), currentUserId());
        return ResponseEntity.ok(userService.updateStatus(cmd));
    }

    record UpdateProfileRequest(@NotBlank String firstName, @NotBlank String lastName,
                                String phone, String city) {}
    record UpdateStatusRequest(boolean active) {}
}
