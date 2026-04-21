package com.insuratech.domain.policies;

import com.insuratech.domain.common.AggregateRoot;
import com.insuratech.domain.common.PolicyStatusHistory;
import com.insuratech.domain.events.PolicyActivatedEvent;
import com.insuratech.domain.events.PolicyCancelledEvent;
import com.insuratech.domain.events.PolicyExpiringSoonEvent;
import com.insuratech.domain.exceptions.*;
import com.insuratech.domain.policies.health.HealthPlanSelection;
import com.insuratech.domain.policies.home.HomePlanSelection;
import com.insuratech.domain.policies.life.LifePlanSelection;
import com.insuratech.domain.policies.travel.TravelPlanSelection;
import com.insuratech.domain.policies.vehicle.VehiclePlanSelection;
import com.insuratech.domain.policies.vo.CoveragePeriod;
import com.insuratech.domain.policies.vo.InsuredPerson;
import com.insuratech.domain.policies.vo.PolicyNumber;
import lombok.Getter;
import org.springframework.data.mongodb.core.mapping.Document;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

@Getter
@Document(collection = "policies")
public class Policy extends AggregateRoot {

    private PolicyNumber policyNumber;
    private PolicyType policyType;
    private PolicyStatus status;
    private InsuredPerson insuredPerson;
    private CoveragePeriod coveragePeriod;
    private BigDecimal premium;
    private BigDecimal insuredAmount;
    private BigDecimal remainingInsuredAmount;
    private String agentId;
    private String cityId;
    private String notes;

    private HealthPlanSelection healthPlanSelection;
    private LifePlanSelection lifePlanSelection;
    private VehiclePlanSelection vehiclePlanSelection;
    private HomePlanSelection homePlanSelection;
    private TravelPlanSelection travelPlanSelection;

    private final List<PolicyStatusHistory> statusHistory = new ArrayList<>();

    protected Policy() { super(); }

    public static Policy create(PolicyNumber policyNumber, PolicyType policyType,
                                InsuredPerson insuredPerson, CoveragePeriod coveragePeriod,
                                BigDecimal premium, BigDecimal insuredAmount,
                                String agentId, String cityId) {
        Policy policy = new Policy();
        policy.policyNumber = policyNumber;
        policy.policyType = policyType;
        policy.status = PolicyStatus.DRAFT;
        policy.insuredPerson = insuredPerson;
        policy.coveragePeriod = coveragePeriod;
        policy.premium = premium;
        policy.insuredAmount = insuredAmount;
        policy.remainingInsuredAmount = insuredAmount;
        policy.agentId = agentId;
        policy.cityId = cityId;
        return policy;
    }

    public static Policy createHealthPolicy(PolicyNumber policyNumber, InsuredPerson insuredPerson,
                                            CoveragePeriod coveragePeriod, HealthPlanSelection selection,
                                            String agentId, String cityId) {
        Policy policy = create(policyNumber, PolicyType.HEALTH, insuredPerson, coveragePeriod,
            selection.getCalculatedPremium(), selection.getPlan().getCoverageLimit(), agentId, cityId);
        policy.healthPlanSelection = selection;
        return policy;
    }

    public static Policy createLifePolicy(PolicyNumber policyNumber, InsuredPerson insuredPerson,
                                          CoveragePeriod coveragePeriod, LifePlanSelection selection,
                                          String agentId, String cityId) {
        Policy policy = create(policyNumber, PolicyType.LIFE, insuredPerson, coveragePeriod,
            selection.getCalculatedPremium(), selection.getPlan().getCoverageAmount(), agentId, cityId);
        policy.lifePlanSelection = selection;
        return policy;
    }

    public static Policy createVehiclePolicy(PolicyNumber policyNumber, InsuredPerson insuredPerson,
                                             CoveragePeriod coveragePeriod, VehiclePlanSelection selection,
                                             String agentId, String cityId) {
        Policy policy = create(policyNumber, PolicyType.VEHICLE, insuredPerson, coveragePeriod,
            selection.getCalculatedPremium(), selection.getQuotation().getVehicleValue(), agentId, cityId);
        policy.vehiclePlanSelection = selection;
        return policy;
    }

