using StargateAPI.Controllers;

namespace StargateAPI.Business.Dtos
{
    public class Response<T> : BaseResponse
    {
        public T? Data { get; set; }
    }
}