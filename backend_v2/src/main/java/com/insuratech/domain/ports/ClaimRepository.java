package com.insuratech.domain.ports;

import com.insuratech.domain.claims.Claim;
import com.insuratech.domain.claims.ClaimStatus;
import com.insuratech.domain.claims.ClaimType;

import java.util.List;
import java.util.Optional;

public interface ClaimRepository {

    Claim save(Claim claim);

    Optional<Claim> findById(String id);

    List<Claim> findByPolicyId(String policyId);

    List<Claim> findByStatus(ClaimStatus status);

    List<Claim> findByType(ClaimType type);

    List<Claim> findByReportedBy(String userId);

    long countByPolicyIdAndStatus(String policyId, ClaimStatus status);

    void deleteById(String id);
}
