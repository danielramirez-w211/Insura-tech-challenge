package com.insuratech.infrastructure.persistence.springdata;

import com.insuratech.domain.claims.Claim;
import com.insuratech.domain.claims.ClaimStatus;
import com.insuratech.domain.claims.ClaimType;
import org.springframework.data.mongodb.repository.MongoRepository;
import org.springframework.stereotype.Repository;

import java.util.List;

@Repository
public interface ClaimSpringRepository extends MongoRepository<Claim, String> {
    List<Claim> findByPolicyIdAndDeletedFalse(String policyId);
    List<Claim> findByStatusAndDeletedFalse(ClaimStatus status);
    List<Claim> findByClaimTypeAndDeletedFalse(ClaimType type);
    List<Claim> findByReportedByAndDeletedFalse(String userId);
    long countByPolicyIdAndStatusAndDeletedFalse(String policyId, ClaimStatus status);
}
