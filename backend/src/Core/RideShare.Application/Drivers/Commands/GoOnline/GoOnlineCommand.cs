using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;

namespace RideShare.Application.Drivers.Commands.GoOnline;

public sealed record GoOnlineCommand(Guid DriverApplicationUserId) : IRequest;

public class GoOnlineCommandHandler(IDriverProfileRepository driverProfileRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<GoOnlineCommand>
{
    public async Task Handle(GoOnlineCommand request, CancellationToken cancellationToken)
    {
        var driverProfile = await driverProfileRepository.GetByApplicationUserIdAsync(request.DriverApplicationUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(DriverProfile), request.DriverApplicationUserId);

        driverProfile.GoOnline();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
