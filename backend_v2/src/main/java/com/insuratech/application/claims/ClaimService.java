package com.insuratech.application.claims;

import com.insuratech.application.claims.commands.*;
import com.insuratech.application.claims.dto.ClaimMapper;
import com.insuratech.application.claims.dto.ClaimResponse;
import com.insuratech.application.common.exceptions.NotFoundException;
import com.insuratech.domain.claims.Claim;
import com.insuratech.domain.claims.ClaimStatus;
import com.insuratech.domain.exceptions.PolicyNotActiveException;
import com.insuratech.domain.ports.ClaimRepository;
import com.insuratech.domain.ports.PolicyRepository;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.Instant;
import java.util.List;

@Service
@RequiredArgsConstructor
public class ClaimService {

    private final ClaimRepository claimRepository;
    private final PolicyRepository policyRepository;

    @Transactional
    public ClaimResponse register(RegisterClaimCommand cmd) {
        var policy = policyRepository.findById(cmd.policyId())
            .orElseThrow(() -> new NotFoundException("Policy", cmd.policyId()));

        if (policy.getStatus().name().equals("ACTIVE") == false)
            throw new PolicyNotActiveException();

        var claim = Claim.register(
            cmd.policyId(),
            cmd.type(),
            cmd.description(),
            cmd.claimedAmount(),
            Instant.from(cmd.incidentDate().atStartOfDay(java.time.ZoneOffset.UTC)),
            cmd.responsibleUser()
        );

        claimRepository.save(claim);
        return ClaimMapper.toResponse(claim);
    }

    @Transactional
    public ClaimResponse startInvestigation(StartInvestigationCommand cmd) {
        var claim = findOrThrow(cmd.claimId());
        claim.startReview(cmd.reviewedBy());
        claimRepository.save(claim);
        return ClaimMapper.toResponse(claim);
    }

    @Transactional
    public ClaimResponse approve(ApproveClaimCommand cmd) {
        var claim = findOrThrow(cmd.claimId());
        claim.approve(cmd.approvedAmount(), cmd.approvedBy());
        claimRepository.save(claim);
        return ClaimMapper.toResponse(claim);
    }

    @Transactional
    public ClaimResponse reject(RejectClaimCommand cmd) {
        var claim = findOrThrow(cmd.claimId());
        claim.reject(cmd.reason(), cmd.rejectedBy());
        claimRepository.save(claim);
        return ClaimMapper.toResponse(claim);
    }

    @Transactional
    public ClaimResponse registerPayment(RegisterPaymentCommand cmd) {
        var claim = findOrThrow(cmd.claimId());
        claim.markAsPaid(cmd.paidBy());
        claimRepository.save(claim);
        return ClaimMapper.toResponse(claim);
    }

    public ClaimResponse getById(String id) {
        return ClaimMapper.toResponse(findOrThrow(id));
    }

    public List<ClaimResponse> getByPolicy(String policyId) {
        return claimRepository.findByPolicyId(policyId).stream()
            .map(ClaimMapper::toResponse).toList();
    }

    @Transactional
    public ClaimResponse appeal(AppealClaimCommand cmd) {
        var claim = findOrThrow(cmd.claimId());
        claim.appeal(cmd.appealedBy(), cmd.reason());
        claimRepository.save(claim);
        return ClaimMapper.toResponse(claim);
    }

    @Transactional
    public ClaimResponse approvePending(ApprovePendingClaimCommand cmd) {
        var claim = findOrThrow(cmd.claimId());
        claim.approvePending(cmd.responsibleUser());
        claimRepository.save(claim);
        return ClaimMapper.toResponse(claim);
    }

    @Transactional
    public ClaimResponse rejectPending(RejectPendingClaimCommand cmd) {
        var claim = findOrThrow(cmd.claimId());
        claim.rejectPending(cmd.responsibleUser(), cmd.reason());
        claimRepository.save(claim);
        return ClaimMapper.toResponse(claim);
    }

    public List<ClaimResponse> getAll() {
        return claimRepository.findByStatus(ClaimStatus.REGISTERED).stream()
            .map(ClaimMapper::toResponse).toList();
    }

    private Claim findOrThrow(String id) {
        return claimRepository.findById(id)
            .orElseThrow(() -> new NotFoundException("Claim", id));
    }
}
