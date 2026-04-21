// Origen: DependencyInjection.cs (Infrastructure) → InfrastructureConfig.java
package com.insuratech.infrastructure.config;

import org.springframework.cache.annotation.EnableCaching;
import org.springframework.context.annotation.Configuration;
import org.springframework.data.mongodb.config.EnableMongoAuditing;
import org.springframework.data.mongodb.repository.config.EnableMongoRepositories;

@Configuration
@EnableCaching
@EnableMongoAuditing
@EnableMongoRepositories(basePackages = "com.insuratech.infrastructure.persistence.springdata")
public class InfrastructureConfig {
}
