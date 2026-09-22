using RideShare.Application.Rides.Commands.RequestRide;
using RideShare.Domain.Enums;

namespace RideShare.Application.Tests;

public class RequestRideCommandValidatorTests
{
    private readonly RequestRideCommandValidator _validator = new();

    private static RequestRideCommand ValidCommand() => new(
        RiderApplicationUserId: Guid.NewGuid(),
        PickupLatitude: 40.7128,
        PickupLongitude: -74.0060,
        PickupAddress: "123 Main St",
        DropoffLatitude: 40.7300,
        DropoffLongitude: -73.9950,
        DropoffAddress: "456 Elm St",
        VehicleType: VehicleType.Economy);

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void EmptyRiderId_FailsValidation()
    {
        var command = ValidCommand() with { RiderApplicationUserId = Guid.Empty };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RequestRideCommand.RiderApplicationUserId));
    }

    [Theory]
    [InlineData(91, 0)]
    [InlineData(-91, 0)]
    [InlineData(0, 181)]
    [InlineData(0, -181)]
    public void OutOfRangePickupCoordinates_FailValidation(double lat, double lng)
    {
        var command = ValidCommand() with { PickupLatitude = lat, PickupLongitude = lng };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void SamePickupAndDropoff_FailsValidation()
    {
        var command = ValidCommand() with { DropoffLatitude = 40.7128, DropoffLongitude = -74.0060 };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
