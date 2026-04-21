// Spec: SPEC-013 — auth-user-system
// Origen: CreateAdvisorHandler, CreateLeaderHandler, UpdateProfileHandler, etc. → UserService.java
package com.insuratech.application.users;

import com.insuratech.application.common.exceptions.NotFoundException;
import com.insuratech.application.common.interfaces.IPasswordHasher;
import com.insuratech.application.users.commands.*;
import com.insuratech.application.users.dto.UserMapper;
import com.insuratech.application.users.dto.UserResponse;
import com.insuratech.domain.users.User;
import com.insuratech.domain.users.UserProfile;
import com.insuratech.domain.users.Role;
import com.insuratech.domain.ports.UserRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

@Service
@RequiredArgsConstructor
public class UserService {

    private final UserRepository userRepository;
    private final IPasswordHasher passwordHasher;

    @Transactional
    public UserResponse createAdvisor(CreateAdvisorCommand cmd) {
        if (userRepository.existsByEmail(cmd.email()))
            throw new IllegalArgumentException("Email already in use: " + cmd.email());
        var profile = new UserProfile(cmd.firstName(), cmd.lastName(), cmd.phone(), cmd.city());
        var user = User.createAdvisor(cmd.email(), passwordHasher.hash(cmd.password()), profile);
        userRepository.save(user);
        return UserMapper.toResponse(user);
    }

    @Transactional
    public UserResponse createLeader(CreateLeaderCommand cmd) {
        if (userRepository.existsByEmail(cmd.email()))
            throw new IllegalArgumentException("Email already in use: " + cmd.email());
        var profile = new UserProfile(cmd.firstName(), cmd.lastName(), cmd.phone(), cmd.city());
        var user = User.createLeader(cmd.email(), passwordHasher.hash(cmd.password()), profile);
        userRepository.save(user);
        return UserMapper.toResponse(user);
    }

    @Transactional
    public UserResponse updateProfile(UpdateProfileCommand cmd) {
        var user = findOrThrow(cmd.userId());
        user.updateProfile(new UserProfile(cmd.firstName(), cmd.lastName(), cmd.phone(), cmd.city()));
        userRepository.save(user);
        return UserMapper.toResponse(user);
    }

    @Transactional
    public UserResponse updateStatus(UpdateUserStatusCommand cmd) {
        var user = findOrThrow(cmd.userId());
        if (cmd.active()) user.activate(); else user.deactivate();
        userRepository.save(user);
        return UserMapper.toResponse(user);
    }

    public UserResponse getById(String id) {
        return UserMapper.toResponse(findOrThrow(id));
    }

    public UserResponse getByEmail(String email) {
        return UserMapper.toResponse(
            userRepository.findByEmail(email)
                .orElseThrow(() -> new NotFoundException("User with email", email)));
    }

    public List<UserResponse> getLeaders() {
        return userRepository.findByRole(Role.LEADER).stream().map(UserMapper::toResponse).toList();
    }

    public List<UserResponse> getAdvisors() {
        return userRepository.findByRole(Role.ADVISOR).stream().map(UserMapper::toResponse).toList();
    }

    private User findOrThrow(String id) {
        return userRepository.findById(id)
            .orElseThrow(() -> new NotFoundException("User", id));
    }
}
