# Configuration Setup Guide

## Overview

This project uses a secure configuration approach where sensitive data is stored in environment variables (Azure App Settings) rather than in source control.

## Local Development Setup

### Development Configuration

Your local development uses `appsettings.Development.json` which contains your local database connection, JWT key, and email credentials. This file is:

- ✅ **Already configured** with your local values
- ✅ **Protected by `.gitignore`** - won't be committed to source control
- ✅ **Automatically loaded** when running in Development environment

### Security Note

- `appsettings.Development.json` is in `.gitignore` and will not be committed to source control
- Never commit real credentials to version control
- Your existing development configuration is secure and ready to use

## Azure Web App Deployment Setup

### 1. Azure App Settings Configuration

In your Azure Web App, configure the following App Settings (Environment Variables):

#### Connection String

- **Name**: `ConnectionStrings__DefaultConnection`
- **Value**: Your Azure SQL Database connection string

#### JWT Configuration

- **Name**: `Jwt__Key`
- **Value**: Your 32-character JWT secret key

#### Email Settings

- **Name**: `EmailSettings__Email`
- **Value**: Your Gmail address
- **Name**: `EmailSettings__Password`
- **Value**: Your Gmail app password

### 2. Azure App Settings Format

Azure App Settings use double underscores (`__`) to represent nested configuration:

```
ConnectionStrings__DefaultConnection = "Server=your-azure-sql-server.database.windows.net;Database=FindepenDB;User Id=your-username;Password=your-password;TrustServerCertificate=True"
Jwt__Key = "your-32-character-jwt-secret-key-here"
EmailSettings__Email = "your-email@gmail.com"
EmailSettings__Password = "your-gmail-app-password"
```

### 3. How It Works

1. **Local Development**: Uses `appsettings.Development.json` (overrides `appsettings.json`)
2. **Azure Production**: Uses `appsettings.json` (with empty strings) + Azure App Settings (environment variables)
3. **Environment Variables Override**: Azure App Settings automatically override any matching configuration values

## Configuration Hierarchy

ASP.NET Core loads configuration in this order (later values override earlier ones):

1. `appsettings.json` (base configuration)
2. `appsettings.{Environment}.json` (environment-specific, only in Development)
3. Environment Variables (Azure App Settings in production)
4. Command Line Arguments

## Security Benefits

- ✅ No sensitive data in source control
- ✅ Environment-specific configuration
- ✅ Azure App Settings are encrypted at rest
- ✅ Easy to rotate secrets without code changes
- ✅ Different values for different environments

## Troubleshooting

### Local Development Issues

- Ensure `appsettings.Development.json` exists and has valid values
- Check that all required fields are filled in

### Azure Deployment Issues

- Verify all Azure App Settings are configured correctly
- Check that connection strings are valid
- Ensure JWT key is exactly 32 characters
- Verify email credentials are correct

### Common Azure App Settings Names

```
ConnectionStrings__DefaultConnection
Jwt__Key
Jwt__Issuer
Jwt__Audience
EmailSettings__Email
EmailSettings__Password
EmailSettings__Host
EmailSettings__DisplayName
EmailSettings__Port
```
