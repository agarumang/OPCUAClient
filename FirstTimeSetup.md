# First Time Setup Guide for New Machines

## Overview
When running the PDF Data Extractor on a new machine where no OPC UA setup has been done, the application needs to create certificates and configure the OPC UA client properly.

## Automatic Setup Process

### What the Application Does Automatically:
1. **Creates Certificate Directories** - Sets up local certificate stores in the application folder
2. **Generates Application Certificate** - Creates a self-signed certificate for the OPC UA client
3. **Configures Security Settings** - Sets up proper security configuration for OPC UA connections
4. **Auto-Accepts Server Certificates** - Automatically trusts the Kepware server certificate on first connection

### Manual Steps (One-Time Only):

#### Option 1: Using the Built-in Setup (Recommended)
1. **Copy Files** to the new machine:
   - `FileReader.exe`
   - `appsettings.json`
   - All `.dll` dependencies

2. **Run the Application** - It will automatically:
   - Create certificate directories
   - Generate required certificates
   - Attempt OPC UA connection
   - Auto-accept server certificates

#### Option 2: Using the Diagnostic Tool
1. **Run Diagnostic First** (optional but recommended):
   ```
   dotnet run --project DiagnosticProgram.cs
   ```
   This will test the connection and show you exactly what's wrong.

2. **Run Certificate Setup** (if needed):
   ```
   dotnet run --project CertificateSetup.cs
   ```

### Common Issues on New Machines:

#### 1. **No Kepware Server**
```
❌ Error: Network connectivity failed
```
**Solution:**
- Install and start Kepware OPC UA Server
- Or change `EndpointUrl` in `appsettings.json` to point to remote server

#### 2. **Firewall Blocking Connection**
```
❌ Error: TCP connection failed
```
**Solution:**
- Allow port 49320 in Windows Firewall
- Add `FileReader.exe` to firewall exceptions

#### 3. **Certificate Permission Issues**
```
❌ Error: Access denied creating certificates
```
**Solution:**
- Run application as Administrator once
- Or use the portable certificate mode (built-in)

#### 4. **Missing Dependencies**
```
❌ Error: Could not load file or assembly
```
**Solution:**
- Ensure all `.dll` files are copied with the executable
- Install .NET Framework 4.8 if not present

## Configuration for New Machine

### Basic Configuration (appsettings.json):
```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://localhost:49320",
    "AutoAcceptUntrustedCertificates": true,
    "UseSecurity": false,
    "Username": "",
    "Password": ""
  }
}
```

### For Remote Kepware Server:
```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://192.168.1.100:49320"
  }
}
```

### For Different Port:
```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://localhost:4840"
  }
}
```

## Deployment Package Structure
```
DeploymentFolder/
├── FileReader.exe                 (Main application)
├── appsettings.json              (Configuration file)
├── *.dll                         (All dependencies)
├── FirstTimeSetup.md             (This guide)
└── Certificates/                 (Created automatically)
    ├── Own/                      (Application certificates)
    ├── TrustedPeers/             (Server certificates)
    ├── TrustedIssuers/           (Certificate authorities)
    └── Rejected/                 (Rejected certificates)
```

## Troubleshooting Steps

### Step 1: Verify Basic Setup
1. Check if Kepware is running: Open Task Manager → Look for "KEPServerEX"
2. Test network: `ping localhost` or `ping <server-ip>`
3. Test port: `telnet localhost 49320`

### Step 2: Check Application
1. Verify all files are present
2. Check `appsettings.json` has correct endpoint
3. Run application - it should create `Certificates` folder automatically

### Step 3: If Connection Still Fails
1. **Run Diagnostic Tool:**
   ```
   # In the application directory
   dotnet FileReader.dll --diagnostic
   ```

2. **Check Certificate Creation:**
   - Look for `Certificates` folder in application directory
   - Should contain `Own`, `TrustedPeers`, `TrustedIssuers`, `Rejected` folders
   - `Own` folder should contain application certificate

3. **Manual Certificate Trust:**
   - If server certificate is rejected, copy it from `Rejected` to `TrustedPeers`
   - Or set `AutoAcceptUntrustedCertificates: true` in config

## Security Considerations

### Development/Testing Environment:
```json
{
  "OpcUaSettings": {
    "AutoAcceptUntrustedCertificates": true,
    "UseSecurity": false
  }
}
```

### Production Environment:
```json
{
  "OpcUaSettings": {
    "AutoAcceptUntrustedCertificates": false,
    "UseSecurity": true,
    "Username": "opcua_user",
    "Password": "secure_password"
  }
}
```

## Quick Start for New Machine

1. **Copy deployment package** to new machine
2. **Edit appsettings.json** if needed (change IP address, port, etc.)
3. **Run FileReader.exe** - it will automatically:
   - Create certificates
   - Connect to OPC UA server
   - Process PDF files
   - Write data to OPC UA nodes
4. **First run might show certificate warnings** - this is normal
5. **Subsequent runs** will connect immediately

## Support Commands

### Create Certificates Manually:
```csharp
await CertificateManager.EnsureCertificatesExistAsync();
```

### Test Connection:
```csharp
await OPCConnectionDiagnostic.DiagnoseConnectionAsync();
```

### Trust Server Certificate:
```csharp
await CertificateManager.TrustServerCertificateAsync("opc.tcp://localhost:49320");
```
