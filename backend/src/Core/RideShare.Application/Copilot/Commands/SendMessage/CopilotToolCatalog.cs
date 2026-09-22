using RideShare.Application.Common.Interfaces;

namespace RideShare.Application.Copilot.Commands.SendMessage;

/// <summary>
/// The set of MediatR commands/queries the copilot is allowed to trigger on a user's behalf,
/// described as tool/function definitions for the underlying LLM. Executing a tool call always
/// runs through the same command handlers (and their FluentValidation validators) that a normal
/// controller call would — the copilot cannot bypass a single business rule.
/// </summary>
public static class CopilotToolCatalog
{
    public const string RequestRide = "request_ride";
    public const string CancelActiveRide = "cancel_active_ride";
    public const string GetActiveRideStatus = "get_active_ride_status";
    public const string GoOnline = "go_online";
    public const string GoOffline = "go_offline";

    public static IReadOnlyList<CopilotToolDefinition> ForRole(string role)
    {
        var riderTools = new[]
        {
            new CopilotToolDefinition(
                RequestRide,
                "Request a new ride for the rider. Only call this once you have both a pickup and a dropoff location.",
                """
                {
                  "type": "object",
                  "properties": {
                    "pickupLatitude": { "type": "number" },
                    "pickupLongitude": { "type": "number" },
                    "pickupAddress": { "type": "string" },
                    "dropoffLatitude": { "type": "number" },
                    "dropoffLongitude": { "type": "number" },
                    "dropoffAddress": { "type": "string" },
                    "vehicleType": { "type": "string", "enum": ["Economy", "Comfort", "Xl", "Premium"] }
                  },
                  "required": ["pickupLatitude", "pickupLongitude", "dropoffLatitude", "dropoffLongitude", "vehicleType"]
                }
                """),
            new CopilotToolDefinition(
                CancelActiveRide,
                "Cancel the rider's current active ride.",
                """{ "type": "object", "properties": { "reason": { "type": "string" } }, "required": ["reason"] }"""),
            new CopilotToolDefinition(
                GetActiveRideStatus,
                "Look up the status of the rider's current active ride (driver assigned, ETA, in progress, etc).",
                """{ "type": "object", "properties": {} }""")
        };

        var driverTools = new[]
        {
            new CopilotToolDefinition(GoOnline, "Set the driver's status to online so they start receiving ride offers.",
                """{ "type": "object", "properties": {} }"""),
            new CopilotToolDefinition(GoOffline, "Set the driver's status to offline so they stop receiving ride offers.",
                """{ "type": "object", "properties": {} }"""),
            new CopilotToolDefinition(
                GetActiveRideStatus,
                "Look up the status of the driver's current active ride.",
                """{ "type": "object", "properties": {} }""")
        };

        return role == "Driver" ? driverTools : riderTools;
    }
}
