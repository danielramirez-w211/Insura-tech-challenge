// Origen: ClaimRepository.cs → MongoClaimRepository.java
package com.insuratech.infrastructure.persistence.repositories;

import com.insuratech.domain.claims.Claim;
import com.insuratech.domain.claims.ClaimStatus;
import com.insuratech.domain.claims.ClaimType;
import com.insuratech.domain.ports.ClaimRepository;
import com.insuratech.infrastructure.persistence.springdata.ClaimSpringRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.Optional;

@Component
@RequiredArgsConstructor
public class MongoClaimRepository implements ClaimRepository {

    private final ClaimSpringRepository springRepo;

    @Override
    public Claim save(Claim claim) { return springRepo.save(claim); }

    @Override
    public Optional<Claim> findById(String id) {
        return springRepo.findById(id).filter(c -> !c.isDeleted());
    }

    @Override
    public List<Claim> findByPolicyId(String policyId) {
        return springRepo.findByPolicyIdAndDeletedFalse(policyId);
    }

    @Override
    public List<Claim> findByStatus(ClaimStatus status) {
        return springRepo.findByStatusAndDeletedFalse(status);
    }

    @Override
    public List<Claim> findByType(ClaimType type) {
        return springRepo.findByClaimTypeAndDeletedFalse(type);
    }

    @Override
    public List<Claim> findByReportedBy(String userId) {
        return springRepo.findByReportedByAndDeletedFalse(userId);
    }

    @Override
    public long countByPolicyIdAndStatus(String policyId, ClaimStatus status) {
        return springRepo.countByPolicyIdAndStatusAndDeletedFalse(policyId, status);
    }

    @Override
    public void deleteById(String id) {
        findById(id).ifPresent(c -> {
            c.setDeleted(true);
            springRepo.save(c);
        });
    }
}
