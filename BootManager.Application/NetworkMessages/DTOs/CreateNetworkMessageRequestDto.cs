using System;

namespace BootManager.Application.NetworkMessages.DTOs
{
    /// <summary>
    /// DTO voor het aanmaken van een nieuw NetworkMessage-record.
    /// </summary>
    public class CreateNetworkMessageRequestDto
    {
        /// <summary>
        /// Tijdstempel (UTC) waarop de regel is ontvangen.
        /// </summary>
        public DateTime ReceivedAtUtc { get; set; }

        /// <summary>
        /// Oorsprong van het bericht (bijv. IP-adres, apparaatnaam).
        /// </summary>
        public string Source { get; set; } = default!;

        /// <summary>
        /// Protocol of bronformaat (bijv. "TCP", "UDP", "NMEA").
        /// </summary>
        public string Protocol { get; set; } = default!;

        /// <summary>
        /// De ruwe ontvangen regel zoals binnengekomen (tekst).
        /// </summary>
        public string RawLine { get; set; } = default!;

        /// <summary>
        /// Optionele bericht-id voor correlatie met bronsystemen.
        /// </summary>
        public string? MessageId { get; set; }

        /// <summary>
        /// Optionele hex-gecodeerde payload voor binaire gegevens.
        /// </summary>
        public string? PayloadHex { get; set; }
    }
}