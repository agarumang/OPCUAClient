# Configuration Guide

## Overview
The application now uses a configuration file (`appsettings.json`) to manage all settings, making it easy to deploy and configure without recompiling.

## Configuration File Location
The `appsettings.json` file should be placed in the same directory as the executable (`FileReader.exe`). The application will automatically copy this file to the output directory during build.

## Configuration Structure

### OPC UA Settings
```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://localhost:49320",
    "ApplicationName": "PDF Data Extractor OPC UA Client",
    "SessionTimeout": 60000,
    "OperationTimeout": 15000,
    "AutoAcceptUntrustedCertificates": true,
    "UseSecurity": false,
    "Username": "",
    "Password": "",
    "NodeMappings": {
      "StartedTime": "ns=2;s=PDF.StartedTime",
      "CompletedTime": "ns=2;s=PDF.CompletedTime",
      "SampleMass": "ns=2;s=PDF.SampleMass",
      "AbsoluteDensity": "ns=2;s=PDF.AbsoluteDensity",
      "MeasurementCount": "ns=2;s=PDF.MeasurementCount",
      "LastExtracted": "ns=2;s=PDF.LastExtracted",
      "CycleNodePrefix": "ns=2;s=PDF.Cycle"
    }
  }
}
```

### Application Settings
```json
{
  "ApplicationSettings": {
    "ExcelFolderName": "excel",
    "ExcelFileName": "ExtractedExcel.xlsx",
    "MaxMeasurementCycles": 10
  }
}
```

## Configuration Parameters

### OPC UA Settings

| Parameter | Description | Default Value |
|-----------|-------------|---------------|
| `EndpointUrl` | OPC UA server endpoint URL | `opc.tcp://localhost:49320` |
| `ApplicationName` | Name of the OPC UA client application | `PDF Data Extractor OPC UA Client` |
| `SessionTimeout` | Session timeout in milliseconds | `60000` (60 seconds) |
| `OperationTimeout` | Operation timeout in milliseconds | `15000` (15 seconds) |
| `AutoAcceptUntrustedCertificates` | Accept untrusted certificates automatically | `true` |
| `UseSecurity` | Enable OPC UA security | `false` |
| `Username` | Username for authentication (leave empty for anonymous) | `""` |
| `Password` | Password for authentication | `""` |

### Node Mappings
Configure which OPC UA nodes to write data to:

| Parameter | Description | Default Value |
|-----------|-------------|---------------|
| `StartedTime` | Node for started timestamp | `ns=2;s=PDF.StartedTime` |
| `CompletedTime` | Node for completed timestamp | `ns=2;s=PDF.CompletedTime` |
| `SampleMass` | Node for sample mass value | `ns=2;s=PDF.SampleMass` |
| `AbsoluteDensity` | Node for absolute density value | `ns=2;s=PDF.AbsoluteDensity` |
| `MeasurementCount` | Node for measurement cycle count | `ns=2;s=PDF.MeasurementCount` |
| `LastExtracted` | Node for extraction timestamp | `ns=2;s=PDF.LastExtracted` |
| `CycleNodePrefix` | Prefix for measurement cycle nodes | `ns=2;s=PDF.Cycle` |

### Application Settings

| Parameter | Description | Default Value |
|-----------|-------------|---------------|
| `ExcelFolderName` | Name of the Excel output folder | `excel` |
| `ExcelFileName` | Name of the Excel output file | `ExtractedExcel.xlsx` |
| `MaxMeasurementCycles` | Maximum number of measurement cycles to write to OPC UA | `10` |

## Example Configurations

### For Different Kepware Servers

#### Remote Kepware Server
```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://192.168.1.100:49320"
  }
}
```

#### Different Port
```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://localhost:4840"
  }
}
```

#### With Authentication
```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://localhost:49320",
    "UseSecurity": true,
    "Username": "your_username",
    "Password": "your_password"
  }
}
```

### For Different Node Structures

#### Custom Namespace
```json
{
  "OpcUaSettings": {
    "NodeMappings": {
      "StartedTime": "ns=3;s=MyProject.PDF.StartedTime",
      "CompletedTime": "ns=3;s=MyProject.PDF.CompletedTime",
      "SampleMass": "ns=3;s=MyProject.PDF.SampleMass",
      "AbsoluteDensity": "ns=3;s=MyProject.PDF.AbsoluteDensity",
      "MeasurementCount": "ns=3;s=MyProject.PDF.MeasurementCount",
      "LastExtracted": "ns=3;s=MyProject.PDF.LastExtracted",
      "CycleNodePrefix": "ns=3;s=MyProject.PDF.Cycle"
    }
  }
}
```

#### Numeric Node IDs
```json
{
  "OpcUaSettings": {
    "NodeMappings": {
      "StartedTime": "ns=2;i=1001",
      "CompletedTime": "ns=2;i=1002",
      "SampleMass": "ns=2;i=1003",
      "AbsoluteDensity": "ns=2;i=1004",
      "MeasurementCount": "ns=2;i=1005",
      "LastExtracted": "ns=2;i=1006",
      "CycleNodePrefix": "ns=2;i=2"
    }
  }
}
```

## Deployment Instructions

1. **Build the Application**: `dotnet build`
2. **Copy Files**: Copy the following files to your target directory:
   - `FileReader.exe`
   - `appsettings.json`
   - All `.dll` dependencies
3. **Configure**: Edit `appsettings.json` for your environment
4. **Run**: Execute `FileReader.exe`

## Troubleshooting

### Configuration Not Loading
- Ensure `appsettings.json` is in the same directory as `FileReader.exe`
- Check JSON syntax is valid
- Verify file permissions allow reading

### OPC UA Connection Issues
- Verify `EndpointUrl` is correct
- Check if Kepware server is running
- Ensure firewall allows connection
- Verify node IDs exist in Kepware project

### Missing Nodes
- Check `NodeMappings` configuration
- Ensure nodes exist in Kepware with correct data types
- Verify namespace indices match your Kepware project

## Configuration Validation
The application will:
- Use default values if configuration file is missing
- Create a default configuration file if none exists
- Continue with default values if configuration is invalid
- Log errors silently and continue operation
