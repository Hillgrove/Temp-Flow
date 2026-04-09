# Temp-Flow

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![EF Core](https://img.shields.io/badge/Entity_Framework_Core-9.0-512BD4?logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL_Server-MSSQL-CC2927?logo=microsoftsqlserver)
![Vue.js](https://img.shields.io/badge/Vue.js-3.x-42B883?logo=vuedotjs)
![Python](https://img.shields.io/badge/Python-3.x-3776AB?logo=python)

Temp-Flow er et IoT-baseret overvågningssystem til indeklima. Raspberry Pi-enheder med Sense HAT-sensorer indsamler temperatur, luftfugtighed og lufttryk og sender dataene via UDP til en central backend. Backenden gemmer målingerne i en SQL Server-database og stiller dem til rådighed via en REST API. Et webbaseret frontend giver brugeren et overblik over rum og sensorer med realtidsdata og historiske grafer.

---

## Indholdsfortegnelse

- [Temp-Flow](#temp-flow)
  - [Indholdsfortegnelse](#indholdsfortegnelse)
  - [Funktioner](#funktioner)
  - [Teknologier](#teknologier)
  - [Arkitektur](#arkitektur)
  - [Kom i gang](#kom-i-gang)
    - [Forudsætninger](#forudsætninger)
    - [Installation af backend](#installation-af-backend)
    - [Databaseopsætning](#databaseopsætning)
    - [Start af services](#start-af-services)
    - [Opsætning af Raspberry Pi-klient](#opsætning-af-raspberry-pi-klient)
    - [Kørsel af frontend](#kørsel-af-frontend)
  - [API-oversigt](#api-oversigt)
  - [Projektstruktur](#projektstruktur)
  - [Test](#test)

---

## Funktioner

- Indsamling af sensordata (temperatur, luftfugtighed, lufttryk) via Raspberry Pi Sense HAT
- Realtidstransmission af sensordata over UDP
- Automatisk oprettelse af sensorer i databasen ved første registrering
- REST API med fuld CRUD for rum og sensorer
- Tildeling af sensorer til rum
- Historisk visning af sensordata grupperet pr. time
- Sletning af sensordata ældre end et angivet antal dage
- Webbaseret dashboard med temperaturgrafer (Chart.js)
- Swagger/OpenAPI-dokumentation tilgængelig i alle miljøer
- Emulator til lokal udvikling uden fysisk Sense HAT-hardware

---

## Teknologier

| Komponent         | Teknologi                                   |
| ----------------- | ------------------------------------------- |
| Backend API       | ASP.NET Core 8, C#                          |
| Datalag           | Entity Framework Core 9, Repository-pattern |
| Database          | Microsoft SQL Server (MSSQL)                |
| UDP-server        | .NET 8 konsol-applikation                   |
| Sensor-klient     | Python 3, Raspberry Pi Sense HAT            |
| Frontend          | Vue.js 3, Bootstrap 5, Axios, Chart.js      |
| API-dokumentation | Swagger / OpenAPI (Swashbuckle)             |
| Test              | xUnit, Selenium WebDriver                   |

---

## Arkitektur

```mermaid
flowchart LR
    RPI["Raspberry Pi / Sense HAT"]
    UDP["UDP-Server (port 65000)"]
    DB[("SQL Server")]
    API["REST API"]
    FE["Frontend (Vue.js)"]

    RPI -->|"UDP / JSON"| UDP
    UDP -->|"EF Core"| DB
    DB  -->|"EF Core"| API
    API -->|"HTTP / JSON"| FE
```

1. **Raspberry Pi-klienten** (`UDP_client.py`) aflæser sensoren hvert minut og sender dataene som JSON over UDP.
2. **UDP-Serveren** modtager pakken, opretter sensoren i databasen hvis den ikke kendes, og gemmer målingen.
3. **REST API'en** udstiller endpoints til rum- og sensoradministration samt datahentning.
4. **Frontend** kalder API'en og viser temperaturdata, afvigelse fra måltemperatur og historiske grafer.

---

## Kom i gang

### Forudsætninger

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Microsoft SQL Server (lokal instans eller ekstern server)
- Python 3.x (til Raspberry Pi-klienten)
- En webbrowser med en lokal HTTP-server (f.eks. Live Server i VS Code) til frontend

### Installation af backend

1. Klon repositoriet:
   ```bash
   git clone <repository-url>
   cd Temp-Flow/Backend
   ```

2. Åbn løsningen `Backend.sln` i Visual Studio eller byg via CLI:
   ```bash
   dotnet build
   ```

### Databaseopsætning

Forbindelsesstrengen konfigureres i [Backend/DataAccess/EnvironmentSecrets.cs](Backend/DataAccess/EnvironmentSecrets.cs). Opdater `_dbConnectionString` med din egen SQL Server-forbindelsesstreng:

```csharp
private string _dbConnectionString = "Server=<server>;Database=<database>;User Id=<user>;Password=<password>;MultipleActiveResultSets=true;";
```

Kør EF Core-migrations for at oprette databaseskemaet:

```bash
cd Backend/DataAccess
dotnet ef database update
```

### Start af services

Start UDP-Serveren (lytter pa port 65000):
```bash
cd Backend/UDP-Server
dotnet run
```

Start REST API'en:
```bash
cd Backend/REST_API
dotnet run
```

Swagger UI er tilgængelig på `http://localhost:<port>/swagger`.

### Opsætning af Raspberry Pi-klient

1. Installer afhængigheder (pa Raspberry Pi eller i virtuelt miljø):
   ```bash
   cd Raspberry_Pi
   pip install sense-hat
   ```

2. Konfigurer `UDP_client.py` — sæt `BROADCAST_IP` til serverens IP-adresse og `DEVELOPMENT_MODE = False` ved kørsel pa rigtig hardware:
   ```python
   DEVELOPMENT_MODE = False
   BROADCAST_IP = "<server-ip>"
   PORT = 65000
   ```

3. Kør klienten:
   ```bash
   python UDP_client.py
   ```

   Under udvikling bruges `DEVELOPMENT_MODE = True`, som aktiverer den medfølgende SenseHat-emulator.

### Kørsel af frontend

Åbn `Frontend/`-mappen med en lokal HTTP-server, f.eks. via Live Server i VS Code. API-base-URL'en konfigureres øverst i de enkelte JavaScript-filer:

```javascript
const baseUrl = 'http://localhost:<port>/api/';
```

---

## API-oversigt

| Metode | Endpoint                               | Beskrivelse                                  |
| ------ | -------------------------------------- | -------------------------------------------- |
| GET    | `/api/Rooms`                           | Hent alle rum med sensorer og data           |
| POST   | `/api/Rooms`                           | Opret nyt rum                                |
| GET    | `/api/Rooms/{id}`                      | Hent enkelt rum                              |
| PUT    | `/api/Rooms/{id}`                      | Opdater rum                                  |
| DELETE | `/api/Rooms/{id}`                      | Slet rum                                     |
| POST   | `/api/Rooms/{id}/addsensor/{sensorId}` | Tildel sensor til rum                        |
| GET    | `/api/Rooms/{id}/data/recent`          | Nyeste sensordata for rum grupperet pr. time |
| GET    | `/api/Sensors`                         | Hent alle sensorer                           |
| GET    | `/api/Sensors/{id}`                    | Hent enkelt sensor                           |
| GET    | `/api/Sensors/{id}/data`               | Hent alle data for sensor                    |
| GET    | `/api/Sensors/{id}/grouped-by-hour`    | Sensordata grupperet pr. time                |
| PUT    | `/api/Sensors/{id}`                    | Opdater sensor                               |
| DELETE | `/api/Sensors/{id}`                    | Slet sensor                                  |
| GET    | `/api/SensorData`                      | Hent alle sensordata                         |
| GET    | `/api/SensorData/{id}`                 | Hent enkelt sensordata-post                  |
| GET    | `/api/SensorData/recent/{days}`        | Hent data fra de seneste N dage              |
| DELETE | `/api/SensorData/older-than/{days}`    | Slet data ældre end N dage                   |

---

## Projektstruktur

```
Temp-Flow/
├── Backend/                        # .NET 8-løsning
│   ├── REST_API/                   # ASP.NET Core Web API
│   │   ├── Controllers/            # RoomsController, SensorsController, SensorDataController
│   │   └── Program.cs              # Konfiguration og DI-registrering
│   ├── UDP-Server/                 # Konsol-applikation til modtagelse af UDP-pakker
│   │   └── Program.cs              # Lytter pa port 65000 og gemmer data i databasen
│   ├── DataAccess/                 # Datalag
│   │   ├── Models/                 # Room, Sensor, SensorData
│   │   ├── DTOs/                   # Dataoverførselsobjekter
│   │   ├── Interfaces/             # IRoomRepository, ISensorRepository
│   │   ├── Repositories/          # Implementationer af repositories
│   │   └── AppDbContext.cs         # EF Core DbContext
│   ├── REST_APITests/              # Unit tests for API-controllers
│   └── DataAccessTests/            # Repository-tests og Selenium UI-tests
├── Frontend/                       # Webapplikation
│   ├── index.html                  # Dashboard
│   ├── rooms.html                  # Rumoversigt og administration
│   ├── sensors.html                # Sensorliste
│   ├── css/                        # Stylesheets
│   └── js/                         # Vue.js applikationslogik
└── Raspberry_Pi/                   # Python sensor-klient
    ├── UDP_client.py               # Sender sensordata via UDP
    └── sense_HAT_emulator.py       # SenseHat-emulator til lokal udvikling
```

---

## Test

Projektet indeholder to testprojekter:

**REST_APITests** — Unit tests for API-controllers med mockede repositories:
```bash
cd Backend
dotnet test REST_APITests/
```

**DataAccessTests** — Tests af repository-laget samt Selenium-baserede UI-tests:
```bash
cd Backend
dotnet test DataAccessTests/
```
