using MediatR;
using StargateAPI.Business.Results;

namespace StargateAPI.Business.Create
{
    public class CreatePerson : IRequest<CreatePersonResult>
    {
        public required string Name { get; set; } = string.Empty;
    }
}