// Ruta: Data/DatabaseHelper.cs
using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace GestorMantenimientoVehicular.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public string DatabasePath { get; }

        public DatabaseHelper(string? databasePath = null)
        {
            DatabasePath = string.IsNullOrWhiteSpace(databasePath)
                ? Path.Combine(AppContext.BaseDirectory, "mantenimiento_vehicular.db")
                : databasePath;

            string? directory = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = DatabasePath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            InicializarBaseDatos();
        }

        public SqliteConnection ObtenerConexion()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public void InicializarBaseDatos()
        {
            using var connection = ObtenerConexion();

            const string sqlComponentes = @"
                CREATE TABLE IF NOT EXISTS Componentes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TipoComponente TEXT NOT NULL,
                    Nombre TEXT NOT NULL,
                    KilometrajeInstalacion INTEGER NOT NULL CHECK (KilometrajeInstalacion >= 0),
                    VidaUtilKm INTEGER NOT NULL CHECK (VidaUtilKm > 0),
                    TipoCombustible TEXT NOT NULL,
                    CostoRepuesto REAL NOT NULL CHECK (CostoRepuesto >= 0),
                    FechaRegistro TEXT NOT NULL DEFAULT (datetime('now', 'localtime'))
                );";

            using (var cmdComponentes = new SqliteCommand(sqlComponentes, connection))
            {
                cmdComponentes.ExecuteNonQuery();
            }

            const string sqlConfiguracion = @"
                CREATE TABLE IF NOT EXISTS ConfiguracionVehiculo (
                    Clave TEXT PRIMARY KEY,
                    Valor TEXT NOT NULL
                );";

            using (var cmdConfig = new SqliteCommand(sqlConfiguracion, connection))
            {
                cmdConfig.ExecuteNonQuery();
            }

            const string sqlDefaultKm = @"
                INSERT OR IGNORE INTO ConfiguracionVehiculo (Clave, Valor)
                VALUES (@clave, @valor);";

            using (var cmdDefault = new SqliteCommand(sqlDefaultKm, connection))
            {
                cmdDefault.Parameters.AddWithValue("@clave", "KilometrajeActual");
                cmdDefault.Parameters.AddWithValue("@valor", "0");
                cmdDefault.ExecuteNonQuery();
            }
        }
    }
}