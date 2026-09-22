using Microsoft.EntityFrameworkCore;
using RideShare.Application.Common.Interfaces;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence.Repositories;

public class PaymentRepository(RideShareDbContext dbContext) : IPaymentRepository
{
    public Task<Payment?> GetByRideIdAsync(Guid rideId, CancellationToken cancellationToken = default) =>
        dbContext.Payments.FirstOrDefaultAsync(x => x.RideId == rideId, cancellationToken);

    public Task<Payment?> GetByStripePaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default) =>
        dbContext.Payments.FirstOrDefaultAsync(x => x.StripePaymentIntentId == paymentIntentId, cancellationToken);

    public void Add(Payment payment) => dbContext.Payments.Add(payment);
}
