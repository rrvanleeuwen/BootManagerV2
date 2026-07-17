using BootManager.Application.Logbook.DTOs;
using BootManager.Application.Logbook.Services;
using BootManager.Application.VesselProfile.Services;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BootManager.UnitTests.Logbook;

/// <summary>
/// Tests voor <see cref="LogbookService"/> rond het handmatig vastleggen van een logboekmoment
/// (<see cref="LogbookService.CreateManualDraftEntryAsync"/>) en de afbakening ten opzichte van de
/// automatische gemiste-momentflow (<see cref="LogbookService.CreateDraftEntryAsync"/>).
/// </summary>
public class LogbookServiceTests
{
    private readonly Mock<IRepository<LogbookTrip>> _tripRepo = new();
    private readonly Mock<IRepository<LogbookEntry>> _entryRepo = new();
    private readonly Mock<ILogbookMeasurementSuggestionService> _suggestionService = new();
    private readonly Mock<ILogbookAttachmentService> _attachmentService = new();
    private readonly Mock<ILogbookEntryDeletionService> _entryDeletionService = new();
    private readonly Mock<IVesselProfileService> _vesselProfileService = new();

    private LogbookService CreateSut() => new(
        _tripRepo.Object,
        _entryRepo.Object,
        _suggestionService.Object,
        _attachmentService.Object,
        _entryDeletionService.Object,
        _vesselProfileService.Object,
        NullLogger<LogbookService>.Instance);

    private static LogbookTrip OpenTrip(int id)
    {
        var trip = new LogbookTrip(name: "Testreis", departureUtc: new DateTime(2026, 7, 16, 8, 0, 0, DateTimeKind.Utc));
        SetTripId(trip, id);
        return trip;
    }

    private static LogbookTrip CompletedTrip(int id)
    {
        var trip = OpenTrip(id);
        trip.CompleteTrip(new DateTime(2026, 7, 16, 18, 0, 0, DateTimeKind.Utc));
        return trip;
    }

    private static void SetTripId(LogbookTrip trip, int id)
    {
        typeof(LogbookTrip).GetProperty(nameof(LogbookTrip.Id))!.SetValue(trip, id);
    }

    [Fact]
    public async Task CreateManualDraftEntryAsync_RequestsLatestKnownSuggestions_AndPersistsDraftSnapshot()
    {
        const int tripId = 42;
        _tripRepo
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<LogbookTrip, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenTrip(tripId));

