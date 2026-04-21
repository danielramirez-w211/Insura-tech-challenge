package com.insuratech.infrastructure.persistence.springdata;

import com.insuratech.domain.users.Role;
import com.insuratech.domain.users.User;
import org.springframework.data.mongodb.repository.MongoRepository;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;

@Repository
public interface UserSpringRepository extends MongoRepository<User, String> {
    Optional<User> findByEmailAndDeletedFalse(String email);
    Optional<User> findByFirebaseUidAndDeletedFalse(String firebaseUid);
    List<User> findByRoleAndDeletedFalse(Role role);
    List<User> findByActiveAndDeletedFalse(boolean active);
    boolean existsByEmailAndDeletedFalse(String email);
}
