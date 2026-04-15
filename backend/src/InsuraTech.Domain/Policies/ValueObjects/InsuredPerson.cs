using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using InsuraTech.Domain.Common;
using InsuraTech.Domain.Policies.Enums;

namespace InsuraTech.Domain.Policies.ValueObjects
{
    public sealed class InsuredPerson : ValueObject
    {
        public string FirstName    { get; }
        public string LastName     { get; }
        public string DocumentType { get; }
        public string DocumentId   { get; }
        public DateOnly BirthDate  { get; }
        public string Gender       { get; }
        public string Address      { get; }
        public string CityName     { get; }
        public string PostalCode   { get; }
        public string Department   { get; }

        private InsuredPerson(
            string firstName, string lastName,
            string documentType, string documentId,
            DateOnly birthDate,
            string gender, string address,
            string cityName, string postalCode, string department)
        {
            FirstName    = firstName;
            LastName     = lastName;
            DocumentType = documentType;
            DocumentId   = documentId;
            BirthDate    = birthDate;
            Gender       = gender;
            Address      = address;
            CityName     = cityName;
            PostalCode   = postalCode;
            Department   = department;
        }

        public static InsuredPerson Create(
            string firstName, string lastName,
            string documentType, string documentId,
            DateOnly birthDate,
            string gender, string address,
            string cityName, string postalCode, string department)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name is required.", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name is required.", nameof(lastName));

            if (string.IsNullOrWhiteSpace(documentType))
                throw new ArgumentException("Document type is required.", nameof(documentType));

            if (!Enum.TryParse<DocumentType>(documentType.Trim(), ignoreCase: true, out var parsedDocType))
                throw new ArgumentException(
                    $"Document type '{documentType}' is not valid. Allowed values: CC, CE, TI, PP, RC.",
                    nameof(documentType));

            if (string.IsNullOrWhiteSpace(documentId))
                throw new ArgumentException("Document ID is required.", nameof(documentId));

            ValidateDocumentId(parsedDocType, documentId.Trim());

            if (birthDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ArgumentException("Birth date must be in the past.", nameof(birthDate));

            var age = CalculateAge(birthDate);
            if (age < 18 || age > 100)
                throw new ArgumentException("The insured person must be between 18 and 100 years old.", nameof(birthDate));

            if (string.IsNullOrWhiteSpace(gender) ||
                (gender.Trim() != "Masculino" && gender.Trim() != "Femenino"))
                throw new ArgumentException("Gender must be 'Masculino' or 'Femenino'.", nameof(gender));

            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Address is required.", nameof(address));

            if (address.Trim().Length > 200)
                throw new ArgumentException("Address must not exceed 200 characters.", nameof(address));

            if (string.IsNullOrWhiteSpace(cityName))
                throw new ArgumentException("City name is required.", nameof(cityName));

            if (string.IsNullOrWhiteSpace(postalCode))
                throw new ArgumentException("Postal code is required.", nameof(postalCode));

            if (string.IsNullOrWhiteSpace(department))
                throw new ArgumentException("Department is required.", nameof(department));

            return new InsuredPerson(
                firstName.Trim(), lastName.Trim(),
                documentType.Trim(), documentId.Trim(),
                birthDate,
                gender.Trim(), address.Trim(),
                cityName.Trim(), postalCode.Trim(), department.Trim());
        }

        /// <summary>
        /// Reconstruye un InsuredPerson desde persistencia sin ejecutar validaciones de negocio.
        /// Usar SOLO para deserialización (MongoDB). Para nuevas entidades usar Create().
        /// </summary>
        public static InsuredPerson Reconstitute(
            string firstName, string lastName,
            string documentType, string documentId,
            DateOnly birthDate,
            string gender, string address,
            string cityName, string postalCode, string department)
        {
            return new InsuredPerson(
                firstName    ?? string.Empty,
                lastName     ?? string.Empty,
                documentType ?? string.Empty,
                documentId   ?? string.Empty,
                birthDate,
                gender       ?? string.Empty,
                address      ?? string.Empty,
                cityName     ?? string.Empty,
                postalCode   ?? string.Empty,
                department   ?? string.Empty);
        }

        public int Age => CalculateAge(BirthDate);

        private static void ValidateDocumentId(DocumentType docType, string documentId)
        {
            bool valid = docType switch
            {
                Enums.DocumentType.CC or
                Enums.DocumentType.TI or
                Enums.DocumentType.RC => Regex.IsMatch(documentId, @"^\d{1,10}$"),
                Enums.DocumentType.CE or
                Enums.DocumentType.PP => Regex.IsMatch(documentId, @"^[A-Za-z0-9]{1,11}$"),
                _ => false
            };

            if (!valid)
                throw new ArgumentException(
                    $"Document ID format is invalid for document type '{docType}'.",
                    nameof(documentId));
        }

        private static int CalculateAge(DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age   = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;
            return age;
        }

        protected override IEnumerable<object> GetEqualityComponets()
        {
            yield return DocumentId;
        }

        public override string ToString() => $"{FirstName} {LastName} {DocumentType} ({DocumentId})";
    }
}
