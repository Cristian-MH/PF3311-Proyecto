# PF3311 TeleRehab API

Experimental ASP.NET Core API for a telerehabilitation prototype. It provides
patients, therapies, therapy logs, motivation messages, and simple agent
interactions.

## Storage

The project does not require a paid database. Data is cached in memory and
mirrored to `Data/tele-rehab-data.json` after create, update, and delete
operations. Each record expires after at least one hour.

Configure the lifetime in `appsettings.json`:

```json
{
  "DataStore": {
    "FilePath": "Data/tele-rehab-data.json",
    "ItemLifetimeHours": 1
  }
}
```

This storage strategy is intended only for experiments and demonstrations. Do
not use it for production traffic or real medical records.

## Run Locally

Requirements:

- .NET 10 SDK

Start the API:

```bash
dotnet restore
dotnet run
```

Open the Swagger UI using the URL printed by the application, followed by
`/swagger`.

## Main Endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| `GET`, `POST` | `/api/patients` | List or create patients |
| `GET`, `POST` | `/api/therapies` | List or create therapies |
| `GET`, `POST` | `/api/therapylogs` | List or create therapy logs |
| `POST` | `/api/motivation/message` | Generate a motivation message |
| `POST` | `/api/agent/interact` | Get a simple agent response |

## Azure Deployment With Zero Cost

Use an Azure App Service plan with the `F1 Free` tier only. Do not add a paid
database, storage account, monitoring service, or paid App Service plan.

Configure these App Service application settings:

```text
DataStore__FilePath=/home/data/tele-rehab-data.json
DataStore__ItemLifetimeHours=1
```

For a Linux custom container, also configure:

```text
WEBSITES_ENABLE_APP_SERVICE_STORAGE=true
```

Before creating the Azure resource, confirm that the portal shows the `F1`
plan and an estimated monthly cost of `$0`. See
[`AZURE_DEPLOYMENT.md`](AZURE_DEPLOYMENT.md) for the deployment notes and
limitations.
