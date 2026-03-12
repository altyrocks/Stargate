using StargateAPI.Business.Data;
using StargateAPI.Business.Common;

namespace StargateAPI.Business.Results
{
    public class GetPeopleResult : BaseResponse
    {
        public List<Person>? Data { get; set; }
    }
}