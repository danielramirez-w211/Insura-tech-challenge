package com.insuratech.domain.policies.vo;

import com.insuratech.domain.common.ValueObject;
import com.insuratech.domain.exceptions.InvalidPolicyNumberException;

import java.util.regex.Pattern;

public final class PolicyNumber extends ValueObject {

    private static final Pattern PATTERN = Pattern.compile("^POL-\\d{4}-\\d{6}$");

    private final String value;

    private PolicyNumber(String value) {
        this.value = value;
    }

    public static PolicyNumber of(String value) {
        if (value == null || !PATTERN.matcher(value).matches()) {
            throw new InvalidPolicyNumberException(value);
        }
        return new PolicyNumber(value);
    }

    public static PolicyNumber generate(int year, long sequence) {
        return new PolicyNumber(String.format("POL-%d-%06d", year, sequence));
    }

    public String getValue() {
        return value;
    }

    @Override
    protected Object[] getEqualityComponents() {
        return new Object[]{value};
    }

    @Override
    public String toString() {
        return value;
    }
}
