using BootManager.Core.Entities;

namespace BootManager.UnitTests.Logbook;

public class LogbookTripTests
{
    [Fact]
    public void Constructor_CalculatesLoggedMiles_FromStartAndEnd()
    {
        var trip = new LogbookTrip(
            name: "Testreis",
            departureUtc: DateTime.UtcNow,
            logstandStart: 500m,
            logstandEnd: 510m);

        Assert.Equal(510m, trip.LogstandEnd);
        Assert.Equal(10m, trip.LoggedMiles);
    }

    [Fact]
    public void Update_RecalculatesLoggedMiles_FromStartAndEnd()
    {
        var trip = new LogbookTrip(
            name: "Testreis",
            departureUtc: DateTime.UtcNow,
            logstandStart: 500m,
            logstandEnd: 510m);

        trip.Update(
            name: "Testreis",
            departureUtc: trip.DepartureUtc,
            arrivalUtc: null,
            departurePort: null,
            destinationPort: null,
            vesselName: null,
            crew: null,
            notes: null,
            logstandStart: 510m,
            logstandEnd: 525.5m,
            engineHoursStart: null,
            engineHoursEnd: null,
            fuel: null,
            totalSailingHours: null);

        Assert.Equal(15.5m, trip.LoggedMiles);
    }

    [Fact]
    public void Constructor_RejectsEndBelowStart()
    {
        var exception = Assert.Throws<ArgumentException>(() => new LogbookTrip(
            name: "Testreis",
            departureUtc: DateTime.UtcNow,
            logstandStart: 510m,
            logstandEnd: 500m));

        Assert.Contains("Logstand eind", exception.Message);
    }
}
