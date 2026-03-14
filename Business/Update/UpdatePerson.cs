using MediatR;
using StargateAPI.Business.Common;

namespace StargateAPI.Business.Update
{
    public class UpdatePerson : IRequest<BaseResponse>
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}