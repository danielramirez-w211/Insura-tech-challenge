package com.insuratech.application.common.interfaces;

import java.math.BigDecimal;
import java.time.LocalDate;

public interface ITrmService {
    BigDecimal getCurrentTrm();
    BigDecimal getTrmForDate(LocalDate date);
}
