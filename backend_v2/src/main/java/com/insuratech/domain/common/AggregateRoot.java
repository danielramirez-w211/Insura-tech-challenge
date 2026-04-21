package com.insuratech.domain.common;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

public abstract class AggregateRoot extends Entity {

    private final List<IDomainEvent> domainEvents = new ArrayList<>();

    protected AggregateRoot() {
        super();
    }

    protected void addDomainEvent(IDomainEvent event) {
        domainEvents.add(event);
    }

    public List<IDomainEvent> getDomainEvents() {
        return Collections.unmodifiableList(domainEvents);
    }

    public void clearDomainEvents() {
        domainEvents.clear();
    }
}
