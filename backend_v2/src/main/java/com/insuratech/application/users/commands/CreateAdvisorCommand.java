package com.insuratech.application.users.commands;
import jakarta.validation.constraints.Email;
import jakarta.validation.constraints.NotBlank;
public record CreateAdvisorCommand(
    @NotBlank @Email String email, @NotBlank String password,
    @NotBlank String firstName, @NotBlank String lastName,
    String phone, String city, String createdByLeaderId
) {}
