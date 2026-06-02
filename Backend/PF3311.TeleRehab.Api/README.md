# PF3311 TeleRehab API

Experimental ASP.NET Core API for a telerehabilitation prototype. It registers
patients, generates therapy exercises with OpenAI, stores therapy logs, and
provides motivation messages.

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
export OpenAI__ApiKey="your-api-key"
dotnet restore
dotnet run
```

Open the Swagger UI using the URL printed by the application, followed by
`/swagger`.

## Main Endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| `POST` | `/api/patients` | Register a patient |
| `GET` | `/api/patients/{patientId}` | Check whether a registered patient still exists |
| `POST` | `/api/therapies/generate/{patientId}` | Generate and store 5 to 7 exercises with OpenAI |
| `GET` | `/api/therapies/patient/{patientId}` | List a patient's exercises |
| `POST` | `/api/therapylogs` | Register exercise progress |
| `POST` | `/api/motivation/message` | Generate a personalized motivation message with OpenAI |
| `POST` | `/api/speech/synthesize` | Generate an MP3 message with Azure Speech |

The OpenAI request sends only the age, sex, condition, and technology level
needed to generate the plan. It uses the Responses API with Structured Outputs
and `store: false`. The API validates that at least five exercises were
returned before storing them. This prototype is not appropriate for real
medical records, and generated exercises require clinician review before use.

Motivation messages are also generated with OpenAI and `store: false`. The
client sends only the patient and therapy IDs. The API builds the personalized
context from the stored patient record, exercise, and five most recent progress
logs.

Speech synthesis uses Azure Speech and returns an MP3 file. Send `text` and the
patient's `sex` (`M` or `F`) to `/api/speech/synthesize`. The API selects
`es-CR-JuanNeural` for `M` and `es-CR-MariaNeural` for `F`. Keep the text free
of personal or medical details unless they are strictly necessary.

## Azure Deployment With Zero Cost

Use an Azure App Service plan with the `F1 Free` tier only. Do not add a paid
database, storage account, monitoring service, or paid App Service plan.

Configure these App Service application settings:

```text
DataStore__FilePath=/home/data/tele-rehab-data.json
DataStore__ItemLifetimeHours=1
OpenAI__ApiKey=<your-api-key>
OpenAI__Model=gpt-5.4-mini
AzureSpeech__Key=<your-speech-key>
AzureSpeech__Region=eastus
AzureSpeech__MaleVoiceName=es-CR-JuanNeural
AzureSpeech__FemaleVoiceName=es-CR-MariaNeural
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
```

For a Linux custom container, also configure:

```text
WEBSITES_ENABLE_APP_SERVICE_STORAGE=true
```

Before creating the Azure resource, confirm that the portal shows the `F1`
plan and an estimated monthly cost of `$0`. See
[`AZURE_DEPLOYMENT.md`](AZURE_DEPLOYMENT.md) for the deployment notes and
limitations.
