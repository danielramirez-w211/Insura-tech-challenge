package com.insuratech.application.policies.commands;

public record SuspendPolicyCommand(String policyId, String reason, String suspendedBy) {}
