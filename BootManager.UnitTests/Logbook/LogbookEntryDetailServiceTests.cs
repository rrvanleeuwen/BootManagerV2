using System.Linq.Expressions;
using BootManager.Application.Logbook.Services;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;
using Moq;

namespace BootManager.UnitTests.Logbook;

/// <summary>
/// Tests voor <see cref="LogbookEntryDetailService"/> rond het terugmappen van de opgeslagen
/// gebeurtenis, weerconditie en notitie (Remarks) naar de detail-DTO.
/// </summary>
public class LogbookEntryDetailServiceTests
{
    private readonly Mock<IRepository<LogbookEntry>> _entryRepo = new();
    private readonly Mock<IRepository<LogbookTrip>> _tripRepo = new();
    private readonly Mock<IRepository<PositionMeasurement>> _positionRepo = new();
    private readonly Mock<IRepository<MotionMeasurement>> _motionRepo = new();
    private readonly Mock<IRepository<HeadingMeasurement>> _headingRepo = new();
    private readonly Mock<IRepository<WindMeasurement>> _windRepo = new();
    private readonly Mock<IRepository<DepthMeasurement>> _depthRepo = new();
    private readonly Mock<IRepository<WaterTemperatureMeasurement>> _waterTempRepo = new();

    private LogbookEntryDetailService CreateSut() => new(
        _entryRepo.Object,
        _tripRepo.Object,
        _positionRepo.Object,
        _motionRepo.Object,
        _headingRepo.Object,
        _windRepo.Object,
        _depthRepo.Object,
        _waterTempRepo.Object);

    private static void SetEntryId(LogbookEntry entry, int id) =>
        typeof(LogbookEntry).GetProperty(nameof(LogbookEntry.Id))!.SetValue(entry, id);

    private static void SetTripId(LogbookTrip trip, int id) =>
        typeof(LogbookTrip).GetProperty(nameof(LogbookTrip.Id))!.SetValue(trip, id);

    [Fact]
    public async Task GetEntryDetailAsync_MapsSavedEventWeatherAndNote_BackToDetailDto()
    {
        var entry = new LogbookEntry(
            logbookTripId: 8,
            entryTimeUtc: new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc),
            remarks: "Overstag bij de ton",
            eventType: LogbookEventType.Overstag,
            weatherCondition: LogbookWeatherCondition.HalfBewolkt);
        SetEntryId(entry, 55);

        var trip = new LogbookTrip(name: "Testreis", departureUtc: new DateTime(2026, 7, 16, 8, 0, 0, DateTimeKind.Utc));
        SetTripId(trip, 8);

        _entryRepo
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<LogbookEntry, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);
        _tripRepo
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<LogbookTrip, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        // Geen eerdere regel: BepaalPeriodStart valt terug op vertrek; meetrepo's leveren niets.
        _entryRepo
            .Setup(r => r.ListAsync(It.IsAny<Expression<Func<LogbookEntry, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LogbookEntry>());
        _positionRepo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<PositionMeasurement, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PositionMeasurement>());
        _motionRepo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<MotionMeasurement, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<MotionMeasurement>());
        _headingRepo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<HeadingMeasurement, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<HeadingMeasurement>());
        _windRepo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<WindMeasurement, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<WindMeasurement>());
        _depthRepo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<DepthMeasurement, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<DepthMeasurement>());
        _waterTempRepo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<WaterTemperatureMeasurement, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<WaterTemperatureMeasurement>());

        var sut = CreateSut();

        var detail = await sut.GetEntryDetailAsync(55);

        Assert.NotNull(detail);
        Assert.NotNull(detail!.SavedValues);
        // De stabiele gebeurtenis- en weerwaarden komen ongewijzigd terug in de detail-DTO.
        Assert.Equal(LogbookEventType.Overstag, detail.SavedValues!.EventType);
        Assert.Equal(LogbookWeatherCondition.HalfBewolkt, detail.SavedValues.WeatherCondition);
        // De korte notitie loopt via het bestaande Remarks-veld.
        Assert.Equal("Overstag bij de ton", detail.SavedValues.Remarks);
    }

    [Fact]
    public async Task GetEntryDetailAsync_LeavesEventAndWeatherNull_ForLegacyEntry()
    {
        var entry = new LogbookEntry(
            logbookTripId: 8,
            entryTimeUtc: new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc));
        SetEntryId(entry, 56);

        var trip = new LogbookTrip(name: "Testreis", departureUtc: new DateTime(2026, 7, 16, 8, 0, 0, DateTimeKind.Utc));
        SetTripId(trip, 8);

        _entryRepo
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<LogbookEntry, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entry);
        _tripRepo
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<LogbookTrip, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);
        _entryRepo
            .Setup(r => r.ListAsync(It.IsAny<Expression<Func<LogbookEntry, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LogbookEntry>());
        _positionRepo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<PositionMeasurement, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<PositionMeasurement>());
        _motionRepo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<MotionMeasurement, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<MotionMeasurement>());
        _headingRepo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<HeadingMeasurement, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<HeadingMeasurement>());
        _windRepo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<WindMeasurement, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<WindMeasurement>());
        _depthRepo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<DepthMeasurement, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<DepthMeasurement>());
        _waterTempRepo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<WaterTemperatureMeasurement, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<WaterTemperatureMeasurement>());

        var sut = CreateSut();

        var detail = await sut.GetEntryDetailAsync(56);

        Assert.NotNull(detail);
        Assert.NotNull(detail!.SavedValues);
        // Bestaande regels zonder context blijven null en renderen zo veilig.
        Assert.Null(detail.SavedValues!.EventType);
        Assert.Null(detail.SavedValues.WeatherCondition);
    }
}
