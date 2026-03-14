using MediatR;
using StargateAPI.Business.Results;

namespace StargateAPI.Business.Update
{
    public class UpsertPerson : IRequest<UpsertPersonResult>
    {
        public required string Name { get; set; }
    }
}