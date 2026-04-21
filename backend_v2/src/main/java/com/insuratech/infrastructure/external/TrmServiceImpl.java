// Origen: TrmService.cs → TrmServiceImpl.java
package com.insuratech.infrastructure.external;

import com.insuratech.application.common.interfaces.ITrmService;
import lombok.extern.slf4j.Slf4j;
import org.springframework.cache.annotation.Cacheable;
import org.springframework.stereotype.Service;
import org.springframework.web.client.RestClient;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.util.List;
import java.util.Map;

@Slf4j
@Service
public class TrmServiceImpl implements ITrmService {

    private static final String TRM_API_URL =
        "https://www.datos.gov.co/resource/32sa-8pi3.json?vigenciahasta=%s";

    private final RestClient restClient;

    public TrmServiceImpl() {
        this.restClient = RestClient.create();
    }

    @Override
    @Cacheable("trm-current")
    public BigDecimal getCurrentTrm() {
        return getTrmForDate(LocalDate.now());
    }

    @Override
    @Cacheable("trm-date")
    public BigDecimal getTrmForDate(LocalDate date) {
        try {
            var url = String.format(TRM_API_URL, date.toString());
            var response = restClient.get().uri(url)
                .retrieve()
                .body(List.class);

            if (response != null && !response.isEmpty()) {
                var first = (Map<?, ?>) response.get(0);
                return new BigDecimal(first.get("valor").toString());
            }
        } catch (Exception e) {
            log.error("Failed to fetch TRM for date {}: {}", date, e.getMessage());
        }
        return new BigDecimal("4200");
    }
}
