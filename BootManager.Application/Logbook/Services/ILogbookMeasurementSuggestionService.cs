using System;
using System.Threading;
using System.Threading.Tasks;
using BootManager.Application.Logbook.DTOs;

namespace BootManager.Application.Logbook.Services;

/// <summary>
/// Contract voor het ophalen van automatische meetdata-suggesties voor een logboekregel.
/// </summary>
public interface ILogbookMeasurementSuggestionService
{
    /// <summary>
    /// Retourneert suggesties voor een logboekregel op het opgegeven tijdstip binnen de opgegeven reis.
    ///
    /// Voor handmatig ingevoerde regels (onlyPeriodData=false):
    /// - Punt-in-tijd velden (Course, Wind, Position) zijn gebaseerd op de meest recente meting vóór of op <paramref name="entryTimeUtc"/>.
    ///
    /// Voor automatisch gemaakte Draft-regels (onlyPeriodData=true):
    /// - Punt-in-tijd velden worden alleen gevuld als er metingen beschikbaar zijn BINNEN het logtijdvak
    ///   (van vorige logboekregel of reisvertrek tot <paramref name="entryTimeUtc"/>).
    /// - Als geen metingen in het tijdvak beschikbaar zijn, blijven die velden leeg.
    ///
    /// Periode-aggregaties (AverageSogKnots) lopen altijd van vorige logboekregel (of reisvertrek) tot <paramref name="entryTimeUtc"/>.
    /// </summary>
    /// <param name="tripId">Reis-ID.</param>
    /// <param name="entryTimeUtc">Logboekregel-moment (UTC).</param>
    /// <param name="onlyPeriodData">
    /// True voor Draft-regels (alleen meetdata uit logtijdvak);
    /// False voor handmatige regels (laatst bekende waarden vóór logmoment).
    /// </param>
    /// <param name="cancellationToken">Cancellationtoken.</param>
    Task<LogbookMeasurementSuggestionDto> GetSuggestionsAsync(
        int tripId,
        DateTime entryTimeUtc,
        bool onlyPeriodData = false,
        CancellationToken cancellationToken = default);
}
