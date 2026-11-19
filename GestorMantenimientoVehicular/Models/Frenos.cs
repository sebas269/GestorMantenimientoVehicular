// Ruta: Models/Frenos.cs
using System;

namespace GestorMantenimientoVehicular.Models
{
    public class Frenos : ComponenteVehicular
    {
        private const double FactorSeguridadFrenado = 0.85;

        public Frenos(
            string nombre,
            int kilometrajeInstalacion,
            int vidaUtilKm,
            string tipoCombustible,
            decimal costoRepuesto)
            : base(nombre, kilometrajeInstalacion, vidaUtilKm, tipoCombustible, costoRepuesto)
        {
        }

        public Frenos(
            int id,
            string nombre,
            int kilometrajeInstalacion,
            int vidaUtilKm,
            string tipoCombustible,
            decimal costoRepuesto)
            : base(id, nombre, kilometrajeInstalacion, vidaUtilKm, tipoCombustible, costoRepuesto)
        {
        }

        public override string TipoComponente => "Frenos";

        public override double UmbralPreventivoSaludPorcentaje => 30.0;

        public override int ObtenerVidaUtilEfectivaKm()
        {
            // Margen de seguridad del 15% por ser un sistema crítico de seguridad activa.
            return Math.Max(1, (int)Math.Round(VidaUtilKm * FactorSeguridadFrenado));
        }

        public override int CalcularProximoMantenimiento(int kmActual)
        {
            int kmObjetivo = KilometrajeInstalacion + ObtenerVidaUtilEfectivaKm();
            return kmObjetivo - Math.Max(0, kmActual);
        }
    }
}