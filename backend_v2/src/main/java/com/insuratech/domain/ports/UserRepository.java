package com.insuratech.domain.ports;

import com.insuratech.domain.users.Role;
import com.insuratech.domain.users.User;

import java.util.List;
import java.util.Optional;

public interface UserRepository {

    User save(User user);

    Optional<User> findById(String id);

    Optional<User> findByEmail(String email);

    Optional<User> findByFirebaseUid(String firebaseUid);

    List<User> findByRole(Role role);

    List<User> findAllActive();

    void deleteById(String id);

    boolean existsByEmail(String email);
}
