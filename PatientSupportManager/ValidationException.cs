using System;

namespace PatientSupportManager
{
    public class ValidationException : Exception 
    {
        public ValidationException(string message) : base(message) {}
    }
}