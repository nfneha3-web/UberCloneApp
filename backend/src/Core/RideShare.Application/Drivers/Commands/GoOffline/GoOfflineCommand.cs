using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;

namespace RideShare.Application.Drivers.Commands.GoOffline;

public sealed record GoOfflineCommand(Guid DriverApplicationUserId) : IRequest;

public class GoOfflineCommandHandler(IDriverProfileRepository driverProfileRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<GoOfflineCommand>
{
    public async Task Handle(GoOfflineCommand request, CancellationToken cancellationToken)
    {
        var driverProfile = await driverProfileRepository.GetByApplicationUserIdAsync(request.DriverApplicationUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(DriverProfile), request.DriverApplicationUserId);

        driverProfile.GoOffline();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
