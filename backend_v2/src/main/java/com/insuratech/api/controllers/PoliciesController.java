// Spec: SPEC-010..016 — gestión de pólizas
package com.insuratech.api.controllers;

import com.insuratech.application.claims.ClaimService;
import com.insuratech.application.claims.dto.ClaimResponse;
import com.insuratech.application.policies.PolicyService;
import com.insuratech.application.policies.commands.*;
import com.insuratech.application.policies.dto.ClientSummaryResponse;
import com.insuratech.application.policies.dto.PolicyResponse;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.servlet.support.ServletUriComponentsBuilder;

import java.util.List;

@RestController
@RequestMapping("/v1/policies")
@RequiredArgsConstructor
public class PoliciesController {

    private final PolicyService policyService;
    private final ClaimService claimService;

    private String currentUserId() {
        return (String) SecurityContextHolder.getContext().getAuthentication().getPrincipal();
    }

    @PostMapping
    public ResponseEntity<PolicyResponse> create(
            @Valid @RequestBody CreatePolicyCommand cmd,
            @RequestHeader(value = "Idempotency-Key", required = false) String idempotencyKey) {
        var result = policyService.create(cmd);
        var location = ServletUriComponentsBuilder.fromCurrentRequest()
            .path("/{id}").buildAndExpand(result.id()).toUri();
        return ResponseEntity.created(location).body(result);
    }

    @GetMapping
    public ResponseEntity<List<PolicyResponse>> getAll() {
        return ResponseEntity.ok(policyService.getAll());
    }

    @GetMapping("/my-clients")
    @PreAuthorize("hasRole('ADVISOR')")
    public ResponseEntity<List<ClientSummaryResponse>> getMyClients() {
        return ResponseEntity.ok(policyService.getClientSummaries(currentUserId()));
    }

    @GetMapping("/{id}")
    public ResponseEntity<PolicyResponse> getById(@PathVariable String id) {
        return ResponseEntity.ok(policyService.getById(id));
    }

    @PutMapping("/{id}/activate")
    public ResponseEntity<PolicyResponse> activate(
            @PathVariable String id) {
        var result = policyService.activate(new ActivatePolicyCommand(id, currentUserId()));
        return ResponseEntity.ok(result);
    }

    @PutMapping("/{id}/suspend")
    public ResponseEntity<PolicyResponse> suspend(
            @PathVariable String id,
            @Valid @RequestBody SuspendPolicyCommand body) {
        var cmd = new SuspendPolicyCommand(id, body.reason(), body.suspendedBy());
        return ResponseEntity.ok(policyService.suspend(cmd));
    }

    @PutMapping("/{id}/cancel")
    public ResponseEntity<PolicyResponse> cancel(
            @PathVariable String id,
            @Valid @RequestBody CancelPolicyCommand body) {
        var cmd = new CancelPolicyCommand(id, body.reason(), body.cancelledBy());
        return ResponseEntity.ok(policyService.cancel(cmd));
    }

    @GetMapping("/{id}/claims")
    public ResponseEntity<List<ClaimResponse>> getClaims(@PathVariable String id) {
        return ResponseEntity.ok(claimService.getByPolicy(id));
    }

    @PostMapping("/{id}/renew")
    public ResponseEntity<PolicyResponse> renew(
            @PathVariable String id,
            @Valid @RequestBody RenewPolicyCommand body) {
        var cmd = new RenewPolicyCommand(id, body.newEndDate(), body.renewedBy());
        var result = policyService.renew(cmd);
        var location = ServletUriComponentsBuilder.fromCurrentRequestUri()
            .replacePath("/v1/policies/{id}").buildAndExpand(result.id()).toUri();
        return ResponseEntity.created(location).body(result);
    }
}
