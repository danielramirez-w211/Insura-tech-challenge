// Origen: UserRepository.cs → MongoUserRepository.java
package com.insuratech.infrastructure.persistence.repositories;

import com.insuratech.domain.users.Role;
import com.insuratech.domain.users.User;
import com.insuratech.domain.ports.UserRepository;
import com.insuratech.infrastructure.persistence.springdata.UserSpringRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.Optional;

@Component
@RequiredArgsConstructor
public class MongoUserRepository implements UserRepository {

    private final UserSpringRepository springRepo;

    @Override
    public User save(User user) { return springRepo.save(user); }

    @Override
    public Optional<User> findById(String id) {
        return springRepo.findById(id).filter(u -> !u.isDeleted());
    }

    @Override
    public Optional<User> findByEmail(String email) {
        return springRepo.findByEmailAndDeletedFalse(email);
    }

    @Override
    public Optional<User> findByFirebaseUid(String firebaseUid) {
        return springRepo.findByFirebaseUidAndDeletedFalse(firebaseUid);
    }

    @Override
    public List<User> findByRole(Role role) {
        return springRepo.findByRoleAndDeletedFalse(role);
    }

    @Override
    public List<User> findAllActive() {
        return springRepo.findByActiveAndDeletedFalse(true);
    }

    @Override
    public void deleteById(String id) {
        findById(id).ifPresent(u -> {
            u.setDeleted(true);
            springRepo.save(u);
        });
    }

    @Override
    public boolean existsByEmail(String email) {
        return springRepo.existsByEmailAndDeletedFalse(email);
    }
}
