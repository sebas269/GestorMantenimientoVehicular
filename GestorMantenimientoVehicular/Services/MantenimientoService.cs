// Ruta: Services/MantenimientoService.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GestorMantenimientoVehicular.Data;
using GestorMantenimientoVehicular.Models;

namespace GestorMantenimientoVehicular.Services
{
    public class ComponenteEstadoItem
    {
        public int Id { get; init; }
        public string TipoComponente { get; init; } = string.Empty;
        public string Nombre { get; init; } = string.Empty;
        public int KilometrajeInstalacion { get; init; }
        public int VidaUtilKm { get; init; }
        public int VidaUtilEfectivaKm { get; init; }
        public string TipoCombustible { get; init; } = string.Empty;
        public decimal CostoRepuesto { get; init; }
        public string CostoRepuestoTexto { get; init; } = string.Empty;
        public int KilometrajeObjetivo { get; init; }
        public int KmRestantes { get; init; }
        public double PorcentajeDesgaste { get; init; }
        public double PorcentajeSalud { get; init; }
        public string PorcentajeSaludTexto { get; init; } = string.Empty;
        public bool RequiereAlerta { get; init; }
        public bool EstaVencido { get; init; }
        public string EstadoTexto { get; init; } = string.Empty;
        public string ColorEstadoHex { get; init; } = "#16A34A";
        public string DetalleProximoMantenimiento { get; init; } = string.Empty;
    }

    public class ComparativaCombustibleResumen
    {
        public int KilometrosEvaluados { get; init; }
        public decimal CostoGasolinaEstimado { get; init; }
        public decimal CostoGnvEstimado { get; init; }
        public decimal AhorroEstimadoGnv { get; init; }
        public double PorcentajeAhorroGnv { get; init; }
        public decimal InversionTotalRepuestos { get; init; }
        public int TotalComponentes { get; init; }
        public int ComponentesEnAlerta { get; init; }
        public int ComponentesVencidos { get; init; }
    }

    public class MantenimientoService
    {
        private static readonly CultureInfo CulturaCol = CultureInfo.GetCultureInfo("es-CO");
        private readonly ComponenteDAO _dao;

        public MantenimientoService(ComponenteDAO dao)
        {
            _dao = dao ?? throw new ArgumentNullException(nameof(dao));
        }

        public int ObtenerKilometrajeActual()
        {
            return _dao.ObtenerKilometrajeActual();
        }

        public void ActualizarKilometrajeActual(int kmActual)
        {
            if (kmActual < 0)
                throw new ArgumentException("El kilometraje actual del vehículo no puede ser negativo.");

            _dao.GuardarKilometrajeActual(kmActual);
        }

        public int RegistrarComponente(
            string tipoComponente,
            string nombre,
            int kmInstalacion,
            int vidaUtilKm,
            string tipoCombustible,
            decimal costoRepuesto)
        {
            ComponenteVehicular componente = ConstruirComponenteValidado(
                0,
                tipoComponente,
                nombre,
                kmInstalacion,
                vidaUtilKm,
                tipoCombustible,
                costoRepuesto);

            return _dao.Insertar(componente);
        }

        public bool ActualizarComponente(
            int id,
            string tipoComponente,
            string nombre,
            int kmInstalacion,
            int vidaUtilKm,
            string tipoCombustible,
            decimal costoRepuesto)
        {
            if (id <= 0)
                throw new ArgumentException("Seleccione un componente válido para actualizar.");

            ComponenteVehicular componente = ConstruirComponenteValidado(
                id,
                tipoComponente,
                nombre,
                kmInstalacion,
                vidaUtilKm,
                tipoCombustible,
                costoRepuesto);

            return _dao.Actualizar(componente);
        }

        public bool EliminarComponente(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Seleccione un componente válido para eliminar.");

            return _dao.Eliminar(id);
        }

