using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PatientSupportManager
{
    public class PatientSupportService
    {

        public PatientSupportService() { }

        public string GeneratePatientCode(Patient patient)
        {
            if (string.IsNullOrWhiteSpace(patient.Name) || string.IsNullOrWhiteSpace(patient.LastName) || string.IsNullOrWhiteSpace(patient.CI))
                throw new ValidationException("Patient name, last name, and CI must not be empty.");

            // Extract initials from all parts of the first and last names.
            string initialsName = ExtractInitials(patient.Name);
            string initialsLastName = ExtractInitials(patient.LastName);

             // Construct the patient code by combining the initials with the CI
             string patientCode = $"{initialsName}{initialsLastName}-{patient.CI}";
             return patientCode.ToUpper(); // Convert to upper case for standardization
        }

        private string ExtractInitials(string name)
        {
            return new string(name.Split(' ')
                                  .Where(part => !string.IsNullOrWhiteSpace(part))
                                  .Select(part => part[0])
                                  .ToArray());
        }
    }
}
