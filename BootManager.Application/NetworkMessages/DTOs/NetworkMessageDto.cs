using System;

namespace BootManager.Application.NetworkMessages.DTOs
{
    /// <summary>
    /// DTO voor het teruggeven van een NetworkMessage.
    /// </summary>
    public class NetworkMessageDto
    {
        /// <summary>
        /// Unieke identificator van het bericht.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tijdstempel (UTC) waarop de regel is ontvangen.
        /// </summary>
        public DateTime ReceivedAtUtc { get; set; }

        /// <summary>
        /// Oorsprong van het bericht.
        /// </summary>
        public string Source { get; set; } = default!;

        /// <summary>
        /// Protocol of bronformaat.
        /// </summary>
        public string Protocol { get; set; } = default!;

        /// <summary>
        /// De ruwe ontvangen regel.
        /// </summary>
        public string RawLine { get; set; } = default!;

        /// <summary>
        /// Optionele bericht-id voor correlatie.
        /// </summary>
        public string? MessageId { get; set; }

        /// <summary>
        /// Optionele hex-gecodeerde payload.
        /// </summary>
        public string? PayloadHex { get; set; }
    }
}