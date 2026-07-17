using Bunit;
using BootManager.Application.Logbook.DTOs;
using BootManager.Application.Logbook.Services;
using BootManager.Application.OperationalSettings.DTOs;
using BootManager.Application.OperationalSettings.Services;
using BootManager.Application.VesselProfile.DTOs;
using BootManager.Application.VesselProfile.Services;
using BootManager.Core.Enums;
using BootManager.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using LogbookPage = BootManager.Web.Components.Pages.Logbook;

namespace BootManager.UnitTests.Logbook;

/// <summary>
/// bUnit-tests voor <see cref="Logbook"/> rond de handmatige actie "Moment vastleggen":
/// zichtbaarheid bij een lopende reis, het effectief aanroepen van de manual-capture-API en het
/// tonen van het geretourneerde concept, en de afwezigheid van de actie bij een afgesloten reis.
/// </summary>
public class LogbookComponentTests : TestContext
{
    private readonly Mock<ILogbookService> _logbookService = new();
    private readonly Mock<ILogbookMeasurementSuggestionService> _suggestionService = new();
    private readonly Mock<ILogbookAttachmentService> _attachmentService = new();
    private readonly Mock<IOperationalSettingsService> _operationalSettingsService = new();
    private readonly Mock<IVesselProfileService> _vesselProfileService = new();

    public LogbookComponentTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        Services.AddScoped(_ => _logbookService.Object);
        Services.AddScoped(_ => _suggestionService.Object);
        Services.AddScoped(_ => _attachmentService.Object);
        Services.AddScoped(_ => _operationalSettingsService.Object);
        Services.AddScoped(_ => _vesselProfileService.Object);

        _operationalSettingsService
            .Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationalSettingsDto());
        _vesselProfileService
            .Setup(s => s.GetOrCreateVesselProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VesselProfileDto { Id = Guid.NewGuid(), VesselName = "Test Vessel" });
    }

    private static LogbookTripDto Trip(int id, LogbookTripStatus status) => new()
    {
        Id = id,
        Name = "Testreis",
        DepartureUtc = new DateTime(2026, 7, 16, 8, 0, 0, DateTimeKind.Utc),
        Status = status,
        LogIntervalMinutes = 60
    };

    private void SetupTrip(LogbookTripDto trip)
    {
        _logbookService
            .Setup(s => s.GetAllTripsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LogbookTripDto> { trip });
        _logbookService
            .Setup(s => s.GetEntriesAsync(trip.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LogbookEntryDto>());
        _logbookService
            .Setup(s => s.GetMissedLogMomentsAsync(trip.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MissedLogMomentsDto { TotalCount = 0, MissedMoments = new List<MissedMomentDto>() });
    }

    [Fact]
    public async Task OpenTrip_CaptureMoment_CallsManualApiOnce_RendersDraft_AndKeepsControls()
    {
        var trip = Trip(11, LogbookTripStatus.Open);
        SetupTrip(trip);

        var draft = new LogbookEntryDto
        {
            Id = 501,
            LogbookTripId = trip.Id,
            EntryTimeUtc = new DateTime(2026, 7, 16, 11, 30, 0, DateTimeKind.Utc),
            Course = 215,
            WindDescription = "NW 4",
            GpsStatus = "OK",
            Latitude = 52.3702,
            Longitude = 4.8952,
            AverageSogKnots = 5.4m,
            Status = LogbookEntryStatus.Draft
        };
        _logbookService
            .Setup(s => s.CreateManualDraftEntryAsync(trip.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(draft);

        var cut = RenderComponent<LogbookPage>();

        // De actie is zichtbaar bij een lopende reis.
        var captureButton = cut.FindAll("button").First(b => b.TextContent.Contains("Moment vastleggen"));

        await cut.InvokeAsync(() => captureButton.Click());

        // Eén klik roept de manual-capture-API precies één keer aan voor de geselecteerde reis.
        _logbookService.Verify(
            s => s.CreateManualDraftEntryAsync(trip.Id, It.IsAny<CancellationToken>()),
            Times.Once);
        // De reis wordt niet afgesloten via de service.
        _logbookService.Verify(
            s => s.CompleteTripAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);

        // Het geretourneerde concept is direct zichtbaar in het overzicht.
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("NW 4", cut.Markup);
            Assert.Contains("Te accorderen", cut.Markup);
        });

        // De actieve-reisbediening blijft zichtbaar (reis blijft open).
        Assert.NotEmpty(cut.FindAll("button").Where(b => b.TextContent.Contains("Moment vastleggen")));
        Assert.NotEmpty(cut.FindAll("button").Where(b => b.TextContent.Contains("Beëindig reis")));
    }

    [Fact]
    public void CompletedTrip_DoesNotRenderMomentVastleggenAction()
    {
        var trip = Trip(22, LogbookTripStatus.Completed);
        SetupTrip(trip);

        var cut = RenderComponent<LogbookPage>();

        // Bij een afgesloten reis wordt de actie niet aangeboden.
        Assert.Empty(cut.FindAll("button").Where(b => b.TextContent.Contains("Moment vastleggen")));
        Assert.Contains("Afgesloten", cut.Markup);
    }
}
