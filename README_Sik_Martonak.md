# 🍴 Objednávací systém v menze (UTB Minute)

Semestrální projekt do předmětu **Aplikační frameworky**.

## 👥 Členové týmu a poměr práce
| Jméno a příjmení | Role v týmu | Poměr práce |
|:---|:---|:---:|
| **Šik** - vedoucí | Datový model, DbManager, Aspire orchestrace | 1 |
| **Martoňák** | DTO Contracts, WebAPI endpointy, Integrační testy | 1 |

*Poznámka: Poměr práce `1:1` značí rovnoměrný přínos obou členů (50 % / 50 %). Veškeré požadavky pro půlsemestrální odevzdání byly splněny.*

---

## 🚀 Spuštění projektu

1. **Požadavky:** .NET 10 SDK, Docker Desktop nebo Podman (nutný pro běh PostgreSQL databáze v Aspire).
2. **Postup:**
   - Spusťte Docker Desktop nebo Podman.
   - Otevřete solution `UTB.Minute.sln` ve Visual Studiu 2026.
   - Nastavte projekt `UTB.Minute.AppHost` jako **Start-up projekt**.
   - Spusťte projekt (klávesou **F5**).
   - V prohlížeči se otevře **.NET Aspire Dashboard**, kde uvidíte stav všech služeb (databáze, webapi, dbmanager).

---

## 📂 Struktura řešení
Projekty pro frontendové klienty (AdminClient, CanteenClient) budou doplněny ve finálním odevzdání. Aktuální struktura backendu se skládá z 6 projektů:

- `UTB.Minute.AppHost`: Aspire orchestrace (spouští PostgreSQL a API).
- `UTB.Minute.Db`: Datové entity (`Meal`, `MenuItem`, `Order`) a `DbContext`.
- `UTB.Minute.DbManager`: Obsahuje endpoint `/dev/seed` pro **Http Command** (reset databáze a seed testovacích dat).
- `UTB.Minute.Contracts`: Sdílená DTO, aby byla zajištěna typová bezpečnost mezi vrstvami.
- `UTB.Minute.WebApi`: Hlavní byznys logika a REST endpointy pro Jídla, Menu a Objednávky.
- `UTB.Minute.WebApi.Tests`: Integrační testy (xUnit) pokrývající funkčnost API.

---

## 🛠️ Klíčová implementační rozhodnutí

### 1. Autorizace a Keycloak
*Bude implementováno ve finální fázi projektu (klientská část).*

### 2. Real-time notifikace (SSE)
*Bude implementováno ve finální fázi projektu.*

### 3. Business pravidla (Backend - Půlsemestrální stav)
- **Ochrana porcí:** Při vytvoření objednávky (`POST /api/orders`) se okamžitě ověřuje dostupnost porcí (`AvailablePortions > 0`) a jejich počet se ihned snižuje.
- **Změna stavů:** Validace nedovoluje obnovit objednávku, která již byla označena jako zrušená (`Cancelled`). Změny probíhají bezpečně přes dedikované DTO (`OrderStatePatchDto`) a metodu `PATCH`.
- **Oddělení DbManageru:** Z důvodu čistoty architektury je logika pro manipulaci se schématem DB přesunuta do izolované mikroslužby.

---

## 📝 Poznámky k odevzdání
* **Stav:** Backendová část pro půlsemestrální odevzdání je plně funkční a splňuje všechny požadavky.
* **Testování:** Integrační testy v `UTB.Minute.WebApi.Tests` využívají testovací databázi přes Aspire. Pokrývají CRUD cyklus pro Jídla, Menu i Objednávky. Testy jsou napsány tak, aby byly plně nezávislé (dynamicky si tvoří vlastní testovací data bez vázání na pevná ID).
* **Problémy:** Během vývoje jsme museli řešit správné nasdílení kontextu databáze mezi jednotlivými xUnit testy. Problém byl vyřešen vytvořením dedikované třídy `DatabaseCollection` a `TestFixture`.

---

## 🧪 Seznam API endpointů (ukázka)
* `POST /dev/seed` *(DbManager)* - Reset a naplnění databáze.
* `GET /api/meals` - Seznam všech jídel.
* `POST /api/menuitems` - Vytvoření položky menu na konkrétní den.
* `POST /api/orders` - Vytvoření nové objednávky (student).
* `PATCH /api/orders/{id}/state` - Změna stavu objednávky (kuchařka).