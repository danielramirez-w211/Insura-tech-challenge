package com.insuratech.application.users.commands;
public record UpdateUserStatusCommand(String userId, boolean active, String updatedBy) {}
