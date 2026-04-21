package com.insuratech.domain.common;

public abstract class ValueObject {

    protected abstract Object[] getEqualityComponents();

    @Override
    public boolean equals(Object obj) {
        if (this == obj) return true;
        if (obj == null || getClass() != obj.getClass()) return false;
        ValueObject other = (ValueObject) obj;
        Object[] components = getEqualityComponents();
        Object[] otherComponents = other.getEqualityComponents();
        if (components.length != otherComponents.length) return false;
        for (int i = 0; i < components.length; i++) {
            if (components[i] == null && otherComponents[i] == null) continue;
            if (components[i] == null || !components[i].equals(otherComponents[i])) return false;
        }
        return true;
    }

    @Override
    public int hashCode() {
        int result = 1;
        for (Object component : getEqualityComponents()) {
            result = 31 * result + (component == null ? 0 : component.hashCode());
        }
        return result;
    }
}
