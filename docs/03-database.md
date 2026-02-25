# Baza danych i systemy zaawansowane

## Aktualna konfiguracja MySQL

Docker Compose automatycznie uruchamia MySQL:
- Host: `cs2-db` (z kontenera) / `localhost` (z hosta)
- Port: `3306`
- User: `server` / `root`
- Password: `changeme` (ZMIEŃ W PRODUKCJI!)
- Database: `db`

## Tabele dla COD Mod

```sql
-- Tabela graczy
CREATE TABLE players (
    id INT AUTO_INCREMENT PRIMARY KEY,
    steam_id VARCHAR(32) UNIQUE NOT NULL,
    name VARCHAR(64),
    xp INT DEFAULT 0,
    level INT DEFAULT 1,
    class VARCHAR(32) DEFAULT 'assault',
    kills INT DEFAULT 0,
    deaths INT DEFAULT 0,
    headshots INT DEFAULT 0,
    playtime INT DEFAULT 0,
    vip_until DATETIME NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_seen TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_steam (steam_id),
    INDEX idx_xp (xp DESC)
);

-- Tabela VIP
CREATE TABLE vip_players (
    id INT AUTO_INCREMENT PRIMARY KEY,
    steam_id VARCHAR(32) NOT NULL,
    tier ENUM('bronze', 'silver', 'gold') NOT NULL,
    expires_at DATETIME NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_steam (steam_id)
);

-- Tabela banów
CREATE TABLE bans (
    id INT AUTO_INCREMENT PRIMARY KEY,
    steam_id VARCHAR(32) NOT NULL,
    admin_steam_id VARCHAR(32),
    reason VARCHAR(255),
    duration INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at DATETIME,
    active BOOLEAN DEFAULT TRUE,
    INDEX idx_steam (steam_id)
);
```

## Połączenie z C#

### Instalacja MySqlConnector
```bash
dotnet add package MySqlConnector
```

### DatabaseService.cs
```csharp
using MySqlConnector;

namespace CodMod.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string host, string database, string user, string password)
    {
        _connectionString = $"Server={host};Database={database};User={user};Password={password};";
    }

    public async Task<PlayerData?> GetPlayerAsync(ulong steamId)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(
            "SELECT * FROM players WHERE steam_id = @steamId", connection);
        command.Parameters.AddWithValue("@steamId", steamId.ToString());

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new PlayerData
            {
                SteamId = reader.GetString("steam_id"),
                Name = reader.GetString("name"),
                XP = reader.GetInt32("xp"),
                Level = reader.GetInt32("level"),
                Class = reader.GetString("class")
            };
        }
        return null;
    }

    public async Task SavePlayerAsync(PlayerData player)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(@"
            INSERT INTO players (steam_id, name, xp, level, class, kills, deaths, headshots)
            VALUES (@steamId, @name, @xp, @level, @class, @kills, @deaths, @headshots)
            ON DUPLICATE KEY UPDATE
                name = @name, xp = @xp, level = @level, class = @class,
                kills = @kills, deaths = @deaths, headshots = @headshots,
                last_seen = NOW()
        ", connection);

        command.Parameters.AddWithValue("@steamId", player.SteamId);
        command.Parameters.AddWithValue("@name", player.Name);
        command.Parameters.AddWithValue("@xp", player.XP);
        command.Parameters.AddWithValue("@level", player.Level);
        command.Parameters.AddWithValue("@class", player.Class);
        command.Parameters.AddWithValue("@kills", player.Kills);
        command.Parameters.AddWithValue("@deaths", player.Deaths);
        command.Parameters.AddWithValue("@headshots", player.Headshots);

        await command.ExecuteNonQueryAsync();
    }
}
```

## System rang

```csharp
public static class Ranks
{
    public static readonly Dictionary<int, RankInfo> All = new()
    {
        [1] = new("Rekrut", 0),
        [2] = new("Szeregowy", 500),
        [3] = new("Starszy Szeregowy", 1200),
        [4] = new("Kapral", 2000),
        [5] = new("Sierżant", 4500),
        [6] = new("Chorąży", 9000),
        [7] = new("Porucznik", 15500),
        [8] = new("Kapitan", 20000),
        [9] = new("Major", 25000),
        [10] = new("Pułkownik", 38000),
        [11] = new("Generał", 55000),
        [12] = new("Legenda", 100000)
    };
    
    public static int GetLevel(int xp)
    {
        for (int i = All.Count; i >= 1; i--)
            if (xp >= All[i].RequiredXP) return i;
        return 1;
    }
}

public record RankInfo(string Name, int RequiredXP);
```
