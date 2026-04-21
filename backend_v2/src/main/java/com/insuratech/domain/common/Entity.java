package com.insuratech.domain.common;

import lombok.AccessLevel;
import lombok.Getter;
import lombok.Setter;
import org.springframework.data.annotation.Id;

import java.time.Instant;
import java.util.Objects;
import java.util.UUID;

@Getter
public abstract class Entity {

    @Id
    private String id;
    private Instant createdAt;
    @Setter(AccessLevel.PROTECTED)
    private Instant updatedAt;
    @Setter
    private boolean deleted;
    private int version;

    protected Entity() {
        this.id = UUID.randomUUID().toString();
        this.createdAt = Instant.now();
    }

    public void incrementVersion() {
        this.version++;
    }

    protected void markAsUpdated() {
        this.updatedAt = Instant.now();
    }

    @Override
    public boolean equals(Object obj) {
        if (this == obj) return true;
        if (!(obj instanceof Entity other)) return false;
        return Objects.equals(id, other.id);
    }

    @Override
    public int hashCode() {
        return Objects.hashCode(id);
    }
}
