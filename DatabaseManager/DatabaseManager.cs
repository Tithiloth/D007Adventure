using D007Adventure.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace D007Adventure.DatabaseManager
{
    public class DatabaseManager : IDisposable
    {
        private readonly string databaseDirectory;

        private readonly Dictionary<string, IntPtr> databases =
            new Dictionary<string, IntPtr>();

        public DatabaseManager()
        {
            Logger.Info("DatabaseManager wird initialisiert.");

            databaseDirectory = Path.Combine(
                AppContext.BaseDirectory,
                "Database"
            );

            Logger.Info(
                $"Database-Pfad: {databaseDirectory}"
            );

            Directory.CreateDirectory(
                databaseDirectory
            );

            LoadDatabases();
        }

        public List<Dictionary<string, object?>> ExecuteReader(
            string databaseName,
            string sql)
        {
            if (!databases.TryGetValue(
                databaseName,
                out IntPtr database))
            {
                throw new Exception(
                    $"Datenbank '{databaseName}.db' ist nicht geladen."
                );
            }

            int result = SQLiteNative.sqlite3_prepare_v2(
                database,
                sql,
                -1,
                out IntPtr statement,
                IntPtr.Zero
            );

            if (result != 0)
            {
                throw new Exception(
                    $"SQL konnte nicht vorbereitet werden. " +
                    $"Fehlercode: {result}"
                );
            }

            try
            {
                List<Dictionary<string, object?>> rows =
                    new List<Dictionary<string, object?>>();

                int columnCount =
                    SQLiteNative.sqlite3_column_count(statement);

                while (true)
                {
                    int step =
                        SQLiteNative.sqlite3_step(statement);

                    if (step == 101)
                    {
                        break;
                    }

                    if (step != 100)
                    {
                        throw new Exception(
                            $"Fehler beim Lesen der Daten. " +
                            $"SQLite-Fehlercode: {step}"
                        );
                    }

                    Dictionary<string, object?> row =
                        new Dictionary<string, object?>();

                    for (
                        int column = 0;
                        column < columnCount;
                        column++)
                    {
                        string columnName =
                            Marshal.PtrToStringUTF8(
                                SQLiteNative.sqlite3_column_name(
                                    statement,
                                    column
                                )
                            ) ?? "";

                        int type =
                            SQLiteNative.sqlite3_column_type(
                                statement,
                                column
                            );

                        object? value = type switch
                        {
                            1 => SQLiteNative.sqlite3_column_int(
                                statement,
                                column
                            ),

                            2 => SQLiteNative.sqlite3_column_double(
                                statement,
                                column
                            ),

                            3 => Marshal.PtrToStringUTF8(
                                SQLiteNative.sqlite3_column_text(
                                    statement,
                                    column
                                )
                            ),

                            5 => null,

                            _ => null
                        };

                        row.Add(
                            columnName,
                            value
                        );
                    }

                    rows.Add(row);
                }

                return rows;
            }
            finally
            {
                SQLiteNative.sqlite3_finalize(
                    statement
                );
            }
        }

        public void ExecuteNonQuery(
        string databaseName,
        string sql)
        {
            if (!databases.TryGetValue(
                databaseName,
                out IntPtr database))
            {
                throw new Exception(
                    $"Datenbank '{databaseName}.db' " +
                    $"ist nicht geladen."
                    );
            }

            int result =
                SQLiteNative.sqlite3_prepare_v2(
                    database,
                    sql,
                    -1,
                    out IntPtr statement,
                    IntPtr.Zero
                    );

            if (result != 0)
            {
                string error =
                    Marshal.PtrToStringUTF8(
                        SQLiteNative.sqlite3_errmsg(
                            database
                        )
                    ) ?? "Unbekannter SQLite-Fehler";

                throw new Exception(
                    $"SQL konnte nicht vorbereitet werden: {error}"
                );
            }

            try
            {
                result =
                    SQLiteNative.sqlite3_step(
                        statement
                    );

                // SQLITE_DONE
                if (result != 101)
                {
                    string error =
                        Marshal.PtrToStringUTF8(
                            SQLiteNative.sqlite3_errmsg(
                                database
                            )
                        ) ?? "Unbekannter SQLite-Fehler";

                    throw new Exception(
                        $"SQL konnte nicht ausgeführt werden: {error}"
                    );
                }
            }
            finally
            {
                SQLiteNative.sqlite3_finalize(
                    statement
                );
            }
        }

        private void LoadDatabases()
        {
            string gameDatabasePath =
                Path.Combine(
                    databaseDirectory,
                    "game.db"
                    );

            // game.db ist immer die Root-Datenbank
            if (!File.Exists(gameDatabasePath))
            {
                Logger.Info(
                    "Root-Datenbank game.db nicht gefunden. Erstelle game.db."
                    );
            }

            LoadDatabase(
                gameDatabasePath,
                true
            );

            // Alle weiteren .db-Dateien laden
            string[] files =
                Directory.GetFiles(
                    databaseDirectory,
                    "*.db"
                    );

            foreach (string file in files)
            {
                string databaseName =
                    Path.GetFileNameWithoutExtension(file);

                // game.db wurde bereits als Root geladen
                if (databaseName.Equals(
                    "game",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                LoadDatabase(
                    file,
                    false
                );
            }
        }

        private void LoadDatabase(
        string file,
        bool isRoot)
        {
            string databaseName =
                Path.GetFileNameWithoutExtension(file);

            if (databases.ContainsKey(databaseName))
            {
                return;
            }

            int result =
                SQLiteNative.sqlite3_open(
                    file,
                    out IntPtr database
                );

            if (result != 0)
            {
                throw new Exception(
                    $"Datenbank '{databaseName}.db' " +
                    $"konnte nicht geöffnet werden. " +
                    $"SQLite-Fehlercode: {result}"
                    );
            }

            databases.Add(
                databaseName,
                database
            );

            if (isRoot)
            {
                Logger.Info(
                    $"Root-Datenbank geladen: {databaseName}.db"
                );
            }
            else
            {
                Logger.Info(
                    $"Untergeordnete Datenbank geladen: {databaseName}.db"
                );
            }
        }

        public void Update()
        {
            string[] files =
                Directory.GetFiles(
                    databaseDirectory,
                    "*.db"
                );

            foreach (string file in files)
            {
                string databaseName =
                    Path.GetFileNameWithoutExtension(file);

                if (databases.ContainsKey(databaseName))
                {
                    continue;
                }

                // game.db darf hier nicht als normale
                // untergeordnete Datenbank geladen werden
                if (databaseName.Equals(
                    "game",
                    StringComparison.OrdinalIgnoreCase))
                {
                    LoadDatabase(
                        file,
                        true
                    );

                    continue;
                }

                Logger.Info(
                    $"Neue untergeordnete Datenbank gefunden: " +
                    $"{databaseName}.db"
                );

                LoadDatabase(
                    file,
                    false
                );
            }
        }
        public List<string> GetTables(
            string databaseName)
        {
            List<string> tables =
                new List<string>();

            List<Dictionary<string, object?>> rows =
                ExecuteReader(
                    databaseName,
                            """
                    SELECT name
                    FROM sqlite_master
                    WHERE type = 'table'
                    AND name NOT LIKE 'sqlite_%'
                    ORDER BY name;
                    """
                   );

            foreach (Dictionary<string, object?> row in rows)
            {
                if (row["name"] is string tableName)
                {
                    tables.Add(tableName);
                }
            }

            return tables;
        }

        public List<Dictionary<string, object?>> GetColumns(
            string databaseName,
            string tableName)
        {
            return ExecuteReader(
                databaseName,
                $"PRAGMA table_info(\"{tableName}\");"
                );
        }

        public List<Tool> LoadTools()
        {
            List<Tool> tools = new List<Tool>();

            List<Dictionary<string, object?>> rows =
                ExecuteReader(
                    "Tools",
                    "SELECT * FROM Tools;"
                    );

            foreach (Dictionary<string, object?> row in rows)
            {
                Tool? tool = null;

                string type =
                    row.ContainsKey("Type")
                        ? row["Type"]?.ToString() ?? ""
                        : "";

                switch (type)
                {
                    case "Schaufel":
                        tool = new Schaufel();
                        break;

                    case "Messer":
                        tool = new Messer();
                        break;

                    case "Kessel":
                        tool = new Kessel();
                        break;

                    default:
                        Logger.Error(
                            $"Unbekannter Tool-Typ: {type}"
                        );
                        continue;
                }

                tool.Name =
                    row["Name"]?.ToString() ?? "default";

                tool.Damage =
                    Convert.ToInt32(row["Damage"]);

                tool.Tier =
                    Convert.ToInt32(row["Tier"]);

                tool.MaxTier =
                    Convert.ToInt32(row["MaxTier"]);

                tools.Add(tool);
            }

            return tools;
        }
        public void TestDatabase()
        {
            ExecuteNonQuery(
                "game",
                """
                CREATE TABLE IF NOT EXISTS Items (
                ItemID INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Damage INTEGER,
                Skin TEXT
                );
                """
                );

            Logger.Info(
                "Tabelle 'Items' wurde erstellt."
                );

            ExecuteNonQuery(
                "game",
                """
                INSERT OR IGNORE INTO Items
                (ItemID, Name, Damage, Skin)
                VALUES
                (
                    1,
                    'Holzschwert',
                    5,
                    'Assets/Skins/weapons/woodensword.png'
                );
                """
                );

            Logger.Info(
                "Test-Item wurde eingefügt."
                );
        }

        public void TestToolsDatabase()
        {
            if (!databases.ContainsKey("Tools"))
            {
                string toolsDatabasePath =
                    Path.Combine(
                        databaseDirectory,
                        "Tools.db"
                    );

                LoadDatabase(
                    toolsDatabasePath,
                    false
                );
            }

            ExecuteNonQuery(
                "Tools",
                """
                CREATE TABLE IF NOT EXISTS Tools
                (
                    ToolID INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    Damage INTEGER NOT NULL,
                    Tier INTEGER NOT NULL,
                    MaxTier INTEGER NOT NULL
                );
                """
                );

            Logger.Info("Tabelle 'Tools' wurde erstellt.");

            ExecuteNonQuery(
                "Tools",
                """
                INSERT OR IGNORE INTO Tools
                (ToolID, Name, Type, Damage, Tier, MaxTier)
                VALUES
                (1, 'Schaufel', 'Schaufel', 1, 0, 5);
                """
                );

            ExecuteNonQuery(
                "Tools",
                """
                INSERT OR IGNORE INTO Tools
                (ToolID, Name, Type, Damage, Tier, MaxTier)
                VALUES
                (2, 'Messer', 'Messer', 2, 0, 5);
                """
                );

            ExecuteNonQuery(
                "Tools",
                """
                INSERT OR IGNORE INTO Tools
                (ToolID, Name, Type, Damage, Tier, MaxTier)
                VALUES
                (3, 'Kessel', 'Kessel', 0, 0, 5);
                """
                );

            Logger.Info("Test-Tools wurden eingefügt.");
        }
        public void Dispose()
        {
            foreach (IntPtr database in databases.Values)
            {
                if (database != IntPtr.Zero)
                {
                    SQLiteNative.sqlite3_close(database);
                }
            }

            databases.Clear();

            Logger.Info(
                "Datenbanken wurden geschlossen."
            );
        }
    }
}