    public static Policy createHomePolicy(PolicyNumber policyNumber, InsuredPerson insuredPerson,
                                          CoveragePeriod coveragePeriod, HomePlanSelection selection,
                                          String agentId, String cityId) {
        Policy policy = create(policyNumber, PolicyType.HOME, insuredPerson, coveragePeriod,
            selection.getCalculatedPremium(), selection.getQuotation().getPropertyValue(), agentId, cityId);
        policy.homePlanSelection = selection;
        return policy;
    }

    public static Policy createTravelPolicy(PolicyNumber policyNumber, InsuredPerson insuredPerson,
                                            CoveragePeriod coveragePeriod, TravelPlanSelection selection,
                                            String agentId, String cityId) {
        Policy policy = create(policyNumber, PolicyType.TRAVEL, insuredPerson, coveragePeriod,
            selection.getCalculatedPremium(), BigDecimal.ZERO, agentId, cityId);
        policy.travelPlanSelection = selection;
        return policy;
    }

    public void activate(String activatedBy) {
        if (status == PolicyStatus.ACTIVE) throw new PolicyAlreadyActiveException();
        status = PolicyStatus.ACTIVE;
        statusHistory.add(new PolicyStatusHistory(PolicyStatus.ACTIVE.name(), activatedBy, "Policy activated"));
        markAsUpdated();
        addDomainEvent(new PolicyActivatedEvent(getId(), policyNumber.getValue()));
    }

    public void suspend(String suspendedBy, String reason) {
        if (status != PolicyStatus.ACTIVE) throw new PolicyNotActiveException();
        status = PolicyStatus.SUSPENDED;
        statusHistory.add(new PolicyStatusHistory(PolicyStatus.SUSPENDED.name(), suspendedBy, reason));
        markAsUpdated();
    }

    public void cancel(String cancelledBy, String reason) {
        if (status == PolicyStatus.CANCELLED)
            throw new BusinessRuleException("PolicyAlreadyCancelled", "Policy is already cancelled.");
        status = PolicyStatus.CANCELLED;
        statusHistory.add(new PolicyStatusHistory(PolicyStatus.CANCELLED.name(), cancelledBy, reason));
        markAsUpdated();
        addDomainEvent(new PolicyCancelledEvent(getId(), reason));
    }

    public void renew(LocalDate newEndDate, String renewedBy) {
        if (status != PolicyStatus.ACTIVE && status != PolicyStatus.EXPIRED)
            throw new BusinessRuleException("PolicyNotRenewable", "Only active or expired policies can be renewed.");
        coveragePeriod = CoveragePeriod.of(coveragePeriod.getEndDate().plusDays(1), newEndDate);
        status = PolicyStatus.ACTIVE;
        statusHistory.add(new PolicyStatusHistory(PolicyStatus.ACTIVE.name(), renewedBy, "Policy renewed"));
        markAsUpdated();
    }

    public void deductInsuredAmount(BigDecimal amount) {
        if (status != PolicyStatus.ACTIVE) throw new PolicyNotActiveException();
        if (amount.compareTo(remainingInsuredAmount) > 0)
            throw new InsufficientInsuredAmountException(amount, remainingInsuredAmount);
        remainingInsuredAmount = remainingInsuredAmount.subtract(amount);
        markAsUpdated();
    }

    public void checkAndRaiseExpiringEvent(int daysThreshold) {
        if (status == PolicyStatus.ACTIVE && coveragePeriod.isExpiringSoon(daysThreshold)) {
            addDomainEvent(new PolicyExpiringSoonEvent(
                getId(), policyNumber.getValue(), coveragePeriod.getEndDate()));
        }
    }

    public List<PolicyStatusHistory> getStatusHistory() {
        return Collections.unmodifiableList(statusHistory);
    }
}
