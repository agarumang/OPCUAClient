# Configuration Guide

## Overview
The application uses `appsettings.json` to manage all settings.

## Basic Configuration

### OPC UA Settings
```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://127.0.0.1:49320",
    "ApplicationName": "PDF Data Extractor OPC UA Client",
    "SessionTimeout": 60000,
    "AutoAcceptUntrustedCertificates": true,
    "NodeMappings": {
      "StartedTime": "ns=2;s=PDF.StartedTime",
      "CompletedTime": "ns=2;s=PDF.CompletedTime",
      "LastExtracted": "ns=2;s=PDF.LastExtracted"
    }
  },
  "ApplicationSettings": {
    "ExcelFolderName": "excel",
    "ExcelFileName": "ExtractedExcel.xlsx"
  }
}
```

## Quick Setup

1. **Start Kepware Server** on port 49320
2. **Run the application** - it will auto-configure certificates
3. **Test connection** using `FileReader.exe --diagnostic`

## Common Issues

- **Connection Failed**: Check if Kepware is running on port 49320
- **Certificate Issues**: Set `AutoAcceptUntrustedCertificates: true`
- **Firewall**: Allow port 49320 through Windows Firewall

## Command Line Options

- `FileReader.exe` - Normal operation (process PDF and write to OPC UA)
- `FileReader.exe --diagnostic` - Test OPC UA connection
- `FileReader.exe --setup` - Setup certificates and test connection