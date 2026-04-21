package com.insuratech.application.travelplans;

import com.insuratech.application.common.interfaces.ITrmService;
import com.insuratech.application.travelplans.dto.TravelPlanCalculationDto;
import com.insuratech.domain.policies.travel.Continent;
import com.insuratech.domain.policies.travel.TravelRatingService;
import com.insuratech.domain.policies.travel.TripType;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.time.Instant;
import java.time.LocalDate;
import java.util.List;

@Service
@RequiredArgsConstructor
public class TravelPlanService {

    private final ITrmService trmService;

    public TravelPlanCalculationDto calculate(TripType tripType, Continent continent, int durationDays) {
        LocalDate departure = LocalDate.now();
        LocalDate returnDate = departure.plusDays(durationDays);

        List<Continent> destinations = continent != null ? List.of(continent) : List.of();
        BigDecimal totalCop = TravelRatingService.calculatePremium(
            tripType, destinations, departure, returnDate, 1);

        BigDecimal trm = trmService.getCurrentTrm();
        BigDecimal totalUsd = trm.compareTo(BigDecimal.ZERO) > 0
            ? totalCop.divide(trm, 2, RoundingMode.HALF_UP)
            : null;
        BigDecimal dailyIncrement = totalCop.divide(BigDecimal.valueOf(durationDays), 0, RoundingMode.HALF_UP);

        return new TravelPlanCalculationDto(
            tripType.name(),
            continent != null ? continent.name() : null,
            durationDays,
            totalUsd,
            totalCop,
            dailyIncrement,
            totalCop,
            trm,
            LocalDate.now(),
            Instant.now()
        );
    }
}
