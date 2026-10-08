
using System.Diagnostics.CodeAnalysis;


namespace HostAnnotation.Common {

    public class Result {

        public bool isSuccess { get; }

        public bool isFailure => !isSuccess;

        public Error? error { get; }


        // C-tor
        public Result(bool isSuccess_, Error? error_ = null) {

            if (isSuccess_ && error_ != null) {
                throw new ArgumentException("A successful result should not include an error parameter", nameof(error_));
            } else if (!isSuccess_ && error_ == null) {
                throw new ArgumentException("A failure result must include an error");
            }

            isSuccess = isSuccess_;
            error = error_;
        }


        // The error message returned is guaranteed to be a non-null string.
        public string getErrorMessage(string? defaultMessage_ = null) {
            if (isSuccess) {
                return string.Empty;
            } else if (error == null || string.IsNullOrEmpty(error.message)) {
                return defaultMessage_ ?? "An unknown error occurred";
            } else {
                return error.message;
            }
        }

        // Static factory methods

        public static Result failure(string errorMessage_, string? location_ = null) {
            return new Result(false, new Error(errorMessage_, location_));
        }

        public static Result success() => new(true);
    }


    public class Result<TValue> : Result {

        private readonly TValue? _value;

        // C-tor
        public Result(TValue? value_, bool isSuccess_, Error? error_ = null)
            : base(isSuccess_, error_) {
            _value = value_;
        }

        public static Result<TValue> success(TValue value_) {
            return new Result<TValue>(value_, true);
        }

        // Override the base method
        public new static Result<TValue> failure(string message_, string? location_ = null) {
            if (message_.Length < 1) { message_ = "An unknown error occurred"; }
            return new Result<TValue>(default, false, new Error(message_, location_));
        }

        // The value property
        [NotNull]
        public TValue value => isSuccess
            ? _value!
            : throw new InvalidOperationException("The value of a failure result can't be accessed.");

        /*public static implicit operator Result<TValue>(TValue? value) =>
            value is not null ? success(value) : Failure<TValue>(Error_notUsed.NullValue);

        public static Result<TValue> ValidationFailure(Error_notUsed error) =>
            new(default, false, error);*/
    }
}