        var suggestion = new LogbookMeasurementSuggestionDto
        {
            Course = 215,
            WindDescription = "NW 4",
            GpsStatus = "OK",
            Latitude = 52.3702,
            Longitude = 4.8952,
            AverageSogKnots = 5.4m
        };
        _suggestionService
            .Setup(s => s.GetSuggestionsAsync(tripId, It.IsAny<DateTime>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(suggestion);

        LogbookEntry? persisted = null;
        _entryRepo
            .Setup(r => r.AddAsync(It.IsAny<LogbookEntry>(), It.IsAny<CancellationToken>()))
            .Callback<LogbookEntry, CancellationToken>((e, _) => persisted = e)
            .Returns(Task.CompletedTask);
        _attachmentService
            .Setup(a => a.GetAttachmentCountAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var sut = CreateSut();

        var before = DateTime.UtcNow;
        var result = await sut.CreateManualDraftEntryAsync(tripId);
        var after = DateTime.UtcNow;

        // De handmatige momentopname vraagt suggesties met onlyPeriodData: false (laatst bekende waarden).
        _suggestionService.Verify(
            s => s.GetSuggestionsAsync(tripId, It.IsAny<DateTime>(), false, It.IsAny<CancellationToken>()),
            Times.Once);
        _suggestionService.Verify(
            s => s.GetSuggestionsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), true, It.IsAny<CancellationToken>()),
            Times.Never);

        // De regel is precies één keer persistent gemaakt met een Draft-momentopname.
        _entryRepo.Verify(r => r.AddAsync(It.IsAny<LogbookEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(persisted);
        Assert.Equal(tripId, persisted!.LogbookTripId);
        Assert.Equal(LogbookEntryStatus.Draft, persisted.Status);
        Assert.InRange(persisted.EntryTimeUtc, before, after);
        Assert.Equal(215, persisted.Course);
        Assert.Equal("NW 4", persisted.WindDescription);
        Assert.Equal("OK", persisted.GpsStatus);
        Assert.Equal(52.3702, persisted.Latitude);
        Assert.Equal(4.8952, persisted.Longitude);
        Assert.Equal(5.4m, persisted.AverageSogKnots);
        // Handmatige velden blijven leeg bij een momentopname.
        Assert.Null(persisted.BaroPressure);
        Assert.Null(persisted.LogValue);
        Assert.Null(persisted.Remarks);

        // De geretourneerde DTO weerspiegelt hetzelfde concept.
        Assert.Equal(tripId, result.LogbookTripId);
        Assert.Equal(LogbookEntryStatus.Draft, result.Status);
        Assert.Equal(215, result.Course);
        Assert.Equal("NW 4", result.WindDescription);
        Assert.Equal("OK", result.GpsStatus);
        Assert.Equal(52.3702, result.Latitude);
        Assert.Equal(4.8952, result.Longitude);
        Assert.Equal(5.4m, result.AverageSogKnots);
        Assert.InRange(result.EntryTimeUtc, before, after);
    }

    [Fact]
    public async Task CreateManualDraftEntryAsync_LeavesUnavailableSnapshotValuesNull()
    {
        const int tripId = 7;
        _tripRepo
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<LogbookTrip, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenTrip(tripId));

        // Geen enkele boordwaarde beschikbaar: alle suggestievelden zijn null.
        _suggestionService
            .Setup(s => s.GetSuggestionsAsync(tripId, It.IsAny<DateTime>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LogbookMeasurementSuggestionDto());

        LogbookEntry? persisted = null;
        _entryRepo
            .Setup(r => r.AddAsync(It.IsAny<LogbookEntry>(), It.IsAny<CancellationToken>()))
            .Callback<LogbookEntry, CancellationToken>((e, _) => persisted = e)
            .Returns(Task.CompletedTask);
        _attachmentService
            .Setup(a => a.GetAttachmentCountAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var sut = CreateSut();

        var result = await sut.CreateManualDraftEntryAsync(tripId);

        _entryRepo.Verify(r => r.AddAsync(It.IsAny<LogbookEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(persisted);
        // Ontbrekende meetwaarden blijven null; er worden geen waarden verzonnen.
        Assert.Null(persisted!.Course);
        Assert.Null(persisted.WindDescription);
        Assert.Null(persisted.GpsStatus);
        Assert.Null(persisted.Latitude);
        Assert.Null(persisted.Longitude);
        Assert.Null(persisted.AverageSogKnots);
        // Maar de regel is nog steeds een Draft voor de juiste reis.
        Assert.Equal(LogbookEntryStatus.Draft, persisted.Status);
        Assert.Equal(tripId, persisted.LogbookTripId);
        Assert.Equal(LogbookEntryStatus.Draft, result.Status);
    }

    [Fact]
    public async Task CreateManualDraftEntryAsync_RejectsCompletedTrip_AndDoesNotRequestSuggestionsOrAddEntry()
    {
        const int tripId = 99;
        _tripRepo
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<LogbookTrip, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CompletedTrip(tripId));

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateManualDraftEntryAsync(tripId));

        // Een afgesloten reis mag geen suggesties opvragen en geen regel toevoegen.
        _suggestionService.Verify(
            s => s.GetSuggestionsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _entryRepo.Verify(r => r.AddAsync(It.IsAny<LogbookEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateDraftEntryAsync_StillUsesPeriodOnlyFlow_ForAutomaticMissedMoments()
    {
        const int tripId = 5;
        var entryTime = new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc);
        _tripRepo
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<LogbookTrip, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenTrip(tripId));
        _suggestionService
            .Setup(s => s.GetSuggestionsAsync(tripId, entryTime, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LogbookMeasurementSuggestionDto());
        _entryRepo
            .Setup(r => r.AddAsync(It.IsAny<LogbookEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _attachmentService
            .Setup(a => a.GetAttachmentCountAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var sut = CreateSut();

        await sut.CreateDraftEntryAsync(tripId, entryTime);

        // De automatische gemiste-momentflow blijft het strengere onlyPeriodData: true gebruiken.
        _suggestionService.Verify(
            s => s.GetSuggestionsAsync(tripId, entryTime, true, It.IsAny<CancellationToken>()),
            Times.Once);
        _suggestionService.Verify(
            s => s.GetSuggestionsAsync(It.IsAny<int>(), It.IsAny<DateTime>(), false, It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
