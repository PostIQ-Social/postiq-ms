namespace PostIQ.Core.Response
{
    public interface ICreatedResponse
    {
        public string Id { get; set; }
    }

    public interface ICreatedResponse<out TModel> : ICreatedResponse where TModel : class
    {
        TModel Data { get; }
    }
}
