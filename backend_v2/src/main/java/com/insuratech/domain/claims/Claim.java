package com.insuratech.domain.claims;

import com.insuratech.domain.common.AggregateRoot;
import com.insuratech.domain.events.ClaimRegisteredEvent;
import com.insuratech.domain.events.ClaimStatusChangedEvent;
import com.insuratech.domain.exceptions.ClaimAlreadyClosedException;
import com.insuratech.domain.exceptions.InvalidClaimTransitionException;
import lombok.Getter;
import org.springframework.data.mongodb.core.mapping.Document;

import java.math.BigDecimal;
import java.time.Instant;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Set;

@Getter
@Document(collection = "claims")
public class Claim extends AggregateRoot {

    private String policyId;
    private ClaimType claimType;
    private ClaimStatus status;
    private String description;
    private BigDecimal claimedAmount;
    private BigDecimal approvedAmount;
    private Instant incidentDate;
    private String reportedBy;
    private String reviewedBy;
    private String rejectionReason;

    private final List<ClaimStatusHistory> statusHistory = new ArrayList<>();

    private static final Set<ClaimStatus> CLOSED_STATUSES = Set.of(
        ClaimStatus.PAID, ClaimStatus.REJECTED, ClaimStatus.CLOSED
    );

    protected Claim() { super(); }

    public static Claim register(String policyId, ClaimType claimType, String description,
                                 BigDecimal claimedAmount, Instant incidentDate, String reportedBy) {
        Claim claim = new Claim();
        claim.policyId = policyId;
        claim.claimType = claimType;
        claim.status = ClaimStatus.REGISTERED;
        claim.description = description;
        claim.claimedAmount = claimedAmount;
        claim.incidentDate = incidentDate;
        claim.reportedBy = reportedBy;
        claim.statusHistory.add(new ClaimStatusHistory(ClaimStatus.REGISTERED, reportedBy, "Claim registered"));
        claim.addDomainEvent(new ClaimRegisteredEvent(claim.getId(), policyId, claimType.name()));
        return claim;
    }

    public void startReview(String reviewedBy) {
        assertNotClosed();
        assertValidTransition(ClaimStatus.UNDER_REVIEW);
        ClaimStatus previous = this.status;
        this.status = ClaimStatus.UNDER_REVIEW;
        this.reviewedBy = reviewedBy;
        statusHistory.add(new ClaimStatusHistory(ClaimStatus.UNDER_REVIEW, reviewedBy, "Review started"));
        markAsUpdated();
        addDomainEvent(new ClaimStatusChangedEvent(getId(), previous.name(), status.name()));
    }

    public void approve(BigDecimal approvedAmount, String approvedBy) {
        assertNotClosed();
        assertValidTransition(ClaimStatus.APPROVED);
        ClaimStatus previous = this.status;
        this.status = ClaimStatus.APPROVED;
        this.approvedAmount = approvedAmount;
        statusHistory.add(new ClaimStatusHistory(ClaimStatus.APPROVED, approvedBy, "Claim approved"));
        markAsUpdated();
        addDomainEvent(new ClaimStatusChangedEvent(getId(), previous.name(), status.name()));
    }

    public void reject(String reason, String rejectedBy) {
        assertNotClosed();
        assertValidTransition(ClaimStatus.REJECTED);
        ClaimStatus previous = this.status;
        this.status = ClaimStatus.REJECTED;
        this.rejectionReason = reason;
        statusHistory.add(new ClaimStatusHistory(ClaimStatus.REJECTED, rejectedBy, reason));
        markAsUpdated();
        addDomainEvent(new ClaimStatusChangedEvent(getId(), previous.name(), status.name()));
    }

    public void markAsPaid(String paidBy) {
        assertNotClosed();
        assertValidTransition(ClaimStatus.PAID);
        ClaimStatus previous = this.status;
        this.status = ClaimStatus.PAID;
        statusHistory.add(new ClaimStatusHistory(ClaimStatus.PAID, paidBy, "Payment processed"));
        markAsUpdated();
        addDomainEvent(new ClaimStatusChangedEvent(getId(), previous.name(), status.name()));
    }

    public void close(String closedBy, String notes) {
        assertNotClosed();
        ClaimStatus previous = this.status;
        this.status = ClaimStatus.CLOSED;
        statusHistory.add(new ClaimStatusHistory(ClaimStatus.CLOSED, closedBy, notes));
        markAsUpdated();
        addDomainEvent(new ClaimStatusChangedEvent(getId(), previous.name(), status.name()));
    }

    private void assertNotClosed() {
        if (CLOSED_STATUSES.contains(status)) throw new ClaimAlreadyClosedException();
    }

    private void assertValidTransition(ClaimStatus target) {
        boolean valid = switch (target) {
            case UNDER_REVIEW -> status == ClaimStatus.REGISTERED;
            case APPROVED, REJECTED -> status == ClaimStatus.UNDER_REVIEW;
            case PAID -> status == ClaimStatus.APPROVED;
            default -> false;
        };
        if (!valid) throw new InvalidClaimTransitionException(status.name(), target.name());
    }

    public List<ClaimStatusHistory> getStatusHistory() {
        return Collections.unmodifiableList(statusHistory);
    }
}
