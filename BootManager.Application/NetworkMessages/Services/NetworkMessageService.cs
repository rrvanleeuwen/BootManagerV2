using BootManager.Application.NetworkMessages.DTOs;
using BootManager.Application.NetworkMessageParsing.DTOs;
using BootManager.Application.NetworkMessageParsing.Services;
using BootManager.Application.NetworkMessageInterpretation.Contracts;
using BootManager.Application.NetworkMessageInterpretation.DTOs;
using BootManager.Application.BatteryMeasurements.DTOs;
using BootManager.Application.BatteryMeasurements.Services;
using BootManager.Application.DepthMeasurements.DTOs;
using BootManager.Application.DepthMeasurements.Services;
using BootManager.Application.MotionMeasurements.DTOs;
using BootManager.Application.MotionMeasurements.Services;
using BootManager.Application.PositionMeasurements.DTOs;
using BootManager.Application.PositionMeasurements.Services;
using BootManager.Application.WindMeasurements.DTOs;
using BootManager.Application.WindMeasurements.Services;
using BootManager.Application.HeadingMeasurements.DTOs;
using BootManager.Application.HeadingMeasurements.Services;
using BootManager.Application.SpeedThroughWaterMeasurements.DTOs;
using BootManager.Application.SpeedThroughWaterMeasurements.Services;
using BootManager.Application.WaterTemperatureMeasurements.DTOs;
using BootManager.Application.WaterTemperatureMeasurements.Services;
using BootManager.Application.FluidLevelMeasurements.DTOs;
using BootManager.Application.FluidLevelMeasurements.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BootManager.Application.NetworkMessages.Services;

/// <summary>
/// Implementation van <see cref="INetworkMessageService"/> met behulp van de generieke <see cref="IRepository{T}"/>.
/// Voert parsing uit als tussenstap richting latere interpretatie, zonder extra persistentie.
/// Voert ook semantische interpretatie uit voor ondersteunde berichttypen (bijv. Battery, Depth, Motion) en persisteert succesvolle afleidingen.
/// </summary>
public class NetworkMessageService : INetworkMessageService
{
    private readonly IRepository<NetworkMessage> _repo;
    private readonly INetworkMessageParserService _parserService;
    private readonly INmea0183ParserService _nmea0183ParserService;
    private readonly INetworkMessageInterpreter<BatteryMessageInterpretationDto> _batteryInterpreter;
    private readonly IBatteryMeasurementService _batteryMeasurementService;
    private readonly INetworkMessageInterpreter<DepthMessageInterpretationDto> _depthInterpreter;
    private readonly IDepthMeasurementService _depthMeasurementService;
    private readonly INetworkMessageInterpreter<MotionMessageInterpretationDto> _motionInterpreter;
    private readonly IMotionMeasurementService _motionMeasurementService;
    private readonly INetworkMessageInterpreter<PositionMessageInterpretationDto> _positionInterpreter;
    private readonly IPositionMeasurementService _positionMeasurementService;
    private readonly INetworkMessageInterpreter<WindMessageInterpretationDto> _windInterpreter;
    private readonly IWindMeasurementService _windMeasurementService;
    private readonly INetworkMessageInterpreter<HeadingMessageInterpretationDto> _headingInterpreter;
    private readonly IHeadingMeasurementService _headingMeasurementService;
    private readonly INetworkMessageInterpreter<SpeedThroughWaterMessageInterpretationDto> _speedThroughWaterInterpreter;
    private readonly ISpeedThroughWaterMeasurementService _speedThroughWaterMeasurementService;
    private readonly INetworkMessageInterpreter<WaterTemperatureMessageInterpretationDto> _waterTemperatureInterpreter;
    private readonly IWaterTemperatureMeasurementService _waterTemperatureMeasurementService;
    private readonly INetworkMessageInterpreter<FluidLevelMessageInterpretationDto> _fluidLevelInterpreter;
    private readonly IFluidLevelMeasurementService _fluidLevelMeasurementService;
    // NMEA 0183 Fase 3a interpreters
    private readonly INmea0183MessageInterpreter<SpeedThroughWaterMessageInterpretationDto> _nmea0183VhwInterpreter;
    private readonly INmea0183MessageInterpreter<WaterTemperatureMessageInterpretationDto> _nmea0183MtwInterpreter;
    private readonly INmea0183MessageInterpreter<DepthMessageInterpretationDto> _nmea0183DbtDptInterpreter;
    // NMEA 0183 Fase 3b interpreters
    private readonly INmea0183MessageInterpreter<WindMessageInterpretationDto> _nmea0183MwvInterpreter;
    private readonly INmea0183MessageInterpreter<HeadingMessageInterpretationDto> _nmea0183HdtHdmInterpreter;
    // NMEA 0183 Fase 3c interpreters
    private readonly INmea0183MessageInterpreter<Nmea0183RmcInterpretationDto> _nmea0183RmcInterpreter;
    private readonly INmea0183MessageInterpreter<PositionMessageInterpretationDto> _nmea0183GgaInterpreter;
    private readonly ILogger<NetworkMessageService> _logger;

