using MediatR;
using System.Net;
using StargateAPI.Business.Services;
using StargateAPI.Business.Results;
using StargateAPI.Business.Update;

namespace StargateAPI.Business.Handlers
{
    public class UpdateAstronautDutyHandler : IRequestHandler<UpdateAstronautDuty, UpdateAstronautDutyResult>
    {
        private readonly IAstronautDutyDomainService _domainService;
        private readonly ILogService _logService;

        public UpdateAstronautDutyHandler(
            IAstronautDutyDomainService domainService,
            ILogService logService)
        {
            _domainService = domainService;
            _logService = logService;
        }

        public async Task<UpdateAstronautDutyResult> Handle(
            UpdateAstronautDuty request,
            CancellationToken cancellationToken)
        {
            var result = new UpdateAstronautDutyResult();

            try
            {
                await _domainService.UpdateDutyAsync(
                    request.Id,
                    request.Rank,
                    request.DutyTitle,
                    request.DutyStartDate,
                    cancellationToken);

                result.Success = true;
                result.ResponseCode = (int)HttpStatusCode.OK;
                result.Id = request.Id;
                result.Message = "Duty updated successfully.";

                await _logService.InfoAsync(
                    nameof(UpdateAstronautDutyHandler),
                    $"Updated duty {request.Id}",
                    null,
                    cancellationToken);

                return result;
            }
            catch (InvalidOperationException ex)
            {
                result.Success = false;
                result.ResponseCode = (int)HttpStatusCode.BadRequest;
                result.Message = ex.Message;

                return result;
            }
        }
    }
}