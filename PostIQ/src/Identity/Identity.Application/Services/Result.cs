namespace PostIQ.Identity.Services
{
    public sealed class Result<T>
    {
        public bool Ok { get; set; }
        public T? Value { get; set; }
        public string? Error { get; set; }
        public int Status { get; set; }

        public static Result<T> Success(T value, int status = 200) => new() { Ok = true, Value = value, Status = status };
        public static Result<T> Failure(int status, string error) => new() { Ok = false, Error = error, Status = status };
    }
}
