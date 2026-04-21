package com.insuratech.domain.common;

import java.time.Instant;

public interface IDomainEvent {
    Instant occurredOn();
}
