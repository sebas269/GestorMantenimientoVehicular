// Ruta: Models/SistemaGas.cs
using System;

namespace GestorMantenimientoVehicular.Models
{
    public class SistemaGas : ComponenteVehicular
    {
        private const int IntervaloMaximoInspeccionGnvKm = 20000;

        public SistemaGas(
            string nombre,
            int kilometrajeInstalacion,
            int vidaUtilKm,
            string tipoCombustible,
            decimal costoRepuesto)
            : base(nombre, kilometrajeInstalacion, vidaUtilKm, tipoCombustible, costoRepuesto)
        {
        }

        public SistemaGas(
            int id,
            string nombre,
            int kilometrajeInstalacion,
            int vidaUtilKm,
            string tipoCombustible,
            decimal costoRepuesto)
            : base(id, nombre, kilometrajeInstalacion, vidaUtilKm, tipoCombustible, costoRepuesto)
        {
        }

        public override string TipoComponente => "SistemaGas";

        public override double UmbralPreventivoSaludPorcentaje => 25.0;

        public override int ObtenerVidaUtilEfectivaKm()
        {
            // Inspección, calibración de reductor y prueba de estanqueidad máx. cada 20,000 km en GNV.
            return TipoCombustible == "GNV"
                ? Math.Min(VidaUtilKm, IntervaloMaximoInspeccionGnvKm)
                : VidaUtilKm;
        }

        public override int CalcularProximoMantenimiento(int kmActual)
        {
            int kmObjetivo = KilometrajeInstalacion + ObtenerVidaUtilEfectivaKm();
            return kmObjetivo - Math.Max(0, kmActual);
        }
    }
}