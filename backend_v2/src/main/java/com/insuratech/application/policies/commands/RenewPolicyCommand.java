package com.insuratech.application.policies.commands;

import java.time.LocalDate;

public record RenewPolicyCommand(String policyId, LocalDate newEndDate, String renewedBy) {}
