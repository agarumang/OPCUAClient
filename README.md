# PDF Data Extractor with OPC UA Integration

A clean, simple application that extracts data from PDF files and writes it to OPC UA servers (like Kepware).

## Features

- **PDF Processing**: Extracts measurement data from PDF reports
- **Excel Export**: Creates Excel files with extracted data
- **OPC UA Integration**: Writes data to OPC UA servers with support for:
  - Single value writes
  - Double array writes
  - Server browsing
  - Automatic certificate handling

## Quick Start

1. **Start your Kepware server** on `opc.tcp://127.0.0.1:49320`
2. **Run the application**: `FileReader.exe`
3. **Select a PDF file** when prompted
4. **Data is automatically**:
   - Extracted from PDF
   - Saved to Excel
   - Written to OPC UA server

## Command Line Options

- `FileReader.exe` - Normal operation
- `FileReader.exe --diagnostic` - Test OPC UA connection
- `FileReader.exe --setup` - Setup certificates and test

## Configuration

Edit `appsettings.json` to configure:

```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://127.0.0.1:49320",
    "ApplicationName": "PDF Data Extractor OPC UA Client",
    "SessionTimeout": 60000,
    "AutoAcceptUntrustedCertificates": true
  }
}
```

## Key Features of Clean Code

- **Simple OPC UA Connection**: Uses proven connection approach
- **Automatic Certificate Setup**: No manual certificate configuration needed
- **Error Handling**: Graceful handling of connection issues
- **Double Array Support**: Can write measurement arrays directly to OPC UA nodes
- **Minimal Dependencies**: Only essential packages included

## Example Usage

The application can write measurement data as double arrays:

```csharp
// Example: Write measurement values as array
double[] values = {18577, 13981, 13.9755, 0.1306, 0.5474, -0.0052};
opcClient.WriteDoubleArray("Channel1.Device1.Tag3", values);
```

## Troubleshooting

- **Connection Issues**: Run `FileReader.exe --diagnostic`
- **Certificate Problems**: Set `AutoAcceptUntrustedCertificates: true`
- **Firewall**: Allow port 49320 through Windows Firewall
- **Kepware Not Running**: Start Kepware Server service

## Requirements

- .NET Framework 4.8
- Windows OS
- Kepware or compatible OPC UA server

The code is now clean, simple, and maintainable!