// Ruta: Data/ComponenteDAO.cs
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GestorMantenimientoVehicular.Models;

namespace GestorMantenimientoVehicular.Data
{
    public class ComponenteDAO
    {
        private readonly DatabaseHelper _dbHelper;

        public ComponenteDAO(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }

        public int Insertar(ComponenteVehicular componente)
        {
            if (componente is null)
                throw new ArgumentNullException(nameof(componente));

            const string sql = @"
                INSERT INTO Componentes
                    (TipoComponente, Nombre, KilometrajeInstalacion, VidaUtilKm, TipoCombustible, CostoRepuesto)
                VALUES
                    (@tipoComponente, @nombre, @kmInstalacion, @vidaUtilKm, @tipoCombustible, @costoRepuesto);
                SELECT last_insert_rowid();";

            using var connection = _dbHelper.ObtenerConexion();
            using var cmd = new SqliteCommand(sql, connection);

            cmd.Parameters.AddWithValue("@tipoComponente", componente.TipoComponente);
            cmd.Parameters.AddWithValue("@nombre", componente.Nombre);
            cmd.Parameters.AddWithValue("@kmInstalacion", componente.KilometrajeInstalacion);
            cmd.Parameters.AddWithValue("@vidaUtilKm", componente.VidaUtilKm);
            cmd.Parameters.AddWithValue("@tipoCombustible", componente.TipoCombustible);
            cmd.Parameters.AddWithValue("@costoRepuesto", componente.CostoRepuesto);

            object? result = cmd.ExecuteScalar();
            int nuevoId = Convert.ToInt32(result);
            componente.Id = nuevoId;
            return nuevoId;
        }

        public List<ComponenteVehicular> ObtenerTodos()
        {
            var lista = new List<ComponenteVehicular>();

            const string sql = @"
                SELECT Id, TipoComponente, Nombre, KilometrajeInstalacion, VidaUtilKm, TipoCombustible, CostoRepuesto
                FROM Componentes
                ORDER BY Id DESC;";

            using var connection = _dbHelper.ObtenerConexion();
            using var cmd = new SqliteCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(MapearComponente(reader));
            }

            return lista;
        }

        public ComponenteVehicular? ObtenerPorId(int id)
        {
            const string sql = @"
                SELECT Id, TipoComponente, Nombre, KilometrajeInstalacion, VidaUtilKm, TipoCombustible, CostoRepuesto
                FROM Componentes
                WHERE Id = @id;";

            using var connection = _dbHelper.ObtenerConexion();
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return MapearComponente(reader);
            }

            return null;
        }

        public bool Actualizar(ComponenteVehicular componente)
        {
            if (componente is null)
                throw new ArgumentNullException(nameof(componente));

            if (componente.Id <= 0)
                throw new ArgumentException("El componente debe tener un Id válido para actualizar.", nameof(componente));

            const string sql = @"
                UPDATE Componentes
                SET TipoComponente = @tipoComponente,
                    Nombre = @nombre,
                    KilometrajeInstalacion = @kmInstalacion,
                    VidaUtilKm = @vidaUtilKm,
                    TipoCombustible = @tipoCombustible,
                    CostoRepuesto = @costoRepuesto
                WHERE Id = @id;";

            using var connection = _dbHelper.ObtenerConexion();
            using var cmd = new SqliteCommand(sql, connection);

            cmd.Parameters.AddWithValue("@id", componente.Id);
            cmd.Parameters.AddWithValue("@tipoComponente", componente.TipoComponente);
            cmd.Parameters.AddWithValue("@nombre", componente.Nombre);
            cmd.Parameters.AddWithValue("@kmInstalacion", componente.KilometrajeInstalacion);
            cmd.Parameters.AddWithValue("@vidaUtilKm", componente.VidaUtilKm);
            cmd.Parameters.AddWithValue("@tipoCombustible", componente.TipoCombustible);
            cmd.Parameters.AddWithValue("@costoRepuesto", componente.CostoRepuesto);

            int filasAfectadas = cmd.ExecuteNonQuery();
            return filasAfectadas > 0;
        }

        public bool Eliminar(int id)
        {
            const string sql = @"
                DELETE FROM Componentes
                WHERE Id = @id;";

            using var connection = _dbHelper.ObtenerConexion();
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", id);

            int filasAfectadas = cmd.ExecuteNonQuery();
            return filasAfectadas > 0;
        }

        public int ObtenerKilometrajeActual()
        {
            const string sql = @"
                SELECT Valor
                FROM ConfiguracionVehiculo
                WHERE Clave = @clave;";

            using var connection = _dbHelper.ObtenerConexion();
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@clave", "KilometrajeActual");

            object? result = cmd.ExecuteScalar();
            if (result is not null && int.TryParse(result.ToString(), out int km))
            {
                return Math.Max(0, km);
            }

            return 0;
        }

        public void GuardarKilometrajeActual(int kilometrajeActual)
        {
            if (kilometrajeActual < 0)
                throw new ArgumentOutOfRangeException(nameof(kilometrajeActual), "El kilometraje actual no puede ser negativo.");

            const string sql = @"
                INSERT INTO ConfiguracionVehiculo (Clave, Valor)
                VALUES (@clave, @valor)
                ON CONFLICT(Clave) DO UPDATE SET Valor = excluded.Valor;";

            using var connection = _dbHelper.ObtenerConexion();
            using var cmd = new SqliteCommand(sql, connection);
            cmd.Parameters.AddWithValue("@clave", "KilometrajeActual");
            cmd.Parameters.AddWithValue("@valor", kilometrajeActual.ToString());
            cmd.ExecuteNonQuery();
        }

        private static ComponenteVehicular MapearComponente(SqliteDataReader reader)
        {
            int id = reader.GetInt32(0);
            string tipoComponente = reader.GetString(1);
            string nombre = reader.GetString(2);
            int kmInstalacion = reader.GetInt32(3);
            int vidaUtilKm = reader.GetInt32(4);
            string tipoCombustible = reader.GetString(5);
            decimal costoRepuesto = reader.GetDecimal(6);

            return tipoComponente switch
            {
                "Motor" => new Motor(id, nombre, kmInstalacion, vidaUtilKm, tipoCombustible, costoRepuesto),
                "Frenos" => new Frenos(id, nombre, kmInstalacion, vidaUtilKm, tipoCombustible, costoRepuesto),
                "SistemaGas" => new SistemaGas(id, nombre, kmInstalacion, vidaUtilKm, tipoCombustible, costoRepuesto),
                "Suspension" => new Suspension(id, nombre, kmInstalacion, vidaUtilKm, tipoCombustible, costoRepuesto),
                _ => throw new InvalidOperationException($"Tipo de componente no reconocido en base de datos: '{tipoComponente}'.")
            };
        }
    }
}