        public List<ComponenteEstadoItem> ObtenerEstadoComponentes(int kmActual)
        {
            if (kmActual < 0)
                throw new ArgumentException("El kilometraje actual no puede ser negativo.");

            List<ComponenteVehicular> componentes = _dao.ObtenerTodos();
            var resultado = new List<ComponenteEstadoItem>(componentes.Count);

            foreach (ComponenteVehicular comp in componentes)
            {
                int vidaEfectiva = comp.ObtenerVidaUtilEfectivaKm();
                int kmObjetivo = comp.CalcularKilometrajeObjetivo();
                int kmRestantes = comp.CalcularProximoMantenimiento(kmActual);
                double desgaste = comp.CalcularPorcentajeDesgaste(kmActual);
                double salud = comp.CalcularPorcentajeSalud(kmActual);
                bool vencido = kmRestantes <= 0;
                bool alerta = comp.RequiereAlertaPreventiva(kmActual);

                string estadoTexto;
                string colorHex;

                if (vencido)
                {
                    estadoTexto = "VENCIDO - MANTENIMIENTO INMEDIATO";
                    colorHex = "#DC2626";
                }
                else if (alerta)
                {
                    estadoTexto = "ALERTA PREVENTIVA";
                    colorHex = "#D97706";
                }
                else
                {
                    estadoTexto = "ÓPTIMO";
                    colorHex = "#16A34A";
                }

                string detalleProximo = vencido
                    ? $"Excedido por {Math.Abs(kmRestantes).ToString("N0", CulturaCol)} km (Meta: {kmObjetivo.ToString("N0", CulturaCol)} km)"
                    : $"Faltan {kmRestantes.ToString("N0", CulturaCol)} km (Meta: {kmObjetivo.ToString("N0", CulturaCol)} km)";

                resultado.Add(new ComponenteEstadoItem
                {
                    Id = comp.Id,
                    TipoComponente = comp.TipoComponente,
                    Nombre = comp.Nombre,
                    KilometrajeInstalacion = comp.KilometrajeInstalacion,
                    VidaUtilKm = comp.VidaUtilKm,
                    VidaUtilEfectivaKm = vidaEfectiva,
                    TipoCombustible = comp.TipoCombustible,
                    CostoRepuesto = comp.CostoRepuesto,
                    CostoRepuestoTexto = $"$ {comp.CostoRepuesto.ToString("N0", CulturaCol)}",
                    KilometrajeObjetivo = kmObjetivo,
                    KmRestantes = kmRestantes,
                    PorcentajeDesgaste = desgaste,
                    PorcentajeSalud = salud,
                    PorcentajeSaludTexto = $"{salud:0.0}% salud ({desgaste:0.0}% desgaste)",
                    RequiereAlerta = alerta,
                    EstaVencido = vencido,
                    EstadoTexto = estadoTexto,
                    ColorEstadoHex = colorHex,
                    DetalleProximoMantenimiento = detalleProximo
                });
            }

            return resultado;
        }

        public ComparativaCombustibleResumen CalcularResumenYComparativa(int kmActual, int kmProyeccionCombustible = 1000)
        {
            if (kmActual < 0)
                throw new ArgumentException("El kilometraje actual no puede ser negativo.");

            int kmEvaluados = kmProyeccionCombustible > 0 ? kmProyeccionCombustible : Math.Max(1000, kmActual);

            decimal costoKmGasolina = ComponenteVehicular.PrecioGalonGasolinaCop / ComponenteVehicular.RendimientoKmPorGalon;
            decimal costoKmGnv = ComponenteVehicular.PrecioMetroCubicoGnvCop / ComponenteVehicular.RendimientoKmPorM3Gnv;

            decimal totalGasolina = decimal.Round(kmEvaluados * costoKmGasolina, 2);
            decimal totalGnv = decimal.Round(kmEvaluados * costoKmGnv, 2);
            decimal ahorro = decimal.Round(totalGasolina - totalGnv, 2);
            double porcentajeAhorro = totalGasolina > 0
                ? Math.Round((double)(ahorro / totalGasolina) * 100.0, 1)
                : 0.0;

            List<ComponenteEstadoItem> estados = ObtenerEstadoComponentes(kmActual);

            return new ComparativaCombustibleResumen
            {
                KilometrosEvaluados = kmEvaluados,
                CostoGasolinaEstimado = totalGasolina,
                CostoGnvEstimado = totalGnv,
                AhorroEstimadoGnv = ahorro,
                PorcentajeAhorroGnv = porcentajeAhorro,
                InversionTotalRepuestos = estados.Sum(e => e.CostoRepuesto),
                TotalComponentes = estados.Count,
                ComponentesEnAlerta = estados.Count(e => e.RequiereAlerta && !e.EstaVencido),
                ComponentesVencidos = estados.Count(e => e.EstaVencido)
            };
        }

        private static ComponenteVehicular ConstruirComponenteValidado(
            int id,
            string tipoComponente,
            string nombre,
            int kmInstalacion,
            int vidaUtilKm,
            string tipoCombustible,
            decimal costoRepuesto)
        {
            if (string.IsNullOrWhiteSpace(tipoComponente))
                throw new ArgumentException("Debe seleccionar un tipo de componente.");

            if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length < 2)
                throw new ArgumentException("El nombre del componente debe tener al menos 2 caracteres.");

            if (kmInstalacion < 0)
                throw new ArgumentException("El kilometraje de instalación no puede ser negativo.");

            if (vidaUtilKm <= 0)
                throw new ArgumentException("La vida útil en kilómetros debe ser mayor a 0.");

            if (costoRepuesto < 0)
                throw new ArgumentException("El costo del repuesto no puede ser negativo.");

            string tipoNorm = tipoComponente.Trim();
            string combNorm = tipoCombustible?.Trim() ?? string.Empty;

            if (tipoNorm.Equals("SistemaGas", StringComparison.OrdinalIgnoreCase) &&
                !combNorm.Equals("GNV", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Regla de negocio: Un componente de 'SistemaGas' opera exclusivamente con combustible 'GNV'.");
            }

            return tipoNorm switch
            {
                "Motor" => new Motor(id, nombre, kmInstalacion, vidaUtilKm, combNorm, costoRepuesto),
                "Frenos" => new Frenos(id, nombre, kmInstalacion, vidaUtilKm, combNorm, costoRepuesto),
                "SistemaGas" => new SistemaGas(id, nombre, kmInstalacion, vidaUtilKm, combNorm, costoRepuesto),
                "Suspension" => new Suspension(id, nombre, kmInstalacion, vidaUtilKm, combNorm, costoRepuesto),
                _ => throw new ArgumentException($"Tipo de componente no soportado: '{tipoComponente}'.")
            };
        }
    }
}