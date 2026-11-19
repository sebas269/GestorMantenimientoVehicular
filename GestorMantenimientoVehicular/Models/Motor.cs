// Ruta: Models/Motor.cs
using System;

namespace GestorMantenimientoVehicular.Models
{
    public class Motor : ComponenteVehicular
    {
        private const double FactorDesgasteTermicoGnv = 0.90;

        public Motor(
            string nombre,
            int kilometrajeInstalacion,
            int vidaUtilKm,
            string tipoCombustible,
            decimal costoRepuesto)
            : base(nombre, kilometrajeInstalacion, vidaUtilKm, tipoCombustible, costoRepuesto)
        {
        }

        public Motor(
            int id,
            string nombre,
            int kilometrajeInstalacion,
            int vidaUtilKm,
            string tipoCombustible,
            decimal costoRepuesto)
            : base(id, nombre, kilometrajeInstalacion, vidaUtilKm, tipoCombustible, costoRepuesto)
        {
        }

        public override string TipoComponente => "Motor";

        public override double UmbralPreventivoSaludPorcentaje => 20.0;

        public override int ObtenerVidaUtilEfectivaKm()
        {
            // En operación a GNV la mayor temperatura de cámara reduce la vida útil preventiva en 10%.
            double factor = TipoCombustible == "GNV" ? FactorDesgasteTermicoGnv : 1.0;
            return Math.Max(1, (int)Math.Round(VidaUtilKm * factor));
        }

        public override int CalcularProximoMantenimiento(int kmActual)
        {
            int kmObjetivo = KilometrajeInstalacion + ObtenerVidaUtilEfectivaKm();
            return kmObjetivo - Math.Max(0, kmActual);
        }
    }
}