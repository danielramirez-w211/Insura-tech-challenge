package com.insuratech.application.users.commands;
public record UpdateProfileCommand(
    String userId, String firstName, String lastName, String phone, String city
) {}
