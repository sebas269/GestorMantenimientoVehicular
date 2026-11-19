// Ruta: Models/Suspension.cs
using System;

namespace GestorMantenimientoVehicular.Models
{
    public class Suspension : ComponenteVehicular
    {
        private const double FactorDesgasteVial = 0.95;

        public Suspension(
            string nombre,
            int kilometrajeInstalacion,
            int vidaUtilKm,
            string tipoCombustible,
            decimal costoRepuesto)
            : base(nombre, kilometrajeInstalacion, vidaUtilKm, tipoCombustible, costoRepuesto)
        {
        }

        public Suspension(
            int id,
            string nombre,
            int kilometrajeInstalacion,
            int vidaUtilKm,
            string tipoCombustible,
            decimal costoRepuesto)
            : base(id, nombre, kilometrajeInstalacion, vidaUtilKm, tipoCombustible, costoRepuesto)
        {
        }

        public override string TipoComponente => "Suspension";

        public override double UmbralPreventivoSaludPorcentaje => 25.0;

        public override int ObtenerVidaUtilEfectivaKm()
        {
            // Ajuste preventivo del 5% considerando irregularidades viales urbanas e intermunicipales.
            return Math.Max(1, (int)Math.Round(VidaUtilKm * FactorDesgasteVial));
        }

        public override int CalcularProximoMantenimiento(int kmActual)
        {
            int kmObjetivo = KilometrajeInstalacion + ObtenerVidaUtilEfectivaKm();
            return kmObjetivo - Math.Max(0, kmActual);
        }
    }
}