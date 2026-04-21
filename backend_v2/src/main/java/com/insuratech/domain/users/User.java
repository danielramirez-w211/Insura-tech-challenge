package com.insuratech.domain.users;

import com.insuratech.domain.common.AggregateRoot;
import lombok.Getter;
import org.springframework.data.mongodb.core.mapping.Document;

@Getter
@Document(collection = "users")
public class User extends AggregateRoot {

    private String email;
    private String passwordHash;
    private Role role;
    private UserProfile profile;
    private boolean active;
    private String firebaseUid;

    protected User() { super(); }

    public static User createAdmin(String email, String passwordHash, UserProfile profile) {
        User user = new User();
        user.email = email;
        user.passwordHash = passwordHash;
        user.role = Role.ADMIN;
        user.profile = profile;
        user.active = true;
        return user;
    }

    public static User createLeader(String email, String passwordHash, UserProfile profile) {
        User user = new User();
        user.email = email;
        user.passwordHash = passwordHash;
        user.role = Role.LEADER;
        user.profile = profile;
        user.active = true;
        return user;
    }

    public static User createAdvisor(String email, String passwordHash, UserProfile profile) {
        User user = new User();
        user.email = email;
        user.passwordHash = passwordHash;
        user.role = Role.ADVISOR;
        user.profile = profile;
        user.active = true;
        return user;
    }

    public void deactivate() {
        this.active = false;
        markAsUpdated();
    }

    public void activate() {
        this.active = true;
        markAsUpdated();
    }

    public void updateProfile(UserProfile profile) {
        this.profile = profile;
        markAsUpdated();
    }

    public void setFirebaseUid(String firebaseUid) {
        this.firebaseUid = firebaseUid;
        markAsUpdated();
    }

    public void changePassword(String newPasswordHash) {
        this.passwordHash = newPasswordHash;
        markAsUpdated();
    }
}
