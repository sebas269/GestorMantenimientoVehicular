// Ruta: Models/ComponenteVehicular.cs
using System;

namespace GestorMantenimientoVehicular.Models
{
    public abstract class ComponenteVehicular
    {
        public const decimal PrecioGalonGasolinaCop = 15800m;
        public const decimal RendimientoKmPorGalon = 40m;
        public const decimal PrecioMetroCubicoGnvCop = 2900m;
        public const decimal RendimientoKmPorM3Gnv = 14m;

        private int _id;
        private string _nombre = string.Empty;
        private int _kilometrajeInstalacion;
        private int _vidaUtilKm;
        private string _tipoCombustible = "Gasolina";
        private decimal _costoRepuesto;

        protected ComponenteVehicular(
            string nombre,
            int kilometrajeInstalacion,
            int vidaUtilKm,
            string tipoCombustible,
            decimal costoRepuesto)
            : this(0, nombre, kilometrajeInstalacion, vidaUtilKm, tipoCombustible, costoRepuesto)
        {
        }

        protected ComponenteVehicular(
            int id,
            string nombre,
            int kilometrajeInstalacion,
            int vidaUtilKm,
            string tipoCombustible,
            decimal costoRepuesto)
        {
            Id = id;
            Nombre = nombre;
            KilometrajeInstalacion = kilometrajeInstalacion;
            VidaUtilKm = vidaUtilKm;
            TipoCombustible = tipoCombustible;
            CostoRepuesto = costoRepuesto;
        }

        public int Id
        {
            get => _id;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Id), "El Id no puede ser negativo.");
                _id = value;
            }
        }

        public string Nombre
        {
            get => _nombre;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre del componente es obligatorio.", nameof(Nombre));
                _nombre = value.Trim();
            }
        }

        public int KilometrajeInstalacion
        {
            get => _kilometrajeInstalacion;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(KilometrajeInstalacion), "El kilometraje de instalación no puede ser negativo.");
                _kilometrajeInstalacion = value;
            }
        }

        public int VidaUtilKm
        {
            get => _vidaUtilKm;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(VidaUtilKm), "La vida útil debe ser mayor a 0 km.");
                _vidaUtilKm = value;
            }
        }

        public string TipoCombustible
        {
            get => _tipoCombustible;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El tipo de combustible es obligatorio.", nameof(TipoCombustible));

                string normalizado = value.Trim();
                if (!normalizado.Equals("Gasolina", StringComparison.OrdinalIgnoreCase) &&
                    !normalizado.Equals("GNV", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("El tipo de combustible debe ser 'Gasolina' o 'GNV'.", nameof(TipoCombustible));
                }

                _tipoCombustible = normalizado.Equals("GNV", StringComparison.OrdinalIgnoreCase)
                    ? "GNV"
                    : "Gasolina";
            }
        }

        public decimal CostoRepuesto
        {
            get => _costoRepuesto;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(CostoRepuesto), "El costo del repuesto no puede ser negativo.");
                _costoRepuesto = decimal.Round(value, 2);
            }
        }

        public abstract string TipoComponente { get; }

        public virtual double UmbralPreventivoSaludPorcentaje => 25.0;

        public virtual int ObtenerVidaUtilEfectivaKm() => VidaUtilKm;

        public abstract int CalcularProximoMantenimiento(int kmActual);

        public int CalcularKilometrajeObjetivo()
        {
            return KilometrajeInstalacion + ObtenerVidaUtilEfectivaKm();
        }

        public double CalcularPorcentajeDesgaste(int kmActual)
        {
            if (kmActual <= KilometrajeInstalacion)
                return 0.0;

            int kmRecorridos = kmActual - KilometrajeInstalacion;
            int vidaEfectiva = Math.Max(1, ObtenerVidaUtilEfectivaKm());
            double desgaste = (double)kmRecorridos / vidaEfectiva * 100.0;

            return Math.Clamp(Math.Round(desgaste, 2), 0.0, 100.0);
        }

        public double CalcularPorcentajeSalud(int kmActual)
        {
            double salud = 100.0 - CalcularPorcentajeDesgaste(kmActual);
            return Math.Clamp(Math.Round(salud, 2), 0.0, 100.0);
        }

        public bool RequiereAlertaPreventiva(int kmActual)
        {
            return CalcularPorcentajeSalud(kmActual) <= UmbralPreventivoSaludPorcentaje;
        }

        public virtual decimal EstimarCostoCombustible(int kmRecorridos)
        {
            if (kmRecorridos <= 0)
                return 0m;

            decimal costoPorKm = TipoCombustible == "GNV"
                ? PrecioMetroCubicoGnvCop / RendimientoKmPorM3Gnv
                : PrecioGalonGasolinaCop / RendimientoKmPorGalon;

            return decimal.Round(kmRecorridos * costoPorKm, 2);
        }

        public decimal EstimarAhorroFrenteAGasolina(int kmRecorridos)
        {
            if (kmRecorridos <= 0 || TipoCombustible != "GNV")
                return 0m;

            decimal costoGasolina = decimal.Round(kmRecorridos * (PrecioGalonGasolinaCop / RendimientoKmPorGalon), 2);
            decimal costoGnv = EstimarCostoCombustible(kmRecorridos);
            return decimal.Round(costoGasolina - costoGnv, 2);
        }
    }
}