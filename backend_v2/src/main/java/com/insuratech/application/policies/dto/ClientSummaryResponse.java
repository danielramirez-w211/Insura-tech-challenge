package com.insuratech.application.policies.dto;

public record ClientSummaryResponse(
    String policyId, String policyNumber,
    String insuredFullName, String insuredDocument,
    String status, String type
) {}
