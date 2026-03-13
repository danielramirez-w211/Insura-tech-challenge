using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InsuraTech.Domain.Common;

namespace InsuraTech.Domain.Policies.ValueObjects
{
    public sealed class InsuredPerson : ValueObject
    {
        public string FullName { get; }
        public string DocumentId { get; }
        public DateOnly BirthDate { get; }

        private InsuredPerson(string fullName, string documentId, DateOnly birthDate)
        {
            FullName = fullName;
            DocumentId = documentId;
            BirthDate = birthDate;
        }

        public static InsuredPerson Create(string fullName, string documentId, DateOnly birthDate)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name is requerid ", nameof(fullName));

            if (string.IsNullOrWhiteSpace(documentId))
                throw new ArgumentException("Document ID is requiered ", nameof(documentId));

            if (birthDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ArgumentException("Birth date must be in the past. ", nameof(birthDate));

            var age = CalculateAge(birthDate);
            if (age < 18 || age > 100)
                throw new ArgumentException("The Insued person must be 18 and 100 years. ", nameof(birthDate));

            return new InsuredPerson(fullName.Trim(), documentId.Trim(), birthDate);

        }

        public int Age => CalculateAge(BirthDate);

        private static int CalculateAge(DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age)) age--;
            return age;
        }

        protected override IEnumerable<object> GetEqualityComponets()
        {
            yield return DocumentId;
        }

        public override string ToString() => $"{FullName} ({DocumentId})";

     }
}
