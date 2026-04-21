package com.insuratech.application.policies.dto;

import java.time.Instant;

public record PolicyStatusHistoryResponse(String status, String notes, Instant changedAt) {}
