# OPC UA Authentication Troubleshooting Guide

## "EndPoint doesn't support user identity type provided" Error

This error occurs when your OPC UA client tries to use an authentication method that the Kepware server doesn't support.

## How It's Now Fixed

The application now automatically:

1. **Discovers supported authentication methods** from the server
2. **Selects the best compatible method** automatically
3. **Falls back through multiple options** if the preferred method fails
4. **Shows detailed authentication info** in diagnostic mode

## Automatic Authentication Selection

The application tries authentication methods in this priority order:

1. **Anonymous** (most common for development)
2. **Username/Password** (if credentials provided)
3. **Certificate-based** (if certificates available)
4. **First available method** (server's default)

## Configuration Options

### Basic Anonymous Authentication (Default)
```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://localhost:49320",
    "PreferredAuthenticationType": "Anonymous",
    "Username": "",
    "Password": ""
  }
}
```

### Username/Password Authentication
```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://localhost:49320",
    "PreferredAuthenticationType": "UserName",
    "Username": "your_username",
    "Password": "your_password"
  }
}
```

### Certificate-based Authentication
```json
{
  "OpcUaSettings": {
    "EndpointUrl": "opc.tcp://localhost:49320",
    "PreferredAuthenticationType": "Certificate",
    "UseSecurity": true
  }
}
```

## Diagnostic Commands

### Check Supported Authentication Methods
```bash
FileReader.exe --diagnostic
```

This will show:
- Available endpoints
- Security policies
- **Supported authentication types**
- Which method was selected

### Example Output
```
Endpoint discovery:
✅ Found 2 endpoint(s):
   - opc.tcp://localhost:49320
     Security: http://opcfoundation.org/UA/SecurityPolicy#None
     Security Mode: None
     Supported Authentication:
       • Anonymous (anonymous)
       • UserName (username_password)
```

## Common Kepware Configurations

### Default Kepware Setup
- **Supports**: Anonymous, Username/Password
- **Default**: Anonymous authentication
- **Security**: None (no encryption)

### Secured Kepware Setup
- **Supports**: Username/Password, Certificate
- **Requires**: Valid user accounts in Kepware
- **Security**: Basic256Sha256 or higher

### Restricted Kepware Setup
- **Supports**: Certificate only
- **Requires**: Client certificates trusted by server
- **Security**: SignAndEncrypt

## Troubleshooting Steps

### Step 1: Check What's Supported
```bash
FileReader.exe --diagnostic
```
Look for the "Supported Authentication" section.

### Step 2: Match Your Configuration
Update `appsettings.json` based on what the server supports:

- If server shows `Anonymous` → Use empty username/password
- If server shows `UserName` → Provide valid credentials
- If server shows `Certificate` → Enable security and certificates

### Step 3: Test Connection
```bash
FileReader.exe --setup
```
This will attempt connection with automatic authentication selection.

## Common Scenarios

### Scenario 1: Development Kepware (Default)
**Server supports**: Anonymous, UserName
**Solution**: Use default configuration (Anonymous)
```json
{
  "Username": "",
  "Password": "",
  "PreferredAuthenticationType": "Anonymous"
}
```

### Scenario 2: Production Kepware with Users
**Server supports**: UserName only
**Solution**: Configure valid credentials
```json
{
  "Username": "opcua_client",
  "Password": "secure_password",
  "PreferredAuthenticationType": "UserName"
}
```

### Scenario 3: High-Security Kepware
**Server supports**: Certificate only
**Solution**: Enable security and certificates
```json
{
  "UseSecurity": true,
  "PreferredAuthenticationType": "Certificate",
  "AutoAcceptUntrustedCertificates": false
}
```

## Error Messages and Solutions

### "EndPoint doesn't support user identity type provided"
**Cause**: Authentication method mismatch
**Solution**: Run diagnostic to see supported methods, then update config

### "BadUserAccessDenied"
**Cause**: Invalid username/password
**Solution**: Check credentials in Kepware user management

### "BadSecurityChecksFailed"
**Cause**: Certificate issues in secured mode
**Solution**: Verify certificates are trusted, or use `AutoAcceptUntrustedCertificates: true`

### "BadIdentityTokenInvalid"
**Cause**: Token format not accepted by server
**Solution**: Try different authentication type or check server configuration

## Kepware Server Configuration

### To Enable Anonymous Access:
1. Open Kepware Configuration
2. Go to Project Properties → OPC UA
3. Enable "Allow Anonymous" under Authentication

### To Add User Accounts:
1. Go to Project Properties → OPC UA → Users
2. Add new user with appropriate permissions
3. Use those credentials in application config

### To Configure Certificates:
1. Go to Project Properties → OPC UA → Certificates
2. Add client certificates to trusted store
3. Configure security policies

## Application Behavior

The application now:
- ✅ **Automatically detects** supported authentication methods
- ✅ **Selects the best option** based on configuration and availability
- ✅ **Provides detailed feedback** about authentication choices
- ✅ **Falls back gracefully** if preferred method fails
- ✅ **Continues operation** with compatible authentication

Your connection should now work regardless of Kepware's authentication configuration!
