package com.insuratech.domain.policies.vo;

import com.insuratech.domain.common.ValueObject;
import com.insuratech.domain.exceptions.InvalidPolicyPeriodException;

import java.time.LocalDate;

public final class CoveragePeriod extends ValueObject {

    private final LocalDate startDate;
    private final LocalDate endDate;

    private CoveragePeriod(LocalDate startDate, LocalDate endDate) {
        this.startDate = startDate;
        this.endDate = endDate;
    }

    public static CoveragePeriod of(LocalDate startDate, LocalDate endDate) {
        if (startDate == null || endDate == null || !endDate.isAfter(startDate)) {
            throw new InvalidPolicyPeriodException();
        }
        return new CoveragePeriod(startDate, endDate);
    }

    public LocalDate getStartDate() { return startDate; }
    public LocalDate getEndDate()   { return endDate; }

    public boolean isActive() {
        LocalDate today = LocalDate.now();
        return !today.isBefore(startDate) && !today.isAfter(endDate);
    }

    public boolean isExpiringSoon(int daysThreshold) {
        return LocalDate.now().plusDays(daysThreshold).isAfter(endDate);
    }

    @Override
    protected Object[] getEqualityComponents() {
        return new Object[]{startDate, endDate};
    }
}
