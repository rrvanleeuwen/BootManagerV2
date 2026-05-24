using System;

namespace BootManager.Application.Logbook.DTOs;

/// <summary>
/// DTO voor een gemist logmoment.
/// </summary>
public class MissedMomentDto
{
    /// <summary>
    /// Verwachte tijd (UTC) van dit gemiste logmoment.
    /// </summary>
    public DateTime EntryTimeUtc { get; set; }
}
