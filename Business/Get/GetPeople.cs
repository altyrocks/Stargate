using MediatR;
using StargateAPI.Business.Common;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;

namespace StargateAPI.Business.Queries
{
    public class GetPeople : IRequest<GetPeopleResult>
    {

    }

    public class GetPeopleResult : BaseResponse
    {
        public List<Person>? Data { get; set; }
    }
}