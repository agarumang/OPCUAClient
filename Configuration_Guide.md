# Configuration Guide

## Overview
The application uses `appsettings.json` for all configuration. This file controls OPC UA connections, node mappings, and application behavior.

## Configuration Structure

### OPC UA Settings
```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://127.0.0.1:49320",
    "ApplicationName": "PDF Data Extractor OPC UA Client",
    "SessionTimeout": 60000,
    "OperationTimeout": 15000,
    "AutoAcceptUntrustedCertificates": true,
    "UseSecurity": false,
    "Username": "",
    "Password": "",
    "PreferredAuthenticationType": "Anonymous"
  }
}
```

### Node Mappings
Configure where each data field is written in the OPC UA server:

```json
{
  "NodeMappings": {
    // Basic data
    "StartedTime": "ns=2;s=Channel1.Device1.started",
    "CompletedTime": "ns=2;s=Channel1.Device1.completed",
    "SampleMass": "ns=2;s=Channel1.Device1.sample_mass",
    "AbsoluteDensity": "ns=2;s=Channel1.Device1.absolute_density",
    
    // Measurement cycles (1-10)
    "CycleRow1": "ns=2;s=Channel1.Device1.cycle_row1",
    "CycleRow2": "ns=2;s=Channel1.Device1.cycle_row2",
    // ... up to CycleRow10
    
    // Report information
    "ReportGenerated": "ns=2;s=Channel1.Device1.ReportInfo.generated",
    "SourceFile": "ns=2;s=Channel1.Device1.ReportInfo.source_file",
    
    // Instrument information
    "InstrumentName": "ns=2;s=Channel1.Device1.Instrument.instrument_name",
    "InstrumentSerialNumber": "ns=2;s=Channel1.Device1.Instrument.serial_number",
    
    // Parameters and results
    "ChamberDiameter": "ns=2;s=Channel1.Device1.Parameters.chamber_diameter",
    "AverageEnvelopeVolume": "ns=2;s=Channel1.Device1.Results.average_envelope_volume"
    // ... more mappings
  }
}
```

### Application Settings
```json
{
  "ApplicationSettings": {
    "OutputFolderName": "output",
    "CsvFileName": "CompleteExtractedData.csv",
    "MaxMeasurementCycles": 10
  }
}
```

## Quick Configuration Steps

1. **Set OPC UA Endpoint**: Update `EndpointUrl` to match your OPC UA server
2. **Configure Node Mappings**: Map each data field to the correct OPC UA node ID
3. **Set Output Path**: Configure where CSV files are saved
4. **Security Settings**: Adjust security and authentication as needed

## Testing Configuration

Use the built-in diagnostic tools:

```bash
# Test OPC UA connection
FileReader.exe --diagnostic

# Setup certificates (first time)
FileReader.exe --setup
```

## Common Configuration Patterns

### Kepware Server
```json
{
  "EndpointUrl": "opc.tcp://localhost:49320",
  "NodeMappings": {
    "StartedTime": "ns=2;s=Channel1.Device1.started"
  }
}
```

### Ignition Server
```json
{
  "EndpointUrl": "opc.tcp://localhost:62541/discovery",
  "NodeMappings": {
    "StartedTime": "ns=1;s=[default]PDF_Data.started"
  }
}
```

## Troubleshooting

- **Connection Issues**: Verify `EndpointUrl` and server status
- **Write Failures**: Check node IDs in `NodeMappings`
- **Security Errors**: Adjust `UseSecurity` and certificate settings
- **Timeout Issues**: Increase `SessionTimeout` and `OperationTimeout`