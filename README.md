# 🍴 Objednávací systém v menze (UTB Minute)

Semestrální projekt do předmětu **Aplikační frameworky**.

## 👥 Členové týmu a poměr práce
| Jméno a příjmení | Role v týmu | Poměr práce |
|:---|:---|:---:|
| **Šik** - vedoucí | Datový model, DbManager, Aspire orchestrace, Keycloak integrace, AdminClient | 1 |
| **Martoňák** | DTO Contracts, WebAPI endpointy, Integrační testy, CanteenClient, SSE real-time aktualizace | 1 |

*Poznámka: Poměr práce `1:1` značí rovnoměrný přínos obou členů (50 % / 50 %). Veškeré požadavky pro finální odevzdání byly splněny.*

---

## 🚀 Spuštění projektu

1. **Požadavky:** .NET 10 SDK, Docker Desktop (nutný pro běh PostgreSQL a Keycloak v Aspire).
2. **Postup:**
   - Spusťte Docker Desktop.
   - Otevřete solution `UTB.Minute.sln` ve Visual Studiu 2026.
   - Nastavte projekt `UTB.Minute.AppHost` jako **Start-up projekt**.
   - Spusťte projekt (klávesou **F5**).
   - V prohlížeči se otevře **.NET Aspire Dashboard**, kde uvidíte stav všech služeb (databáze, webapi, dbmanager, keycloak, adminclient, canteenclient).
   - **Testovací přihlašovací údaje:**
     - Administrátor: jméno `admin`, heslo `admin`
     - Kuchařka: jméno `cook`, heslo `cook`

---

## 📂 Struktura řešení
Projekt implementuje celou full-stack architekturu v .NET:

- `UTB.Minute.AppHost`: Aspire orchestrace (spouští PostgreSQL, Keycloak, API a frontendové klienty).
- `UTB.Minute.Db`: Datové entity (`Meal`, `MenuItem`, `Order`) a `DbContext`.
- `UTB.Minute.DbManager`: Obsahuje endpoint `/dev/seed` pro **Http Command** (reset databáze a seed testovacích dat).
- `UTB.Minute.Contracts`: Sdílená DTO, aby byla zajištěna typová bezpečnost mezi vrstvami.
- `UTB.Minute.WebApi`: Hlavní byznys logika a REST endpointy pro Jídla, Menu a Objednávky. Přenáší SSE události klientským aplikacím.
- `UTB.Minute.WebApi.Tests`: Integrační testy (xUnit) pokrývající funkčnost API.
- `UTB.Minute.AdminClient`: Blazor Server aplikace pro administrátory (správa jídel, menu).
- `UTB.Minute.CanteenClient`: Blazor Server aplikace pro studenty (objednávání) a kuchařky (správa stavu objednávek).

---

## 🛠️ Klíčová implementační rozhodnutí

### 1. Autorizace a Keycloak
Projekt využívá **Keycloak** kontejner spravovaný přes Aspire. Klienti se autentizují přes OIDC (OpenID Connect). Aplikace rozlišuje role (`admin`, `cook`), které omezují přístup ke specifickým stránkám v klientských aplikacích (Blazor komponenty) a odesílají Bearer tokeny při volání API.

### 2. Real-time notifikace (SSE)
Pro obousměrnou komunikaci a okamžité aktualizace UI bez nutnosti obnovovat stránku využíváme **Server-Sent Events (SSE)**.
Při jakékoli změně v backendu (např. objednání porce, změna stavu jídla) WebAPI generuje událost, na kterou naslouchají Blazor Server klienti (`AdminClient`, `CanteenClient`) a dynamicky tak překreslují rozhraní (`StateHasChanged`).

### 3. Optimistická konkurence
- **Ochrana porcí:** Zabránění tomu, aby si dva studenti objednali poslední porci zároveň. Implementováno pomocí DbUpdateConcurrencyException při souběžném zápisu.

### 4. Business pravidla
- **Ochrana porcí:** Při vytvoření objednávky (`POST /api/orders`) se okamžitě ověřuje dostupnost porcí (`AvailablePortions > 0`) a jejich počet se ihned snižuje.
- **Změna stavů:** Validace nedovoluje obnovit objednávku, která již byla označena jako zrušená (`Cancelled`). Změny probíhají přes dedikované DTO (`OrderStatePatchDto`).
- **Oddělení DbManageru:** Logika pro manipulaci se schématem DB je izolovaná.

---

## 📝 Poznámky k odevzdání
* **Stav:** Projekt je kompletní pro finální odevzdání. Obě Blazor aplikace (Admin a Canteen) jsou plně responzivní a integrované.
* **Testování:** Integrační testy v `UTB.Minute.WebApi.Tests` využívají testovací databázi přes Aspire a pokrývají CRUD cyklus. Testy jsou nezávislé a dynamicky tvoří data bez pevných ID.
* **Problémy:** Během vývoje jsme řešili synchronizaci přístupu ke Keycloak autoritě a sdílení JWT tokenů mezi interaktivními okruhy Blazor Serveru. Bylo také nutné ošetřit pre-rendering u SSE (`TaskCanceledException`), což jsme vyřešili inicializací SSE spojení výhradně až po vykreslení.

---

## 🧪 Seznam API endpointů (ukázka)
* `POST /dev/seed` *(DbManager)* - Reset a naplnění databáze.
* `GET /api/meals` - Seznam všech jídel.
* `POST /api/menuitems` - Vytvoření položky menu na konkrétní den.
* `POST /api/orders` - Vytvoření nové objednávky (student).
* `PATCH /api/orders/{id}/state` - Změna stavu objednávky (kuchařka).
* `GET /api/sse` - Stream pro Server-Sent Events.