// Spec: SPEC-014 — gestión de siniestros
package com.insuratech.api.controllers;

import com.insuratech.application.claims.ClaimService;
import com.insuratech.application.claims.commands.*;
import com.insuratech.application.claims.dto.ClaimResponse;
import jakarta.validation.Valid;
import jakarta.validation.constraints.NotBlank;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.servlet.support.ServletUriComponentsBuilder;

import java.math.BigDecimal;
import java.util.List;

@RestController
@RequestMapping("/v1/claims")
@RequiredArgsConstructor
public class ClaimsController {

    private final ClaimService claimService;

    private String currentUserId() {
        return (String) SecurityContextHolder.getContext().getAuthentication().getPrincipal();
    }

    private String currentUserEmail() {
        Authentication auth = SecurityContextHolder.getContext().getAuthentication();
        return auth.getDetails() != null ? auth.getDetails().toString() : currentUserId();
    }

    @GetMapping
    public ResponseEntity<List<ClaimResponse>> getAll() {
        return ResponseEntity.ok(claimService.getAll());
    }

    @PostMapping
    public ResponseEntity<ClaimResponse> register(@Valid @RequestBody RegisterClaimCommand body) {
        var cmd = new RegisterClaimCommand(
            body.policyId(), body.type(), body.claimedAmount(),
            body.incidentDate(), body.description(), body.responsibleUser(),
            currentUserId());
        var result = claimService.register(cmd);
        var location = ServletUriComponentsBuilder.fromCurrentRequest()
            .path("/{id}").buildAndExpand(result.id()).toUri();
        return ResponseEntity.created(location).body(result);
    }

    @GetMapping("/{id}")
    public ResponseEntity<ClaimResponse> getById(@PathVariable String id) {
        return ResponseEntity.ok(claimService.getById(id));
    }

    @PutMapping("/{id}/investigate")
    public ResponseEntity<ClaimResponse> investigate(
            @PathVariable String id,
            @RequestBody InvestigateRequest body) {
        return ResponseEntity.ok(claimService.startInvestigation(
            new StartInvestigationCommand(id, body.reviewedBy() != null ? body.reviewedBy() : currentUserId())));
    }

    @PutMapping("/{id}/approve")
    public ResponseEntity<ClaimResponse> approve(
            @PathVariable String id,
            @RequestBody ApproveRequest body) {
        return ResponseEntity.ok(claimService.approve(
            new ApproveClaimCommand(id, body.approvedAmount(), currentUserId())));
    }

    @PutMapping("/{id}/reject")
    public ResponseEntity<ClaimResponse> reject(
            @PathVariable String id,
            @Valid @RequestBody RejectRequest body) {
        return ResponseEntity.ok(claimService.reject(
            new RejectClaimCommand(id, body.reason(), currentUserId())));
    }

    @PutMapping("/{id}/appeal")
    public ResponseEntity<ClaimResponse> appeal(
            @PathVariable String id,
            @Valid @RequestBody AppealRequest body) {
        return ResponseEntity.ok(claimService.appeal(
            new AppealClaimCommand(id, body.reason(), currentUserId())));
    }

    @PutMapping("/{id}/pay")
    public ResponseEntity<ClaimResponse> pay(@PathVariable String id) {
        return ResponseEntity.ok(claimService.registerPayment(
            new RegisterPaymentCommand(id, currentUserId())));
    }

    @PatchMapping("/{id}/approve")
    @PreAuthorize("hasRole('LEADER')")
    public ResponseEntity<ClaimResponse> approvePending(@PathVariable String id) {
        return ResponseEntity.ok(claimService.approvePending(
            new ApprovePendingClaimCommand(id, currentUserEmail())));
    }

    @PatchMapping("/{id}/reject")
    @PreAuthorize("hasRole('LEADER')")
    public ResponseEntity<ClaimResponse> rejectPending(
            @PathVariable String id,
            @Valid @RequestBody RejectPendingRequest body) {
        return ResponseEntity.ok(claimService.rejectPending(
            new RejectPendingClaimCommand(id, body.reason(), currentUserEmail())));
    }

    record InvestigateRequest(String reviewedBy) {}
    record ApproveRequest(BigDecimal approvedAmount) {}
    record RejectRequest(@NotBlank String reason) {}
    record AppealRequest(@NotBlank String reason) {}
    record RejectPendingRequest(@NotBlank String reason) {}
}
