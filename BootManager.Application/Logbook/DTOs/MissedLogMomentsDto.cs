using System;
using System.Collections.Generic;

namespace BootManager.Application.Logbook.DTOs;

/// <summary>
/// DTO voor gegevens over gemiste logmomenten voor een reis.
/// </summary>
public class MissedLogMomentsDto
{
    /// <summary>
    /// Totaal aantal gemiste logmomenten.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Lijst van gemiste logmomenten (UTC). Chronologisch oplopend gesorteerd.
    /// </summary>
    public IReadOnlyList<MissedMomentDto> MissedMoments { get; set; } = new List<MissedMomentDto>();
}
