package com.insuratech.domain.exceptions;

import java.math.BigDecimal;

public class InsufficientInsuredAmountException extends BusinessRuleException {
    public InsufficientInsuredAmountException(BigDecimal requested, BigDecimal available) {
        super("InsufficientInsuredAmount",
            String.format("Requested amount %.2f exceeds available insured amount %.2f.", requested, available));
    }
}
