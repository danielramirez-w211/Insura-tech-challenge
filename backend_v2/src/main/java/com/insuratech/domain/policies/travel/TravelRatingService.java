package com.insuratech.domain.policies.travel;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.LocalDate;
import java.time.temporal.ChronoUnit;
import java.util.List;

public final class TravelRatingService {

    private static final BigDecimal BASE_DAILY_RATE = new BigDecimal("15000");

    private TravelRatingService() {}

    public static BigDecimal calculatePremium(TripType tripType, List<Continent> destinations,
                                              LocalDate departure, LocalDate returnDate,
                                              int travelers) {
        long days = Math.max(1, ChronoUnit.DAYS.between(departure, returnDate));
        BigDecimal base = BASE_DAILY_RATE.multiply(BigDecimal.valueOf(days));

        BigDecimal continentFactor = destinations.stream()
            .map(TravelRatingService::getContinentFactor)
            .max(BigDecimal::compareTo)
            .orElse(BigDecimal.ONE);

        BigDecimal tripTypeFactor = getTripTypeFactor(tripType);
        BigDecimal travelerFactor = BigDecimal.valueOf(travelers);

        return base.multiply(continentFactor)
                   .multiply(tripTypeFactor)
                   .multiply(travelerFactor)
                   .setScale(0, RoundingMode.HALF_UP);
    }

    private static BigDecimal getContinentFactor(Continent continent) {
        return switch (continent) {
            case SOUTH_AMERICA -> BigDecimal.ONE;
            case NORTH_AMERICA -> new BigDecimal("1.30");
            case EUROPE        -> new BigDecimal("1.25");
            case ASIA          -> new BigDecimal("1.35");
            case AFRICA        -> new BigDecimal("1.40");
            case OCEANIA       -> new BigDecimal("1.30");
        };
    }

    private static BigDecimal getTripTypeFactor(TripType type) {
        return switch (type) {
            case SINGLE     -> BigDecimal.ONE;
            case MULTI_TRIP -> new BigDecimal("0.85");
            case ANNUAL     -> new BigDecimal("0.70");
        };
    }
}
