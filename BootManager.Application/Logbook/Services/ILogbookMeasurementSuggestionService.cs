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
    /// Punt-in-tijd velden zijn gebaseerd op de meest recente meting vóór of op <paramref name="entryTimeUtc"/>.
    /// Periode-aggregaties lopen van de vorige logboekregel (of reisvertrek) tot <paramref name="entryTimeUtc"/>.
    /// </summary>
    Task<LogbookMeasurementSuggestionDto> GetSuggestionsAsync(int tripId, DateTime entryTimeUtc, CancellationToken cancellationToken = default);
}
