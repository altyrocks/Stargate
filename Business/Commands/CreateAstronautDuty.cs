using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Services;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is null) throw new BadHttpRequestException($"Person '{request.Name}' not found.");

            var verifyNoPreviousDuty = _context.AstronautDuties.
                FirstOrDefault(z => z.DutyTitle == request.DutyTitle && z.DutyStartDate == request.DutyStartDate);

            if (verifyNoPreviousDuty is not null)
                throw new BadHttpRequestException($"Duty '{request.DutyTitle}' already exists with start date {request.DutyStartDate:yyyy-MM-dd}.");

            return Task.CompletedTask;
        }
    }

    public class CreateAstronautDutyHandler
        : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;
        private readonly ILogService _logService;

        public CreateAstronautDutyHandler(StargateContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<CreateAstronautDutyResult> Handle(
            CreateAstronautDuty request,
            CancellationToken cancellationToken)
        {
            var result = new CreateAstronautDutyResult();

            try
            {
                // extra safety in case model validation or pre-processor misses something
                if (string.IsNullOrWhiteSpace(request.Name) ||
                    string.IsNullOrWhiteSpace(request.Rank) ||
                    string.IsNullOrWhiteSpace(request.DutyTitle))
                {
                    result.Success = false;
                    result.Message = "Name, Rank, and DutyTitle are required.";
                    result.ResponseCode = (int)HttpStatusCode.BadRequest;
                    return result;
                }

                var name = request.Name.Trim();
                var dutyStart = request.DutyStartDate.Date;

                // get person by name (parameterized)
                const string personSql = @"SELECT * FROM [Person]
                                           WHERE Name = @Name;";

                var person = await _context.Connection.QueryFirstOrDefaultAsync<Person>(
                    new CommandDefinition(
                        personSql,
                        new { Name = name },
                        cancellationToken: cancellationToken));

                if (person == null)
                {
                    result.Success = false;
                    result.Message = $"Person '{name}' not found.";
                    result.ResponseCode = (int)HttpStatusCode.NotFound;

                    await _logService.InfoAsync(
                        nameof(CreateAstronautDutyHandler),
                        $"Attempt to add duty for unknown person '{name}'.",
                        null,
                        cancellationToken);

                    return result;
                }

                // get AstronautDetail (if any)
                const string detailSql = @"SELECT * FROM [AstronautDetail]
                                           WHERE PersonId = @PersonId;";

                var astronautDetail = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDetail>(
                    new CommandDefinition(
                        detailSql,
                        new { PersonId = person.Id },
                        cancellationToken: cancellationToken));

                // create or update AstronautDetail (career info)
                if (astronautDetail == null)
                {
                    astronautDetail = new AstronautDetail
                    {
                        PersonId = person.Id,
                        CurrentDutyTitle = request.DutyTitle.Trim(),
                        CurrentRank = request.Rank.Trim(),
                        CareerStartDate = dutyStart
                    };

                    if (request.DutyTitle.Equals("RETIRED", StringComparison.OrdinalIgnoreCase))
                    {
                        // career end date is one day before retired duty start
                        astronautDetail.CareerEndDate = dutyStart.AddDays(-1);
                    }

                    await _context.AstronautDetails.AddAsync(astronautDetail, cancellationToken);
                }
                else
                {
                    astronautDetail.CurrentDutyTitle = request.DutyTitle.Trim();
                    astronautDetail.CurrentRank = request.Rank.Trim();

                    if (request.DutyTitle.Equals("RETIRED", StringComparison.OrdinalIgnoreCase))
                    {
                        astronautDetail.CareerEndDate = dutyStart.AddDays(-1);
                    }

                    _context.AstronautDetails.Update(astronautDetail);
                }

                // get last AstronautDuty (most recent) for this person
                const string dutySql = @"SELECT * FROM [AstronautDuty]
                                         WHERE PersonId = @PersonId              
                                         ORDER BY DutyStartDate DESC
                                         LIMIT 1;";

                var lastDuty = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDuty>(
                    new CommandDefinition(
                        dutySql,
                        new { PersonId = person.Id },
                        cancellationToken: cancellationToken));

                // enforce: previous duty end date = day before new start (if previous duty exists)
                if (lastDuty != null)
                {
                    // Optional chronological check
                    if (dutyStart <= lastDuty.DutyStartDate.Date)
                    {
                        result.Success = false;
                        result.Message = "New duty start date must be after last duty start date.";
                        result.ResponseCode = (int)HttpStatusCode.BadRequest;
                        return result;
                    }

                    lastDuty.DutyEndDate = dutyStart.AddDays(-1);
                    _context.AstronautDuties.Update(lastDuty);
                }

                // create new current AstronautDuty (EndDate = null)
                var newAstronautDuty = new AstronautDuty
                {
                    PersonId = person.Id,
                    Rank = request.Rank.Trim(),
                    DutyTitle = request.DutyTitle.Trim(),
                    DutyStartDate = dutyStart,
                    DutyEndDate = null
                };

                await _context.AstronautDuties.AddAsync(newAstronautDuty, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                result.Id = newAstronautDuty.Id;
                result.Success = true;
                result.Message = "Astronaut duty created successfully.";
                result.ResponseCode = (int)HttpStatusCode.Created;

                await _logService.InfoAsync(
                    nameof(CreateAstronautDutyHandler),
                    $"Created new duty '{newAstronautDuty.DutyTitle}' for '{name}' starting {dutyStart:yyyy-MM-dd}.",
                    null,
                    cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "An error occurred while creating the astronaut duty.";
                result.ResponseCode = (int)HttpStatusCode.InternalServerError;

                await _logService.ErrorAsync(
                    nameof(CreateAstronautDutyHandler),
                    "Error in CreateAstronautDuty.",
                    ex.ToString(),
                    cancellationToken);

                return result;
            }
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}