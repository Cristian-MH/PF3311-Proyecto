# Azure App Service Free Deployment

This API can run on the Azure App Service `F1` free tier for development,
demonstrations, and learning. It stores data in memory and mirrors each change
to a JSON file. Records expire after at least one hour.

## App Service settings

Configure these application settings in Azure:

| Name | Value |
| --- | --- |
| `DataStore__FilePath` | `/home/data/tele-rehab-data.json` |
| `DataStore__ItemLifetimeHours` | `1` |

For a Linux custom container, also configure:

| Name | Value |
| --- | --- |
| `WEBSITES_ENABLE_APP_SERVICE_STORAGE` | `true` |

The `/home` path is important because Azure App Service uses it for persistent
filesystem storage. The checked-in `Data/tele-rehab-data.json` file is the
local starter file.

## Limits

This approach is intentionally small and inexpensive. Use one App Service
instance only. A JSON file is not suitable for production traffic, multiple
instances, or long-term medical record storage.
