using StargateAPI.Business.Common;
using StargateAPI.Business.Data;

namespace StargateAPI.Business.Results
{
    public class GetPeopleResult : BaseResponse
    {
        public List<Person>? Data { get; set; }
    }
}