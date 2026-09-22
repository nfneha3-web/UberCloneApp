using MediatR;
using RideShare.Application.Common.Exceptions;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;

namespace RideShare.Application.Rides.Commands.CompleteRide;

public class CompleteRideCommandHandler(
    IRideRepository rideRepository,
    IDriverProfileRepository driverProfileRepository,
    IRiderProfileRepository riderProfileRepository,
    IPaymentRepository paymentRepository,
    IRideQueryService rideQueryService,
    IRideRealtimeNotifier realtimeNotifier,
    IPaymentService paymentService,
    IUnitOfWork unitOfWork) : IRequestHandler<CompleteRideCommand, CompleteRideResult>
{
    public async Task<CompleteRideResult> Handle(CompleteRideCommand request, CancellationToken cancellationToken)
    {
        var driverProfile = await driverProfileRepository.GetByApplicationUserIdAsync(request.DriverApplicationUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(DriverProfile), request.DriverApplicationUserId);

        var ride = await rideRepository.GetByIdAsync(request.RideId, cancellationToken)
            ?? throw new NotFoundException(nameof(Ride), request.RideId);

        if (ride.DriverProfileId != driverProfile.Id)
            throw new ForbiddenAccessException("Only the assigned driver can complete this ride.");

        var riderProfile = await riderProfileRepository.GetByIdAsync(ride.RiderProfileId, cancellationToken)
            ?? throw new NotFoundException(nameof(RiderProfile), ride.RiderProfileId);

        // Phase 1 simplification: the final fare equals the upfront estimate (no live meter
        // tracking actual distance/time driven yet — see docs/ROADMAP.md Phase 2).
        var finalFare = ride.EstimatedFare;

        ride.Complete(finalFare);
        driverProfile.MarkAvailableAfterTrip();

        var paymentIntent = await paymentService.CreatePaymentIntentAsync(finalFare, riderProfile.StripeCustomerId, cancellationToken);
        var payment = new Payment(ride.Id, finalFare, paymentIntent.PaymentIntentId, paymentIntent.ClientSecret);
        paymentRepository.Add(payment);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var rideDetails = await rideQueryService.GetByIdAsync(ride.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Ride), ride.Id);

        await realtimeNotifier.NotifyRideStatusChangedAsync(rideDetails, cancellationToken);

        return new CompleteRideResult(rideDetails, paymentIntent.PaymentIntentId, paymentIntent.ClientSecret);
    }
}
