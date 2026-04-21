package com.insuratech.application.users.dto;
import com.insuratech.domain.users.User;
public final class UserMapper {
    private UserMapper() {}
    public static UserResponse toResponse(User u) {
        return new UserResponse(
            u.getId(), u.getEmail(), u.getRole().name(),
            u.getProfile().firstName(), u.getProfile().lastName(),
            u.getProfile().phone(), u.getProfile().city(),
            u.isActive(), u.getCreatedAt()
        );
    }
}
