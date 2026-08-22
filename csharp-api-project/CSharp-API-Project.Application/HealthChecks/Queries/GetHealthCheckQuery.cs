
namespace Move4Purpose.Application.HealthChecks.Queries;

public class GetHealthCheckQueryResponse
{
	public string Status { get; set; }
	public DateTime StatusDate { get; set; }
}

public class GetHealthCheckQuery : IRequest<GetHealthCheckQueryResponse>
{
}

public class GetHealthCheckQueryHandler : IRequestHandler<GetHealthCheckQuery, GetHealthCheckQueryResponse>
{
	public async Task<GetHealthCheckQueryResponse> Handle(GetHealthCheckQuery request, CancellationToken cancellationToken)
	{
		return new GetHealthCheckQueryResponse
		{
			Status = "Healthy",
			StatusDate = DateTime.UtcNow
		};
	}
}