package com.insuratech.domain.policies.vo;

import com.insuratech.domain.common.ValueObject;
import com.insuratech.domain.exceptions.InvalidInsuredPersonException;
import com.insuratech.domain.policies.enums.DocumentType;

import java.time.LocalDate;

public final class InsuredPerson extends ValueObject {

    private final String firstName;
    private final String lastName;
    private final DocumentType documentType;
    private final String documentNumber;
    private final LocalDate birthDate;
    private final String email;
    private final String phone;

    private InsuredPerson(String firstName, String lastName, DocumentType documentType,
                          String documentNumber, LocalDate birthDate, String email, String phone) {
        this.firstName = firstName;
        this.lastName = lastName;
        this.documentType = documentType;
        this.documentNumber = documentNumber;
        this.birthDate = birthDate;
        this.email = email;
        this.phone = phone;
    }

    public static InsuredPerson of(String firstName, String lastName, DocumentType documentType,
                                   String documentNumber, LocalDate birthDate, String email, String phone) {
        validate(firstName, lastName, documentType, documentNumber, birthDate, email);
        return new InsuredPerson(firstName, lastName, documentType, documentNumber, birthDate, email, phone);
    }

    private static void validate(String firstName, String lastName, DocumentType documentType,
                                  String documentNumber, LocalDate birthDate, String email) {
        if (firstName == null || firstName.isBlank())
            throw new InvalidInsuredPersonException("first name is required");
        if (lastName == null || lastName.isBlank())
            throw new InvalidInsuredPersonException("last name is required");
        if (documentType == null)
            throw new InvalidInsuredPersonException("document type is required");
        if (documentNumber == null || documentNumber.isBlank())
            throw new InvalidInsuredPersonException("document number is required");
        if (birthDate == null || birthDate.isAfter(LocalDate.now()))
            throw new InvalidInsuredPersonException("birth date must be in the past");
        if (email == null || !email.contains("@"))
            throw new InvalidInsuredPersonException("valid email is required");
    }

    public int getAge() {
        return LocalDate.now().getYear() - birthDate.getYear();
    }

    public String getFirstName()     { return firstName; }
    public String getLastName()      { return lastName; }
    public DocumentType getDocumentType() { return documentType; }
    public String getDocumentNumber() { return documentNumber; }
    public LocalDate getBirthDate()  { return birthDate; }
    public String getEmail()         { return email; }
    public String getPhone()         { return phone; }
    public String getFullName()      { return firstName + " " + lastName; }

    @Override
    protected Object[] getEqualityComponents() {
        return new Object[]{documentType, documentNumber};
    }
}
