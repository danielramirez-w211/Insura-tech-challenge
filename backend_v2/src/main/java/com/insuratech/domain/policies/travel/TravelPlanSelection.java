package com.insuratech.domain.policies.travel;

import lombok.Getter;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.List;

@Getter
public class TravelPlanSelection {

    private final TripType tripType;
    private final List<Continent> destinations;
    private final LocalDate departureDate;
    private final LocalDate returnDate;
    private final int numberOfTravelers;
    private final BigDecimal calculatedPremium;

    public TravelPlanSelection(TripType tripType, List<Continent> destinations,
                               LocalDate departureDate, LocalDate returnDate, int numberOfTravelers) {
        this.tripType = tripType;
        this.destinations = List.copyOf(destinations);
        this.departureDate = departureDate;
        this.returnDate = returnDate;
        this.numberOfTravelers = numberOfTravelers;
        this.calculatedPremium = TravelRatingService.calculatePremium(
            tripType, destinations, departureDate, returnDate, numberOfTravelers);
    }
}
