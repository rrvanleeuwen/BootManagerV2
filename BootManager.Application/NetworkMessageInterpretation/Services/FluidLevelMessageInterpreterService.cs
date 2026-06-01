namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using BootManager.Core.Entities;
using NetworkMessageParsing.DTOs;
using NetworkMessageParsing.Enums;

/// <summary>
/// Semantische interpreter voor tankniveau-berichten.
/// 
/// Gebaseerd op PGN 127505 (Fluid Level):
/// - Byte 0:   Instance (bits 0-3) + Type (bits 4-7)
/// - Bytes 1-2: Level in 0,004% increments (uint16, little-endian). Waarde 0x7FFF geeft invalid aan.
/// - Bytes 3-6: Capacity in 0,1 liter increments (uint32, little-endian)
/// - Byte 7:   Reserved
/// 
/// Speciale waarde 0x7FFF voor level geeft aan dat de waarde onbekend/ongeldig is.
/// </summary>
public class FluidLevelMessageInterpreterService : INetworkMessageInterpreter<FluidLevelMessageInterpretationDto>
{
    private const ushort InvalidLevelValue = 0x7FFF;
    private const decimal LevelScaleFactor = 0.004m;  // 0,004% per increment
    private const decimal CapacityScaleFactor = 0.1m; // 0,1 liter per increment

    /// <summary>
    /// Bepaalt of dit parse-resultaat door deze interpreter kan worden geïnterpreteerd.
    /// </summary>
    public bool CanInterpret(NetworkMessageParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && parseResult.MessageType == NetworkMessageType.FluidLevel
            && parseResult.PayloadBytes.Length >= 8;
    }

    /// <summary>
    /// Voert semantische interpretatie uit.
    /// </summary>
    public FluidLevelMessageInterpretationDto Interpret(NetworkMessageParseResultDto parseResult)
    {
        if (!parseResult.IsSuccess)
        {
            return new FluidLevelMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Parse-resultaat is niet succesvol."
            };
        }

        if (parseResult.MessageType != NetworkMessageType.FluidLevel)
        {
            return new FluidLevelMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Bericht-type is {parseResult.MessageType}, geen FluidLevel."
            };
        }

        if (parseResult.PayloadBytes.Length < 8)
        {
            return new FluidLevelMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Onvoldoende bytes in payload. Minimaal 8 bytes vereist."
            };
        }

        try
        {
            byte instanceAndType = parseResult.PayloadBytes[0];
            byte fluidInstance = (byte)(instanceAndType & 0x0F);
            byte rawFluidType = (byte)((instanceAndType >> 4) & 0x0F);

            // Parse FluidType enum, fallback naar Unknown
            var fluidType = FluidTypeFromRawValue(rawFluidType);

            // Bytes 1-2: level in 0,004% increments (uint16, little-endian)
            ushort rawLevel = (ushort)(parseResult.PayloadBytes[1] | (parseResult.PayloadBytes[2] << 8));

            // Controleer op invalid level marker
            bool isLevelInvalid = rawLevel == InvalidLevelValue;
            decimal? levelPercent = null;

            if (!isLevelInvalid && rawLevel <= 25000) // 25000 * 0,004% = 100% (sanity check)
            {
                levelPercent = Math.Round(rawLevel * LevelScaleFactor, 2);
                // Zorg dat percentage niet boven 100 gaat (uitgezonderd invalid marker)
                if (levelPercent > 100)
                {
                    levelPercent = 100;
                }
            }

            // Bytes 3-6: capacity in 0,1 liter increments (uint32, little-endian)
            uint rawCapacity = (uint)(
                parseResult.PayloadBytes[3] |
                (parseResult.PayloadBytes[4] << 8) |
                (parseResult.PayloadBytes[5] << 16) |
                (parseResult.PayloadBytes[6] << 24)
            );

            decimal? capacityLiters = null;
            if (rawCapacity > 0 && rawCapacity < uint.MaxValue) // 0xFFFFFFFF geeft unknown aan
            {
                capacityLiters = Math.Round(rawCapacity * CapacityScaleFactor, 1);
            }

            return new FluidLevelMessageInterpretationDto
            {
                IsSuccess = true,
                FluidInstance = fluidInstance,
                FluidType = fluidType,
                RawFluidType = rawFluidType,
                LevelPercent = levelPercent,
                CapacityLiters = capacityLiters,
                IsLevelInvalid = isLevelInvalid
            };
        }
        catch (Exception ex)
        {
            return new FluidLevelMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Onverwachte fout bij decoding: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Converteert ruwe type-waarde naar FluidType enum.
    /// </summary>
    private static FluidType FluidTypeFromRawValue(byte rawValue)
    {
        return rawValue switch
        {
            0 => FluidType.Fuel,
            1 => FluidType.FreshWater,
            2 => FluidType.GrayWater,
            3 => FluidType.BlackWater,
            4 => FluidType.LiveWell,
            5 => FluidType.Oil,
            _ => FluidType.Unknown // Veilige fallback voor onbekende/toekomstige types
        };
    }
}
