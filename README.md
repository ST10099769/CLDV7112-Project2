# Project 2 — ABCRetail.Functions

This is a **separate Azure Functions project** (`ABCRetail.Functions`) that
sits alongside your existing `ABCRetail.Web` project from Project 1. It
reuses the exact same:

- **Models** — `CustomerProfile`, `Product`, `OrderQueueMessage` (copied in
  under `Models/`, namespace changed to `ABCRetail.Functions.Models`)
- **Config keys** — `AzureStorage:ConnectionString`, `TableName`,
  `BlobContainerName`, `QueueName`, `FileShareName`, `FileShareDirectory`
- **Storage names** — table `AbcRetailRecords`, container `product-media`,
  queue `order-processing-queue`, file share `abcretaillogs`/`logs`

...so anything you write through these functions shows up immediately in
your Project 1 web app's Customers / Media / Order Queue / Logs pages, and
vice versa — same storage account, same data.

## 1. Recommended folder layout

Put it as a sibling of `ABCRetail.Web` inside your existing repo, and add
both projects to one solution:

```
ABCRetail/
  ABCRetail.Web/          <- Project 1 (unchanged)
  ABCRetail.Functions/     <- this project (Project 2)
  ABCRetail.sln
```

```bash
cd ABCRetail
dotnet new sln -n ABCRetail          # only if you don't already have a .sln
dotnet sln add ABCRetail.Web/ABCRetail.Web.csproj
dotnet sln add ABCRetail.Functions/ABCRetail.Functions.csproj
```

## 2. Prerequisites

- .NET 8 SDK
- Azure Functions Core Tools v4 (`npm i -g azure-functions-core-tools@4` or
  via the VS Code Azure Functions extension)
- The **same** Azure Storage Account you already created for Project 1

## 3. Configure `local.settings.json`

A template is already included. Replace both `UseDevelopmentStorage=true`
placeholders with your real storage connection string (same one you used
for `AzureStorage:ConnectionString` in Project 1's user secrets):

```json
"AzureStorageConnection": "<your-connection-string>",
"AzureStorage__ConnectionString": "<your-connection-string>",
```

Two separate settings are needed because the queue *trigger* binding
(`ProcessOrderMessage.cs`) requires a **flat** app setting name
(`AzureStorageConnection`), while the other functions read the connection
string through `IConfiguration` using the same nested
`AzureStorage:ConnectionString` style as your web app (the `__` double
underscore is how flat env-var names map to nested config keys).

`local.settings.json` is already covered by the standard Functions
`.gitignore` — do **not** commit your real connection string.

## 4. Run it locally

```bash
cd ABCRetail.Functions
dotnet restore
func start
```

You'll get four HTTP endpoints plus one background queue-triggered function,
e.g.:

```
http://localhost:7071/api/StoreCustomerToTable
http://localhost:7071/api/UploadProductImage
http://localhost:7071/api/SendOrderMessage
http://localhost:7071/api/UploadOrderFile
```

`ProcessOrderMessage` has no URL — it fires automatically whenever a message
lands in the queue.

## 5. Test each function (see sample request bodies in each .cs file)

Example with curl:

```bash
curl -X POST http://localhost:7071/api/StoreCustomerToTable \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Thabo Mokoena","email":"thabo@example.com","phone":"0821234567","shippingAddress":"12 Main Rd, Johannesburg"}'
```

Then open your **Project 1 web app locally** and check the Customers page —
the new record should already be there, proving both projects share the
same storage account.

Do the same for:
- `UploadProductImage` → check the Media page
- `SendOrderMessage` → check the Order Queue page, and watch the terminal
  for `ProcessOrderMessage`'s log line firing automatically
- `UploadOrderFile` → check the Logs page

📸 **Screenshot opportunities (per rubric row):**
- Each function's code open in your editor
- Each function running (terminal output showing the trigger + success log)
- The corresponding data visible in Azure Storage Explorer / Portal Storage
  browser (row in the table, blob in the container, message in the queue,
  file in the share)

## 6. Deploy to Azure

1. In the Azure Portal, create a **Function App** (separate from your App
   Service) — Consumption plan is fine for a student project, same region
   as your storage account.
2. Set its **Configuration → Application settings**:
   - `AzureStorageConnection` = your connection string
   - `AzureStorage__ConnectionString` = your connection string
   - `AzureStorage__TableName` = `AbcRetailRecords`
   - `AzureStorage__BlobContainerName` = `product-media`
   - `AzureStorage__QueueName` = `order-processing-queue`
   - `AzureStorage__FileShareName` = `abcretaillogs`
   - `AzureStorage__FileShareDirectory` = `logs`
3. Publish:
   ```bash
   func azure functionapp publish <your-function-app-name>
   ```
4. Re-test each endpoint against the live
   `https://<function-app-name>.azurewebsites.net/api/...` URLs, and confirm
   the data still shows up in your deployed Project 1 web app.

## 7. What goes in your Word doc for Project 2

- Screenshots per rubric row (function code + function app run + resulting
  data in storage) for all four functions
- The Event Hubs vs Event Bus discussion (see
  `Discussion-EventHubs-EventBus.md`)
- Your deployed web app link, GitHub link, student number, module code —
  same as Project 1's requirements, per the brief
