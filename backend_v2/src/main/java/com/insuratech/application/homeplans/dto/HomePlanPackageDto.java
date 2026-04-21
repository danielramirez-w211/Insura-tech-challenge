package com.insuratech.application.homeplans.dto;

import java.util.List;

public record HomePlanPackageDto(String packageId, String packageName, List<String> coverages) {}
