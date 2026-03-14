using MediatR;
using StargateAPI.Business.Results;

namespace StargateAPI.Business.Update
{
    public class UpdateAstronautDuty : IRequest<UpdateAstronautDutyResult>
    {
        public int Id { get; set; }
        public string Rank { get; set; } = string.Empty;
        public string DutyTitle { get; set; } = string.Empty;
        public DateTime DutyStartDate { get; set; }
    }
}