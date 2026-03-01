using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Services;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<Response<AstronautDutyDto>>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler
        : IRequestHandler<GetAstronautDutiesByName, Response<AstronautDutyDto>>
    {
        private readonly StargateContext _context;
        private readonly ILogService _logService;

        public GetAstronautDutiesByNameHandler(StargateContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<Response<AstronautDutyDto>> Handle(
            GetAstronautDutiesByName request,
            CancellationToken cancellationToken)
        {
            var result = new Response<AstronautDutyDto>();

            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    result.Success = false;
                    result.Message = "Name is required.";
                    result.ResponseCode = (int)HttpStatusCode.BadRequest;
                    return result;
                }

                var name = request.Name.Trim().ToLower();

                const string personSql = @"SELECT a.Id AS PersonId, a.Name, b.CurrentRank, 
                                           b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate
                                           FROM [Person] a
                                           LEFT JOIN [AstronautDetail] b ON b.PersonId = a.Id
                                           WHERE LOWER(a.Name) = @Name;";

                var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronautDto>(
                    new CommandDefinition(
                        personSql,
                        new { Name = name },
                        cancellationToken: cancellationToken));

                if (person == null)
                {
                    result.Success = false;
                    result.Message = $"No person found with name '{name}'.";
                    result.ResponseCode = (int)HttpStatusCode.NotFound;

                    await _logService.InfoAsync(
                        nameof(GetAstronautDutiesByNameHandler),
                        $"No person found with name '{name}'.",
                        null,
                        cancellationToken);

                    return result;
                }

                const string dutySql = @"SELECT * FROM [AstronautDuty]
                                         WHERE PersonId = @PersonId
                                         ORDER BY DutyStartDate DESC;";

                var duties = await _context.Connection.QueryAsync<AstronautDuty>(
                    new CommandDefinition(
                        dutySql,
                        parameters: new { person.PersonId },
                        cancellationToken: cancellationToken));

                var latestDuty = duties.FirstOrDefault();

                if (latestDuty == null)
                {
                    result.Success = false;
                    result.Message = $"No duty records found for '{name}'.";
                    result.ResponseCode = (int)HttpStatusCode.NotFound;

                    await _logService.InfoAsync(
                        nameof(GetAstronautDutiesByNameHandler),
                        $"No duty records found for '{name}'.",
                        null,
                        cancellationToken);

                    return result;
                }

                // Map to DTO
                result.Data = new AstronautDutyDto
                {
                    Name = person.Name,
                    Assignment = latestDuty.DutyTitle ?? person.CurrentDutyTitle ?? string.Empty,
                    Rank = latestDuty.Rank ?? person.CurrentRank ?? string.Empty,
                    LastUpdated = latestDuty.DutyStartDate
                };

                result.Success = true;
                result.Message = "Astronaut duties retrieved successfully.";
                result.ResponseCode = (int)HttpStatusCode.OK;

                await _logService.InfoAsync(
                    nameof(GetAstronautDutiesByNameHandler),
                    $"Retrieved {duties.Count()} duties for '{name}'. Latest duty mapped to DTO.",
                    null,
                    cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "An error occurred while retrieving astronaut duties.";
                result.ResponseCode = (int)HttpStatusCode.InternalServerError;

                await _logService.ErrorAsync(
                    nameof(GetAstronautDutiesByNameHandler),
                    "Error in GetAstronautDutiesByName.",
                    ex.ToString(),
                    cancellationToken);

                return result;
            }
        }
    }
}

public class GetAstronautDutiesByNameResult : BaseResponse
{
    public PersonAstronautDto? Person { get; set; }
    public List<AstronautDuty> AstronautDuties { get; set; } = new List<AstronautDuty>();
}