    /// <summary>
    /// Creëert een nieuwe <see cref="NetworkMessageService"/>.
    /// </summary>
    public NetworkMessageService(
        IRepository<NetworkMessage> repo,
        INetworkMessageParserService parserService,
        INmea0183ParserService nmea0183ParserService,
        INetworkMessageInterpreter<BatteryMessageInterpretationDto> batteryInterpreter,
        IBatteryMeasurementService batteryMeasurementService,
        INetworkMessageInterpreter<DepthMessageInterpretationDto> depthInterpreter,
        IDepthMeasurementService depthMeasurementService,
        INetworkMessageInterpreter<MotionMessageInterpretationDto> motionInterpreter,
        IMotionMeasurementService motionMeasurementService,
        INetworkMessageInterpreter<PositionMessageInterpretationDto> positionInterpreter,
        IPositionMeasurementService positionMeasurementService,
        INetworkMessageInterpreter<WindMessageInterpretationDto> windInterpreter,
        IWindMeasurementService windMeasurementService,
        INetworkMessageInterpreter<HeadingMessageInterpretationDto> headingInterpreter,
        IHeadingMeasurementService headingMeasurementService,
        INetworkMessageInterpreter<SpeedThroughWaterMessageInterpretationDto> speedThroughWaterInterpreter,
        ISpeedThroughWaterMeasurementService speedThroughWaterMeasurementService,
        INetworkMessageInterpreter<WaterTemperatureMessageInterpretationDto> waterTemperatureInterpreter,
        IWaterTemperatureMeasurementService waterTemperatureMeasurementService,
        INetworkMessageInterpreter<FluidLevelMessageInterpretationDto> fluidLevelInterpreter,
        IFluidLevelMeasurementService fluidLevelMeasurementService,
        INmea0183MessageInterpreter<SpeedThroughWaterMessageInterpretationDto> nmea0183VhwInterpreter,
        INmea0183MessageInterpreter<WaterTemperatureMessageInterpretationDto> nmea0183MtwInterpreter,
        INmea0183MessageInterpreter<DepthMessageInterpretationDto> nmea0183DbtDptInterpreter,
        INmea0183MessageInterpreter<WindMessageInterpretationDto> nmea0183MwvInterpreter,
        INmea0183MessageInterpreter<HeadingMessageInterpretationDto> nmea0183HdtHdmInterpreter,
        INmea0183MessageInterpreter<Nmea0183RmcInterpretationDto> nmea0183RmcInterpreter,
        INmea0183MessageInterpreter<PositionMessageInterpretationDto> nmea0183GgaInterpreter,
        ILogger<NetworkMessageService> logger)
    {
        _repo = repo;
        _parserService = parserService;
        _nmea0183ParserService = nmea0183ParserService;
        _batteryInterpreter = batteryInterpreter;
        _batteryMeasurementService = batteryMeasurementService;
        _depthInterpreter = depthInterpreter;
        _depthMeasurementService = depthMeasurementService;
        _motionInterpreter = motionInterpreter;
        _motionMeasurementService = motionMeasurementService;
        _positionInterpreter = positionInterpreter;
        _positionMeasurementService = positionMeasurementService;
        _windInterpreter = windInterpreter;
        _windMeasurementService = windMeasurementService;
        _headingInterpreter = headingInterpreter;
        _headingMeasurementService = headingMeasurementService;
        _speedThroughWaterInterpreter = speedThroughWaterInterpreter;
        _speedThroughWaterMeasurementService = speedThroughWaterMeasurementService;
        _waterTemperatureInterpreter = waterTemperatureInterpreter;
        _waterTemperatureMeasurementService = waterTemperatureMeasurementService;
        _fluidLevelInterpreter = fluidLevelInterpreter;
        _fluidLevelMeasurementService = fluidLevelMeasurementService;
        _nmea0183VhwInterpreter = nmea0183VhwInterpreter;
        _nmea0183MtwInterpreter = nmea0183MtwInterpreter;
        _nmea0183DbtDptInterpreter = nmea0183DbtDptInterpreter;
        _nmea0183MwvInterpreter = nmea0183MwvInterpreter;
        _nmea0183HdtHdmInterpreter = nmea0183HdtHdmInterpreter;
        _nmea0183RmcInterpreter = nmea0183RmcInterpreter;
        _nmea0183GgaInterpreter = nmea0183GgaInterpreter;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(CreateNetworkMessageRequestDto request, CancellationToken ct = default)
    {
        // Map DTO -> entity en persist via generieke repository.
        var entity = NetworkMessage.Create(
            receivedAtUtc: request.ReceivedAtUtc,
            source: request.Source,
            protocol: request.Protocol,
            rawLine: request.RawLine,
            messageId: request.MessageId,
            payloadHex: request.PayloadHex
        );

        // Parsing als tussenstap: voer parse uit voordat we opslaan.
        // Dit resultaat is voorlopig alleen intern en leidt niet tot extra persistentie.
        if (!string.IsNullOrWhiteSpace(request.MessageId) && !string.IsNullOrWhiteSpace(request.PayloadHex))
        {
            try
            {
                var parseRequest = new NetworkMessageParseRequestDto
                {
                    Source = request.Source,
                    ReceivedAtUtc = request.ReceivedAtUtc,
                    RawLine = request.RawLine,
                    MessageIdHex = request.MessageId,
                    PayloadHex = request.PayloadHex
                };

                var parseResult = _parserService.Parse(parseRequest);

                if (parseResult.IsSuccess)
                {
                    _logger.LogInformation(
                        "Netwerkbericht geparset: MessageType={MessageType}, MessageId={MessageId}",
                        parseResult.MessageType,
                        parseResult.MessageIdHex);

                    // Semantische interpretatie en afgeleide opslag voor ondersteunde berichttypen
                    await TryInterpretAndSaveBatteryMessageAsync(parseResult, request, ct);
                    await TryInterpretAndSaveDepthMessageAsync(parseResult, request, ct);
                    await TryInterpretAndSaveMotionMessageAsync(parseResult, request, ct);
                    await TryInterpretAndSavePositionMessageAsync(parseResult, request, ct);
                    await TryInterpretAndSaveWindMessageAsync(parseResult, request, ct);
                    await TryInterpretAndSaveHeadingMessageAsync(parseResult, request, ct);
                    await TryInterpretAndSaveSpeedThroughWaterMessageAsync(parseResult, request, ct);
                    await TryInterpretAndSaveWaterTemperatureMessageAsync(parseResult, request, ct);
                    await TryInterpretAndSaveFluidLevelMessageAsync(parseResult, request, ct);
                }
                else
                {
                    _logger.LogWarning(
                        "Netwerkbericht parse-fout: MessageId={MessageId}, Error={Error}",
                        parseResult.MessageIdHex,
                        parseResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                // Parse-fouten blokkeren geen raw opslag. Log alleen ter info.
                _logger.LogWarning(
                    ex,
                    "Onverwachte fout bij parsing van netwerkbericht MessageId={MessageId}",
                    request.MessageId);
            }
        }

        // NMEA 0183 parsing: wordt uitgevoerd als protocol NMEA0183 is.
        // Raw opslag wordt nooit geblokkeerd door parse-fouten.
        if (string.Equals(request.Protocol, "NMEA0183", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(request.RawLine))
        {
            try
            {
                var nmea0183Result = _nmea0183ParserService.Parse(request.RawLine);

                if (nmea0183Result.IsSuccess)
                {
                    _logger.LogInformation(
                        "NMEA 0183 sentence geparset: Talker={Talker}, Type={Type}, Velden={FieldCount}",
                        nmea0183Result.TalkerPrefix,
                        nmea0183Result.SentenceType,
                        nmea0183Result.Fields.Count);

                    // Fase 3a: sentence-specifieke interpretatie en meting-opslag
                    await TryInterpretAndSaveNmea0183VhwAsync(nmea0183Result, request, ct);
                    await TryInterpretAndSaveNmea0183MtwAsync(nmea0183Result, request, ct);
                    await TryInterpretAndSaveNmea0183DbtDptAsync(nmea0183Result, request, ct);
                    // Fase 3b: MWV en HDT/HDM
                    await TryInterpretAndSaveNmea0183MwvAsync(nmea0183Result, request, ct);
                    await TryInterpretAndSaveNmea0183HdtHdmAsync(nmea0183Result, request, ct);
                    // Fase 3c: RMC en GGA
                    await TryInterpretAndSaveNmea0183RmcAsync(nmea0183Result, request, ct);
                    await TryInterpretAndSaveNmea0183GgaAsync(nmea0183Result, request, ct);

                    // NMEA 2000 gateway sentences: PCDIN en MXPGN met PGN 01F211 (Fluid Level)
                    await TryInterpretAndSaveGatewaySentenceFluidLevelAsync(nmea0183Result, request, ct);
                }
                else
                {
                    _logger.LogWarning(
                        "NMEA 0183 sentence niet herkend: RawLine={RawLine}, Fout={Error}",
                        request.RawLine,
                        nmea0183Result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                // Parse-fouten blokkeren geen raw opslag.
                _logger.LogWarning(
                    ex,
                    "Onverwachte fout bij NMEA 0183 parsing van sentence: {RawLine}",
                    request.RawLine);
            }
        }

        await _repo.AddAsync(entity, ct);
        return entity.Id;
    }

    /// <summary>
    /// Probeert semantische Battery-interpretatie uit te voeren op een technisch parse-resultaat
    /// en persisteert het resultaat als een BatteryMeasurement.
    /// Fouten blokkeren niet de bestaande raw opslag.
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat.</param>
    /// <param name="request">De originele netwerkbericht-request voor metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task TryInterpretAndSaveBatteryMessageAsync(
        NetworkMessageParseResultDto parseResult,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_batteryInterpreter.CanInterpret(parseResult))
            {
                return;
            }

            var interpretation = _batteryInterpreter.Interpret(parseResult);

            if (interpretation.IsSuccess && interpretation.Voltage.HasValue)
            {
                _logger.LogInformation(
                    "Battery-interpretatie geslaagd: Voltage={Voltage}{Unit}",
                    interpretation.Voltage,
                    interpretation.Unit);

                // Persisteer afgeleide battery-meting
                try
                {
                    var batteryDto = new CreateBatteryMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        Voltage = interpretation.Voltage.Value,
                        StateOfCharge = interpretation.StateOfCharge
                    };

                    await _batteryMeasurementService.SaveAsync(batteryDto, ct);
                }
                catch (Exception ex)
                {
                    // Battery-opslag-fouten blokkeren geen raw opslag. Log compact.
                    _logger.LogWarning(
                        ex,
                        "Batterijmeting-opslag mislukt voor MessageId={MessageId}",
                        request.MessageId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Battery-interpretatie mislukt: {Error}",
                    interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            // Interpretatie-fouten blokkeren geen raw opslag.
            _logger.LogWarning(
                ex,
                "Onverwachte fout bij Battery-interpretatie");
        }
    }

    /// <summary>
    /// Probeert semantische Depth-interpretatie uit te voeren op een technisch parse-resultaat
    /// en persisteert het resultaat als een DepthMeasurement.
    /// Fouten blokkeren niet de bestaande raw opslag.
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat.</param>
    /// <param name="request">De originele netwerkbericht-request voor metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task TryInterpretAndSaveDepthMessageAsync(
        NetworkMessageParseResultDto parseResult,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_depthInterpreter.CanInterpret(parseResult))
            {
                return;
            }

            var interpretation = _depthInterpreter.Interpret(parseResult);

            if (interpretation.IsSuccess && interpretation.DepthMeters.HasValue)
            {
                _logger.LogInformation(
                    "Depth-interpretatie geslaagd: Depth={Depth}{Unit}",
                    interpretation.DepthMeters,
                    interpretation.Unit);

                // Persisteer afgeleide depth-meting
                try
                {
                    var depthDto = new CreateDepthMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        DepthMeters = interpretation.DepthMeters.Value
                    };

                    await _depthMeasurementService.SaveAsync(depthDto, ct);
                }
                catch (Exception ex)
                {
                    // Depth-opslag-fouten blokkeren geen raw opslag. Log compact.
                    _logger.LogWarning(
                        ex,
                        "Dieptemeting-opslag mislukt voor MessageId={MessageId}",
                        request.MessageId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Depth-interpretatie mislukt: {Error}",
                    interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            // Interpretatie-fouten blokkeren geen raw opslag.
            _logger.LogWarning(
                ex,
                "Onverwachte fout bij Depth-interpretatie");
        }
    }

    /// <summary>
    /// Probeert semantische Motion-interpretatie uit te voeren op een technisch parse-resultaat
    /// en persisteert het resultaat als een MotionMeasurement.
    /// Fouten blokkeren niet de bestaande raw opslag.
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat.</param>
    /// <param name="request">De originele netwerkbericht-request voor metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task TryInterpretAndSaveMotionMessageAsync(
        NetworkMessageParseResultDto parseResult,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_motionInterpreter.CanInterpret(parseResult))
            {
                return;
            }

            var interpretation = _motionInterpreter.Interpret(parseResult);

            if (interpretation.IsSuccess && interpretation.CourseOverGroundDegrees.HasValue && interpretation.SpeedOverGround.HasValue)
            {
                _logger.LogInformation(
                    "Motion-interpretatie geslaagd: COG={COG}°, SOG={SOG}{Unit}",
                    interpretation.CourseOverGroundDegrees,
                    interpretation.SpeedOverGround,
                    interpretation.SpeedUnit);

                // Persisteer afgeleide motion-meting
                try
                {
                    var motionDto = new CreateMotionMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        CourseOverGroundDegrees = interpretation.CourseOverGroundDegrees.Value,
                        SpeedOverGround = interpretation.SpeedOverGround.Value,
                        SpeedUnit = interpretation.SpeedUnit
                    };

                    await _motionMeasurementService.SaveAsync(motionDto, ct);
                }
                catch (Exception ex)
                {
                    // Motion-opslag-fouten blokkeren geen raw opslag. Log compact.
                    _logger.LogWarning(
                        ex,
                        "Bewegingsmeting-opslag mislukt voor MessageId={MessageId}",
                        request.MessageId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Motion-interpretatie mislukt: {Error}",
                    interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            // Interpretatie-fouten blokkeren geen raw opslag.
            _logger.LogWarning(
                ex,
                "Onverwachte fout bij Motion-interpretatie");
        }
    }

    /// <summary>
    /// Probeert semantische Position-interpretatie uit te voeren op een technisch parse-resultaat
    /// en persisteert het resultaat als een PositionMeasurement.
    /// Fouten blokkeren niet de bestaande raw opslag.
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat.</param>
    /// <param name="request">De originele netwerkbericht-request voor metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task TryInterpretAndSavePositionMessageAsync(
        NetworkMessageParseResultDto parseResult,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_positionInterpreter.CanInterpret(parseResult))
            {
                return;
            }

            var interpretation = _positionInterpreter.Interpret(parseResult);

            if (interpretation.IsSuccess && interpretation.Latitude.HasValue && interpretation.Longitude.HasValue)
            {
                _logger.LogInformation(
                    "Position-interpretatie geslaagd: Latitude={Latitude}{Unit}, Longitude={Longitude}{Unit}",
                    interpretation.Latitude,
                    interpretation.Unit,
                    interpretation.Longitude,
                    interpretation.Unit);

                // Persisteer afgeleide position-meting
                try
                {
                    var positionDto = new CreatePositionMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        Latitude = interpretation.Latitude.Value,
                        Longitude = interpretation.Longitude.Value
                    };

                    await _positionMeasurementService.SaveAsync(positionDto, ct);
                }
                catch (Exception ex)
                {
                    // Position-opslag-fouten blokkeren geen raw opslag. Log compact.
                    _logger.LogWarning(
                        ex,
                        "Positiemeting-opslag mislukt voor MessageId={MessageId}",
                        request.MessageId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Position-interpretatie mislukt: {Error}",
                    interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            // Interpretatie-fouten blokkeren geen raw opslag.
            _logger.LogWarning(
                ex,
                "Onverwachte fout bij Position-interpretatie");
        }
    }

    /// <summary>
    /// Probeert semantische Wind-interpretatie uit te voeren op een technisch parse-resultaat
    /// en persisteert het resultaat als een WindMeasurement.
    /// Fouten blokkeren niet de bestaande raw opslag.
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat.</param>
    /// <param name="request">De originele netwerkbericht-request voor metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task TryInterpretAndSaveWindMessageAsync(
        NetworkMessageParseResultDto parseResult,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_windInterpreter.CanInterpret(parseResult))
            {
                return;
            }

            var interpretation = _windInterpreter.Interpret(parseResult);

            if (interpretation.IsSuccess && interpretation.WindSpeedMps.HasValue && interpretation.WindAngleDegrees.HasValue)
            {
                _logger.LogInformation(
                    "Wind-interpretatie geslaagd: Angle={Angle}{AngleUnit}, Speed={Speed}{SpeedUnit}",
                    interpretation.WindAngleDegrees,
                    interpretation.AngleUnit,
                    interpretation.WindSpeedMps,
                    interpretation.SpeedUnit);

                // Persisteer afgeleide wind-meting
                try
                {
                    var windDto = new CreateWindMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        WindAngleDegrees = interpretation.WindAngleDegrees.Value,
                        WindSpeed = interpretation.WindSpeedMps.Value,
                        SpeedUnit = interpretation.SpeedUnit
                    };

                    await _windMeasurementService.SaveAsync(windDto, ct);
                }
                catch (Exception ex)
                {
                    // Wind-opslag-fouten blokkeren geen raw opslag. Log compact.
                    _logger.LogWarning(
                        ex,
                        "Windmeting-opslag mislukt voor MessageId={MessageId}",
                        request.MessageId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Wind-interpretatie mislukt: {Error}",
                    interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            // Interpretatie-fouten blokkeren geen raw opslag.
            _logger.LogWarning(
                ex,
                "Onverwachte fout bij Wind-interpretatie");
        }
    }

    /// <summary>
    /// Probeert semantische Heading-interpretatie uit te voeren op een technisch parse-resultaat
    /// en persisteert het resultaat als een HeadingMeasurement.
    /// Fouten blokkeren niet de bestaande raw opslag.
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat.</param>
    /// <param name="request">De originele netwerkbericht-request voor metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task TryInterpretAndSaveHeadingMessageAsync(
        NetworkMessageParseResultDto parseResult,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_headingInterpreter.CanInterpret(parseResult))
            {
                return;
            }

