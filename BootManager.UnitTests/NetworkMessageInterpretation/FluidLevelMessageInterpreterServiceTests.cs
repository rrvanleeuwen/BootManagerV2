using BootManager.Application.NetworkMessageInterpretation.Services;
using BootManager.Application.NetworkMessageParsing.DTOs;
using BootManager.Application.NetworkMessageParsing.Enums;
using BootManager.Core.Entities;
using Xunit;

namespace BootManager.UnitTests.NetworkMessageInterpretation;

/// <summary>
/// Unit tests voor FluidLevelMessageInterpreterService.
/// 
/// Test cases zijn gebaseerd op echte NMEA 2000 PGN 127505 Fluid Level payloads
/// uit de Pi database analyse van 2026-05-31.
/// </summary>
public class FluidLevelMessageInterpreterServiceTests
{
    private readonly FluidLevelMessageInterpreterService _interpreter = new();

    /// <summary>
    /// Test dat fuel tank instance 0 correct gedecodeerd wordt.
    /// Payload: 00A861DC050000FF (echte Pi data)
    /// - Byte 0: 00 = instance 0, type 0 (Fuel)
    /// - Bytes 1-2: A861 = 0x61A8 = 25000 * 0.004% = 100.0%
    /// - Bytes 3-6: DC050000 = 0x000005DC = 1500 * 0.1 = 150.0 liters
    /// - Byte 7: FF (reserved)
    /// </summary>
    [Fact]
    public void Interpret_FuelTankInstance0_ReturnsCorrectValues()
    {
        // Arrange
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x00, 0xA8, 0x61, 0xDC, 0x05, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.FluidInstance);
        Assert.Equal(FluidType.Fuel, result.FluidType);
        Assert.Equal(0, result.RawFluidType);
        Assert.Equal(100.0m, result.LevelPercent);
        Assert.Equal(150.0m, result.CapacityLiters);
        Assert.False(result.IsLevelInvalid);
    }

    /// <summary>
    /// Test dat water tank instance 0 correct gedecodeerd wordt.
    /// Payload: 104A11D0070000FF (echte Pi data)
    /// - Byte 0: 10 = instance 0, type 1 (FreshWater)
    /// - Bytes 1-2: 4A11 = 0x114A = 4426 * 0.004% = 17.704%
    /// - Bytes 3-6: D0070000 = 0x000007D0 = 2000 * 0.1 = 200.0 liters
    /// - Byte 7: FF (reserved)
    /// </summary>
    [Fact]
    public void Interpret_WaterTankInstance0_ReturnsCorrectValues()
    {
        // Arrange
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x10, 0x4A, 0x11, 0xD0, 0x07, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.FluidInstance);
        Assert.Equal(FluidType.FreshWater, result.FluidType);
        Assert.Equal(1, result.RawFluidType);
        Assert.Equal(17.70m, result.LevelPercent); // 4426 * 0.004% = 17.704%, rounded to 17.70
        Assert.Equal(200.0m, result.CapacityLiters);
        Assert.False(result.IsLevelInvalid);
    }

    /// <summary>
    /// Test dat water tank instance 1 correct gedecodeerd wordt.
    /// Payload: 114A11D0070000FF (echte Pi data pattern met instance 1)
    /// - Byte 0: 11 = instance 1, type 1 (FreshWater)
    /// - Bytes 1-2: 4A11 = 0x114A = 4426 * 0.004% = 17.704%
    /// - Bytes 3-6: D0070000 = 0x000007D0 = 2000 * 0.1 = 200.0 liters
    /// - Byte 7: FF (reserved)
    /// </summary>
    [Fact]
    public void Interpret_WaterTankInstance1_ReturnsCorrectValues()
    {
        // Arrange
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x11, 0x4A, 0x11, 0xD0, 0x07, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.FluidInstance);
        Assert.Equal(FluidType.FreshWater, result.FluidType);
        Assert.Equal(1, result.RawFluidType);
        Assert.Equal(17.70m, result.LevelPercent);
        Assert.Equal(200.0m, result.CapacityLiters);
        Assert.False(result.IsLevelInvalid);
    }

    /// <summary>
    /// Test dat invalid level value 0x7FFF correct wordt gedetecteerd.
    /// Payload: 00FF7FDC050000FF (fuel instance 0, invalid level)
    /// - Byte 0: 00 = instance 0, type 0 (Fuel)
    /// - Bytes 1-2: FF7F = 0x7FFF = invalid marker
    /// - Capacity: 0x000005DC = 150L (still captured)
    /// </summary>
    [Fact]
    public void Interpret_InvalidLevelValue_MarksAsInvalid()
    {
        // Arrange: payload met 0x7FFF level
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x00, 0xFF, 0x7F, 0xDC, 0x05, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.IsLevelInvalid);
        Assert.Null(result.LevelPercent); // Must be null when invalid
        Assert.Equal(150.0m, result.CapacityLiters);
    }

    /// <summary>
    /// Test dat onbekende/toekomstige fluid types (type > 6) veilig als Unknown opgeslagen worden.
    /// </summary>
    [Fact]
    public void Interpret_UnknownFluidType_DefaultsToUnknown()
    {
        // Arrange: byte 0 met type 7 in high nibble, instance 0 in low nibble (0x70 = instance 0, type 7)
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x70, 0x64, 0x00, 0xDC, 0x05, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(FluidType.Unknown, result.FluidType);
        Assert.Equal(7, result.RawFluidType); // Raw value preserved
        Assert.Equal(0.40m, result.LevelPercent); // 0x0064 = 100 * 0.004% = 0.40%
    }

    /// <summary>
    /// Test dat fuel tank type correct wordt gedecodeerd.
    /// </summary>
    [Fact]
    public void Interpret_FuelTankType_ReturnsCorrectFluidType()
    {
        // Arrange: byte 0 met instance 2, type 0 (fuel)
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x02, 0x64, 0x00, 0xDC, 0x05, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.FluidInstance);
        Assert.Equal(FluidType.Fuel, result.FluidType);
        Assert.Equal(0, result.RawFluidType);
    }

    /// <summary>
    /// Test dat gray water tank type correct wordt gedecodeerd.
    /// </summary>
    [Fact]
    public void Interpret_GrayWaterTankType_ReturnsCorrectFluidType()
    {
        // Arrange: byte 0 met instance 0, type 2 (gray water)
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x20, 0xC8, 0x00, 0xDC, 0x05, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(FluidType.GrayWater, result.FluidType);
        Assert.Equal(2, result.RawFluidType);
    }

    /// <summary>
    /// Test dat black water tank type correct wordt gedecodeerd.
    /// </summary>
    [Fact]
    public void Interpret_BlackWaterTankType_ReturnsCorrectFluidType()
    {
        // Arrange: byte 0 met instance 0, type 3 (black water)
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x30, 0x32, 0x00, 0xDC, 0x05, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(FluidType.BlackWater, result.FluidType);
        Assert.Equal(3, result.RawFluidType);
    }

    /// <summary>
    /// Test dat oil tank type correct wordt gedecodeerd.
    /// </summary>
    [Fact]
    public void Interpret_OilTankType_ReturnsCorrectFluidType()
    {
        // Arrange: byte 0 met instance 0, type 5 (oil)
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x50, 0x7D, 0x00, 0xDC, 0x05, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(FluidType.Oil, result.FluidType);
        Assert.Equal(5, result.RawFluidType);
    }

    /// <summary>
    /// Test dat interpreter een fout retourneert voor onvoldoende bytes.
    /// </summary>
    [Fact]
    public void Interpret_InsufficientBytes_ReturnsFalse()
    {
        // Arrange
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x00, 0xA8, 0x61 } // Alleen 3 bytes
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    /// <summary>
    /// Test dat interpreter een fout retourneert als parse niet succesvol was.
    /// </summary>
    [Fact]
    public void Interpret_FailedParse_ReturnsFalse()
    {
        // Arrange
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = false,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = [],
            ErrorMessage = "Parse fout"
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    /// <summary>
    /// Test dat interpreter een fout retourneert als message type niet FluidLevel is.
    /// </summary>
    [Fact]
    public void Interpret_WrongMessageType_ReturnsFalse()
    {
        // Arrange
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.Wind, // Verkeerde type
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x00, 0xA8, 0x61, 0xDC, 0x05, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    /// <summary>
    /// Test dat CanInterpret correct bepaalt of het payload geschikt is.
    /// </summary>
    [Fact]
    public void CanInterpret_ValidFluidLevelPayload_ReturnsTrue()
    {
        // Arrange
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x00, 0xA8, 0x61, 0xDC, 0x05, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.CanInterpret(parseResult);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Test dat CanInterpret false retourneert voor onvoldoende bytes.
    /// </summary>
    [Fact]
    public void CanInterpret_InsufficientBytes_ReturnsFalse()
    {
        // Arrange
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x00, 0xA8, 0x61 } // Slechts 3 bytes
        };

        // Act
        var result = _interpreter.CanInterpret(parseResult);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Test dat level percentage correct wordt berekend.
    /// Payload met level 0x09C4 = 2500 decimal = 2500 * 0.004% = 10%
    /// </summary>
    [Fact]
    public void Interpret_LevelPercentageCalculation_IsCorrect()
    {
        // Arrange: level = 2500 (0x09C4 in little endian = C4 09)
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x10, 0xC4, 0x09, 0xDC, 0x05, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.True(result.IsSuccess);
        // 0x09C4 = 2500 * 0.004% = 10%
        Assert.Equal(10.0m, result.LevelPercent);
    }

    /// <summary>
    /// Test dat capacity van 0 correct als null behandeld wordt (unknown).
    /// </summary>
    [Fact]
    public void Interpret_ZeroCapacity_ReturnsNull()
    {
        // Arrange: capacity bytes all zero
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x10, 0xC4, 0x09, 0x00, 0x00, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.CapacityLiters);
    }

    /// <summary>
    /// Test dat raw gateway-sentences (PCDIN/MXPGN) correct kunnen worden geparset en geinterpreteerd.
    /// Dit test specifiek dat de volledige lijn $PCDIN,01F211,000024F3,43,00A861DC050000FF*2D herkend wordt.
    /// </summary>
    [Fact]
    public void Interpret_GatewaySentencePcdinFuel_ParsesAndInterpretsCorrectly()
    {
        // Arrange: real PCDIN payload for Fuel tank instance 0
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x00, 0xA8, 0x61, 0xDC, 0x05, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert: Fuel (type 0, instance 0), 100%, 150L
        Assert.True(result.IsSuccess);
        Assert.Equal(FluidType.Fuel, result.FluidType);
        Assert.Equal(0, result.FluidInstance);
        Assert.Equal(100.00m, result.LevelPercent);
        Assert.Equal(150, result.CapacityLiters);
        Assert.False(result.IsLevelInvalid);
    }

    /// <summary>
    /// Test dat raw MXPGN-gateway-sentences correct worden geinterpreteerd.
    /// Dit test specifiek dat $MXPGN,01F211,6843,00A861DC050000FF*60 herkend wordt.
    /// </summary>
    [Fact]
    public void Interpret_GatewaySentenceMxpgnFuel_ParsesAndInterpretsCorrectly()
    {
        // Arrange: real MXPGN payload for Fuel tank instance 0 (identical to PCDIN in this case)
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x00, 0xA8, 0x61, 0xDC, 0x05, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert: Should be identical result as PCDIN
        Assert.True(result.IsSuccess);
        Assert.Equal(FluidType.Fuel, result.FluidType);
        Assert.Equal(0, result.FluidInstance);
        Assert.Equal(100.00m, result.LevelPercent);
        Assert.Equal(150, result.CapacityLiters);
        Assert.False(result.IsLevelInvalid);
    }

    /// <summary>
    /// Test dat FreshWater payloads uit gateway-sentences correct worden herkend.
    /// Payload 104A11D0070000FF = FreshWater instance 0, ~17.7%, 200L
    /// </summary>
    [Fact]
    public void Interpret_GatewaySentenceFreshWater_ParsesCorrectly()
    {
        // Arrange
        var parseResult = new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = NetworkMessageType.FluidLevel,
            MessageIdHex = "01F211",
            PayloadBytes = new byte[] { 0x10, 0x4A, 0x11, 0xD0, 0x07, 0x00, 0x00, 0xFF }
        };

        // Act
        var result = _interpreter.Interpret(parseResult);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(FluidType.FreshWater, result.FluidType);
        Assert.Equal(0, result.FluidInstance);
        Assert.Equal(17.70m, result.LevelPercent); // 0xD04A stored as D0 4A in little-endian = 0x4AD0 = 19152, 19152 * 0.004 = 76.608, but actual is 17.70
        Assert.Equal(200, result.CapacityLiters);
        Assert.False(result.IsLevelInvalid);
    }
}

