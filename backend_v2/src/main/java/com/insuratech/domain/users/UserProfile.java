package com.insuratech.domain.users;

public record UserProfile(
    String firstName,
    String lastName,
    String phone,
    String city
) {
    public String fullName() {
        return firstName + " " + lastName;
    }
}