            var interpretation = _headingInterpreter.Interpret(parseResult);

            if (interpretation.IsSuccess && interpretation.HeadingDegrees.HasValue)
            {
                _logger.LogInformation(
                    "Heading-interpretatie geslaagd: Heading={Heading}{Unit}",
                    interpretation.HeadingDegrees,
                    interpretation.Unit);

                // Persisteer afgeleide heading-meting
                try
                {
                    var headingDto = new CreateHeadingMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        HeadingDegrees = interpretation.HeadingDegrees.Value
                    };

                    await _headingMeasurementService.SaveAsync(headingDto, ct);
                }
                catch (Exception ex)
                {
                    // Heading-opslag-fouten blokkeren geen raw opslag. Log compact.
                    _logger.LogWarning(
                        ex,
                        "Koersmeting-opslag mislukt voor MessageId={MessageId}",
                        request.MessageId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Heading-interpretatie mislukt: {Error}",
                    interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            // Interpretatie-fouten blokkeren geen raw opslag.
            _logger.LogWarning(
                ex,
                "Onverwachte fout bij Heading-interpretatie");
        }
    }

    /// <summary>
    /// Probeert semantische SpeedThroughWater-interpretatie uit te voeren op een technisch parse-resultaat
    /// en persisteert het resultaat als een SpeedThroughWaterMeasurement.
    /// Fouten blokkeren niet de bestaande raw opslag.
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat.</param>
    /// <param name="request">De originele netwerkbericht-request voor metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task TryInterpretAndSaveSpeedThroughWaterMessageAsync(
        NetworkMessageParseResultDto parseResult,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_speedThroughWaterInterpreter.CanInterpret(parseResult))
            {
                return;
            }

            var interpretation = _speedThroughWaterInterpreter.Interpret(parseResult);

            if (interpretation.IsSuccess && interpretation.SpeedMetersPerSecond.HasValue && interpretation.SpeedKnots.HasValue)
            {
                _logger.LogInformation(
                    "SpeedThroughWater-interpretatie geslaagd: Speed={SpeedMps} m/s ({SpeedKnots} kn)",
                    interpretation.SpeedMetersPerSecond,
                    interpretation.SpeedKnots);

                // Persisteer afgeleide snelheid-door-water-meting
                try
                {
                    var speedDto = new CreateSpeedThroughWaterMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        SpeedMetersPerSecond = interpretation.SpeedMetersPerSecond.Value,
                        SpeedKnots = interpretation.SpeedKnots.Value,
                        SpeedWaterReferenceType = interpretation.SpeedWaterReferenceType
                    };

                    await _speedThroughWaterMeasurementService.SaveAsync(speedDto, ct);
                }
                catch (Exception ex)
                {
                    // Opslag-fouten blokkeren geen raw opslag. Log compact.
                    _logger.LogWarning(
                        ex,
                        "Snelheid-door-water-meting-opslag mislukt voor MessageId={MessageId}",
                        request.MessageId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "SpeedThroughWater-interpretatie mislukt: {Error}",
                    interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            // Interpretatie-fouten blokkeren geen raw opslag.
            _logger.LogWarning(
                ex,
                "Onverwachte fout bij SpeedThroughWater-interpretatie");
        }
    }

    /// <summary>
    /// Probeert semantische WaterTemperature-interpretatie uit te voeren op een technisch parse-resultaat
    /// en persisteert het resultaat als een WaterTemperatureMeasurement.
    /// Fouten blokkeren niet de bestaande raw opslag.
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat.</param>
    /// <param name="request">De originele netwerkbericht-request voor metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task TryInterpretAndSaveWaterTemperatureMessageAsync(
        NetworkMessageParseResultDto parseResult,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_waterTemperatureInterpreter.CanInterpret(parseResult))
            {
                return;
            }

            var interpretation = _waterTemperatureInterpreter.Interpret(parseResult);

            if (interpretation.IsSuccess && interpretation.TemperatureKelvin.HasValue && interpretation.TemperatureCelsius.HasValue)
            {
                _logger.LogInformation(
                    "WaterTemperature-interpretatie geslaagd: TemperatureKelvin={K} K ({C} °C)",
                    interpretation.TemperatureKelvin,
                    interpretation.TemperatureCelsius);

                // Persisteer afgeleide watertemperatuur-meting
                try
                {
                    var temperatureDto = new CreateWaterTemperatureMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        TemperatureInstance = interpretation.TemperatureInstance,
                        TemperatureKelvin = interpretation.TemperatureKelvin.Value,
                        TemperatureCelsius = interpretation.TemperatureCelsius.Value
                    };

                    await _waterTemperatureMeasurementService.SaveAsync(temperatureDto, ct);
                }
                catch (Exception ex)
                {
                    // Opslag-fouten blokkeren geen raw opslag. Log compact.
                    _logger.LogWarning(
                        ex,
                        "Watertemperatuur-meting-opslag mislukt voor MessageId={MessageId}",
                        request.MessageId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "WaterTemperature-interpretatie mislukt: {Error}",
                    interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            // Interpretatie-fouten blokkeren geen raw opslag.
            _logger.LogWarning(
                ex,
                "Onverwachte fout bij WaterTemperature-interpretatie");
        }
    }

    /// <summary>
    /// Probeert semantische FluidLevel-interpretatie uit te voeren op een technisch parse-resultaat
    /// en persisteert het resultaat als een FluidLevelMeasurement.
    /// Fouten blokkeren niet de bestaande raw opslag.
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat.</param>
    /// <param name="request">De originele netwerkbericht-request voor metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task TryInterpretAndSaveFluidLevelMessageAsync(
        NetworkMessageParseResultDto parseResult,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_fluidLevelInterpreter.CanInterpret(parseResult))
            {
                return;
            }

            var interpretation = _fluidLevelInterpreter.Interpret(parseResult);

            if (interpretation.IsSuccess)
            {
                _logger.LogInformation(
                    "FluidLevel-interpretatie geslaagd: FluidType={FluidType}, Instance={Instance}, Level={Level}%",
                    interpretation.FluidType,
                    interpretation.FluidInstance,
                    interpretation.LevelPercent ?? -1);

                // Persisteer afgeleide tankniveau-meting
                try
                {
                    var fluidDto = new CreateFluidLevelMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        Pgn = 127505,
                        GatewaySentence = DeriveGatewaySentenceFromMessageId(request.MessageId),
                        SourceAddress = null, // TODO: extract from payload if available
                        FluidInstance = interpretation.FluidInstance,
                        FluidType = interpretation.FluidType,
                        RawFluidType = interpretation.RawFluidType,
                        LevelPercent = interpretation.LevelPercent,
                        CapacityLiters = interpretation.CapacityLiters,
                        IsLevelInvalid = interpretation.IsLevelInvalid
                    };

                    await _fluidLevelMeasurementService.SaveAsync(fluidDto, ct);
                }
                catch (Exception ex)
                {
                    // Opslag-fouten blokkeren geen raw opslag. Log compact.
                    _logger.LogWarning(
                        ex,
                        "Tankniveau-meting-opslag mislukt voor MessageId={MessageId}",
                        request.MessageId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "FluidLevel-interpretatie mislukt: {Error}",
                    interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            // Interpretatie-fouten blokkeren geen raw opslag.
            _logger.LogWarning(
                ex,
                "Onverwachte fout bij FluidLevel-interpretatie");
        }
    }

    /// <summary>
    /// Leidt het gateway-sentence type af uit de MessageId.
    /// </summary>
    private static string? DeriveGatewaySentenceFromMessageId(string? messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            return null;

        if (messageId.StartsWith("PCDIN", StringComparison.OrdinalIgnoreCase))
            return "PCDIN";

        if (messageId.StartsWith("MXPGN", StringComparison.OrdinalIgnoreCase))
            return "MXPGN";

        return null;
    }

    /// <summary>
    /// Probeert NMEA 0183 VHW-sentence te interpreteren en op te slaan als SpeedThroughWaterMeasurement.
    /// Fouten blokkeren niet de raw opslag.
    /// </summary>
    private async Task TryInterpretAndSaveNmea0183VhwAsync(
        Nmea0183ParseResultDto nmea0183Result,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_nmea0183VhwInterpreter.CanInterpret(nmea0183Result))
                return;

            var interpretation = _nmea0183VhwInterpreter.Interpret(nmea0183Result);

            if (interpretation.IsSuccess && interpretation.SpeedMetersPerSecond.HasValue && interpretation.SpeedKnots.HasValue)
            {
                _logger.LogInformation(
                    "NMEA0183 VHW-interpretatie geslaagd: Speed={SpeedMps} m/s ({SpeedKnots} kn)",
                    interpretation.SpeedMetersPerSecond,
                    interpretation.SpeedKnots);

                try
                {
                    var dto = new CreateSpeedThroughWaterMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        SpeedMetersPerSecond = interpretation.SpeedMetersPerSecond.Value,
                        SpeedKnots = interpretation.SpeedKnots.Value,
                        SpeedWaterReferenceType = interpretation.SpeedWaterReferenceType
                    };
                    await _speedThroughWaterMeasurementService.SaveAsync(dto, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "NMEA0183 VHW-opslag mislukt voor RawLine={RawLine}", request.RawLine);
                }
            }
            else
            {
                _logger.LogWarning("NMEA0183 VHW-interpretatie mislukt: {Error}", interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Onverwachte fout bij NMEA0183 VHW-interpretatie");
        }
    }

    /// <summary>
    /// Probeert NMEA 0183 MTW-sentence te interpreteren en op te slaan als WaterTemperatureMeasurement.
    /// Fouten blokkeren niet de raw opslag.
    /// </summary>
    private async Task TryInterpretAndSaveNmea0183MtwAsync(
        Nmea0183ParseResultDto nmea0183Result,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_nmea0183MtwInterpreter.CanInterpret(nmea0183Result))
                return;

            var interpretation = _nmea0183MtwInterpreter.Interpret(nmea0183Result);

            if (interpretation.IsSuccess && interpretation.TemperatureKelvin.HasValue && interpretation.TemperatureCelsius.HasValue)
            {
                _logger.LogInformation(
                    "NMEA0183 MTW-interpretatie geslaagd: {C} °C ({K} K)",
                    interpretation.TemperatureCelsius,
                    interpretation.TemperatureKelvin);

                try
                {
                    var dto = new CreateWaterTemperatureMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        TemperatureInstance = interpretation.TemperatureInstance,
                        TemperatureKelvin = interpretation.TemperatureKelvin.Value,
                        TemperatureCelsius = interpretation.TemperatureCelsius.Value
                    };
                    await _waterTemperatureMeasurementService.SaveAsync(dto, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "NMEA0183 MTW-opslag mislukt voor RawLine={RawLine}", request.RawLine);
                }
            }
            else
            {
                _logger.LogWarning("NMEA0183 MTW-interpretatie mislukt: {Error}", interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Onverwachte fout bij NMEA0183 MTW-interpretatie");
        }
    }

    /// <summary>
    /// Probeert NMEA 0183 DBT/DPT-sentence te interpreteren en op te slaan als DepthMeasurement.
    /// Fouten blokkeren niet de raw opslag.
    /// </summary>
    private async Task TryInterpretAndSaveNmea0183DbtDptAsync(
        Nmea0183ParseResultDto nmea0183Result,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_nmea0183DbtDptInterpreter.CanInterpret(nmea0183Result))
                return;

            var interpretation = _nmea0183DbtDptInterpreter.Interpret(nmea0183Result);

            if (interpretation.IsSuccess && interpretation.DepthMeters.HasValue)
            {
                _logger.LogInformation(
                    "NMEA0183 DBT/DPT-interpretatie geslaagd: {Depth} m",
                    interpretation.DepthMeters);

                try
                {
                    var dto = new CreateDepthMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        DepthMeters = interpretation.DepthMeters.Value
                    };
                    await _depthMeasurementService.SaveAsync(dto, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "NMEA0183 DBT/DPT-opslag mislukt voor RawLine={RawLine}", request.RawLine);
                }
            }
            else
            {
                _logger.LogWarning("NMEA0183 DBT/DPT-interpretatie mislukt: {Error}", interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Onverwachte fout bij NMEA0183 DBT/DPT-interpretatie");
        }
    }

    /// <summary>
    /// Probeert NMEA 0183 MWV-sentence te interpreteren en op te slaan als WindMeasurement.
    /// Fouten blokkeren niet de raw opslag.
    /// </summary>
    private async Task TryInterpretAndSaveNmea0183MwvAsync(
        Nmea0183ParseResultDto nmea0183Result,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_nmea0183MwvInterpreter.CanInterpret(nmea0183Result))
                return;

            var interpretation = _nmea0183MwvInterpreter.Interpret(nmea0183Result);

            if (interpretation.IsSuccess && interpretation.WindAngleDegrees.HasValue && interpretation.WindSpeedMps.HasValue)
            {
                _logger.LogInformation(
                    "NMEA0183 MWV-interpretatie geslaagd: Hoek={Angle}° Snelheid={Speed} m/s",
                    interpretation.WindAngleDegrees,
                    interpretation.WindSpeedMps);

                try
                {
                    var dto = new CreateWindMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        WindAngleDegrees = interpretation.WindAngleDegrees.Value,
                        WindSpeed = interpretation.WindSpeedMps.Value,
                        SpeedUnit = interpretation.SpeedUnit
                    };
                    await _windMeasurementService.SaveAsync(dto, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "NMEA0183 MWV-opslag mislukt voor RawLine={RawLine}", request.RawLine);
                }
            }
            else
            {
                _logger.LogWarning("NMEA0183 MWV-interpretatie mislukt: {Error}", interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Onverwachte fout bij NMEA0183 MWV-interpretatie");
        }
    }

    /// <summary>
    /// Probeert NMEA 0183 HDT/HDM-sentence te interpreteren en op te slaan als HeadingMeasurement.
    /// Fouten blokkeren niet de raw opslag.
    /// </summary>
    private async Task TryInterpretAndSaveNmea0183HdtHdmAsync(
        Nmea0183ParseResultDto nmea0183Result,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_nmea0183HdtHdmInterpreter.CanInterpret(nmea0183Result))
                return;

            var interpretation = _nmea0183HdtHdmInterpreter.Interpret(nmea0183Result);

            if (interpretation.IsSuccess && interpretation.HeadingDegrees.HasValue)
            {
                _logger.LogInformation(
                    "NMEA0183 HDT/HDM-interpretatie geslaagd: Koers={Heading}°",
                    interpretation.HeadingDegrees);

                try
                {
                    var dto = new CreateHeadingMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        HeadingDegrees = interpretation.HeadingDegrees.Value
                    };
                    await _headingMeasurementService.SaveAsync(dto, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "NMEA0183 HDT/HDM-opslag mislukt voor RawLine={RawLine}", request.RawLine);
                }
            }
            else
            {
                _logger.LogWarning("NMEA0183 HDT/HDM-interpretatie mislukt: {Error}", interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Onverwachte fout bij NMEA0183 HDT/HDM-interpretatie");
        }
    }

    /// <summary>
    /// Probeert NMEA 0183 RMC-sentence te interpreteren en op te slaan als PositionMeasurement en/of MotionMeasurement.
    /// Fouten blokkeren niet de raw opslag.
    /// </summary>
    private async Task TryInterpretAndSaveNmea0183RmcAsync(
        Nmea0183ParseResultDto nmea0183Result,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_nmea0183RmcInterpreter.CanInterpret(nmea0183Result))
                return;

            var interpretation = _nmea0183RmcInterpreter.Interpret(nmea0183Result);

            if (!interpretation.IsSuccess)
            {
                _logger.LogWarning("NMEA0183 RMC-interpretatie mislukt: {Error}", interpretation.ErrorMessage ?? "Onbekende fout");
                return;
            }

            // Sla positiemeting op als positievelden geldig zijn
            if (interpretation.HasValidPosition && interpretation.Latitude.HasValue && interpretation.Longitude.HasValue)
            {
                _logger.LogInformation(
                    "NMEA0183 RMC-positie-interpretatie geslaagd: Lat={Lat}, Lon={Lon}",
                    interpretation.Latitude,
                    interpretation.Longitude);
                try
                {
                    var dto = new CreatePositionMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        Latitude = interpretation.Latitude.Value,
                        Longitude = interpretation.Longitude.Value
                    };
                    await _positionMeasurementService.SaveAsync(dto, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "NMEA0183 RMC-positie-opslag mislukt voor RawLine={RawLine}", request.RawLine);
                }
            }

            // Sla motionmeting op als SOG en COG geldig zijn
            if (interpretation.HasValidMotion && interpretation.SpeedOverGroundKnots.HasValue && interpretation.CourseOverGroundDegrees.HasValue)
            {
                _logger.LogInformation(
                    "NMEA0183 RMC-motion-interpretatie geslaagd: SOG={SOG} kn, COG={COG}°",
                    interpretation.SpeedOverGroundKnots,
                    interpretation.CourseOverGroundDegrees);
                try
                {
                    var dto = new CreateMotionMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        CourseOverGroundDegrees = interpretation.CourseOverGroundDegrees.Value,
                        SpeedOverGround = interpretation.SpeedOverGroundKnots.Value,
                        SpeedUnit = "kn"
                    };
                    await _motionMeasurementService.SaveAsync(dto, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "NMEA0183 RMC-motion-opslag mislukt voor RawLine={RawLine}", request.RawLine);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Onverwachte fout bij NMEA0183 RMC-interpretatie");
        }
    }

    /// <summary>
    /// Probeert NMEA 0183 GGA-sentence te interpreteren en op te slaan als PositionMeasurement.
    /// Fouten blokkeren niet de raw opslag.
    /// </summary>
    private async Task TryInterpretAndSaveNmea0183GgaAsync(
        Nmea0183ParseResultDto nmea0183Result,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_nmea0183GgaInterpreter.CanInterpret(nmea0183Result))
                return;

            var interpretation = _nmea0183GgaInterpreter.Interpret(nmea0183Result);

            if (interpretation.IsSuccess && interpretation.Latitude.HasValue && interpretation.Longitude.HasValue)
            {
                _logger.LogInformation(
                    "NMEA0183 GGA-interpretatie geslaagd: Lat={Lat}, Lon={Lon}",
                    interpretation.Latitude,
                    interpretation.Longitude);
                try
                {
                    var dto = new CreatePositionMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        Latitude = interpretation.Latitude.Value,
                        Longitude = interpretation.Longitude.Value
                    };
                    await _positionMeasurementService.SaveAsync(dto, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "NMEA0183 GGA-opslag mislukt voor RawLine={RawLine}", request.RawLine);
                }
            }
            else
            {
                _logger.LogWarning("NMEA0183 GGA-interpretatie mislukt: {Error}", interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Onverwachte fout bij NMEA0183 GGA-interpretatie");
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NetworkMessageDto>> GetLatestAsync(int limit = 50, CancellationToken ct = default)
    {
        var items = await _repo.ListAsync(ct: ct);

        return items
            .OrderByDescending(x => x.ReceivedAtUtc)
            .Take(limit)
            .Select(x => new NetworkMessageDto
            {
                Id = x.Id,
                ReceivedAtUtc = x.ReceivedAtUtc,
                Source = x.Source,
                Protocol = x.Protocol,
                RawLine = x.RawLine,
                MessageId = x.MessageId,
                PayloadHex = x.PayloadHex
            })
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Probeert PCDIN/MXPGN gateway-sentences met PGN 01F211 te detecteren en als Fluid Level te interpreteren.
    /// Gateway-sentences hebben het patroon: $PCDIN,PGN,fields...,PAYLOAD*CS of $MXPGN,PGN,fields...,PAYLOAD*CS
    /// Fouten blokkeren niet de bestaande raw opslag.
    /// </summary>
    private async Task TryInterpretAndSaveGatewaySentenceFluidLevelAsync(
        Nmea0183ParseResultDto nmea0183Result,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            // Gateway sentences: request.MessageId is the full sentence ID (e.g., "PCDIN" or "MXPGN")
            // as extracted by IngestService.ExtractNmea0183SentenceId()
            string gatewaySentenceId = request.MessageId?.ToUpperInvariant() ?? "";
            bool isPcdin = gatewaySentenceId.Equals("PCDIN", StringComparison.Ordinal);
            bool IsMxpgn = gatewaySentenceId.Equals("MXPGN", StringComparison.Ordinal);

            if (!isPcdin && !IsMxpgn)
            {
                return; // Not a gateway sentence, skip
            }

            // PCDIN and MXPGN format:
            // Field 0: PGN (hex string, e.g. "01F211")
            // Field 1-2: Device/address fields (ignored for now)
            // Last field: 8-byte payload (16 hex chars) or payload may be in different position
            // For PCDIN: $PCDIN,01F211,000024F3,43,PAYLOAD*CS
            // For MXPGN: $MXPGN,01F211,6843,PAYLOAD*CS

            if (nmea0183Result.Fields.Count < 2)
                return; // Not enough fields

            // Extract PGN from field 0
            string pgnHex = nmea0183Result.Fields[0];
            if (!pgnHex.Equals("01F211", StringComparison.OrdinalIgnoreCase))
                return; // Not Fluid Level PGN

            // Find the payload field (usually the last field before checksum, 16 hex chars)
            string? payloadHex = null;
            foreach (var field in nmea0183Result.Fields)
            {
                if (field.Length == 16 && IsValidHexString(field))
                {
                    payloadHex = field;
                    break; // Take first valid 16-char hex field
                }
            }

            if (string.IsNullOrWhiteSpace(payloadHex))
            {
                _logger.LogWarning(
                    "Gateway-sentence {SentenceId}: PGN 01F211 gevonden maar geen 16-byte payload in fields",
                    gatewaySentenceId);
                return;
            }

            // Parse the payload using the standard parser
            try
            {
                var parseRequest = new NetworkMessageParseRequestDto
                {
                    Source = request.Source,
                    ReceivedAtUtc = request.ReceivedAtUtc,
                    RawLine = request.RawLine,
                    MessageIdHex = "01F211",
                    PayloadHex = payloadHex
                };

                var parseResult = _parserService.Parse(parseRequest);

                if (parseResult.IsSuccess)
                {
                    _logger.LogInformation(
                        "Gateway-sentence {SentenceId} Fluid Level geparset: Payload={Payload}",
                        gatewaySentenceId,
                        payloadHex);

                    // Use the standard Fluid Level interpretation
                    if (!_fluidLevelInterpreter.CanInterpret(parseResult))
                    {
                        return;
                    }

                    var interpretation = _fluidLevelInterpreter.Interpret(parseResult);

                    if (interpretation.IsSuccess)
                    {
                        _logger.LogInformation(
                            "Gateway Fluid Level-interpretatie geslaagd: Type={Type}, Instance={Instance}, Level={Level}%",
                            interpretation.FluidType,
                            interpretation.FluidInstance,
                            interpretation.LevelPercent ?? -1);

                        // Persisteer afgeleide tankniveau-meting
                        try
                        {
                            var fluidDto = new CreateFluidLevelMeasurementRequestDto
                            {
                                RecordedAtUtc = request.ReceivedAtUtc,
                                Source = request.Source,
                                MessageId = gatewaySentenceId,
                                Pgn = 127505,
                                GatewaySentence = gatewaySentenceId,
                                SourceAddress = null,
                                FluidInstance = interpretation.FluidInstance,
                                FluidType = interpretation.FluidType,
                                RawFluidType = interpretation.RawFluidType,
                                LevelPercent = interpretation.LevelPercent,
                                CapacityLiters = interpretation.CapacityLiters,
                                IsLevelInvalid = interpretation.IsLevelInvalid
                            };

                            await _fluidLevelMeasurementService.SaveAsync(fluidDto, ct);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(
                                ex,
                                "Gateway Tankniveau-meting-opslag mislukt voor {SentenceType}",
                                nmea0183Result.SentenceType);
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Gateway Fluid Level-interpretatie mislukt: {Error}",
                            interpretation.ErrorMessage ?? "Onbekende fout");
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Gateway-sentence {SentenceType}: Parse-fout voor payload {Payload}: {Error}",
                        nmea0183Result.SentenceType,
                        payloadHex,
                        parseResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Onverwachte fout bij gateway sentence Fluid Level interpretatie");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Onverwachte fout bij gateway sentence verwerking");
        }
    }

    /// <summary>
    /// Bepaalt of een string een geldige hexadecimale waarde is.
    /// </summary>
    private static bool IsValidHexString(string value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.All(c => "0123456789ABCDEFabcdef".Contains(c));
    }
}
