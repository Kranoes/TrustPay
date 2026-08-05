namespace TrustPay.Domain.Common
{
    public record Error(string Code, string Description, ErrorType Type)
    {
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

        public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
        public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
        public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
        public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);
        public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);
        public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);

        public static implicit operator Error(string description) =>
            string.IsNullOrWhiteSpace(description) ? None : Failure("General.Error", description);

        public static implicit operator string(Error error) => error?.Description ?? string.Empty;
    }
}