// Ruta: Views/MainWindow.axaml.cs
using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Media;
using GestorMantenimientoVehicular.Data;
using GestorMantenimientoVehicular.Services;

namespace GestorMantenimientoVehicular.Views
{
    public class HexToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string hex && Color.TryParse(hex, out Color color))
            {
                return new SolidColorBrush(color);
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public partial class MainWindow : Window
    {
        private enum TipoNotificacion
        {
            Info,
            Exito,
            Advertencia,
            Error
        }

        private static readonly CultureInfo CulturaCol = CultureInfo.GetCultureInfo("es-CO");
        private readonly MantenimientoService _service;
        private int? _idSeleccionado;
        private bool _cargandoFormulario;

        public MainWindow()
        {
            InitializeComponent();

            var dbHelper = new DatabaseHelper();
            var dao = new ComponenteDAO(dbHelper);
            _service = new MantenimientoService(dao);

            InicializarControles();
            CargarDatosEnPantalla();
        }

        private void InicializarControles()
        {
            CmbTipoComponente.ItemsSource = new List<string>
            {
                "Motor",
                "Frenos",
                "SistemaGas",
                "Suspension"
            };
            CmbTipoComponente.SelectedIndex = 0;

            CmbTipoCombustible.ItemsSource = new List<string>
            {
                "Gasolina",
                "GNV"
            };
            CmbTipoCombustible.SelectedIndex = 0;

            int kmActual = _service.ObtenerKilometrajeActual();
            TxtOdometroActual.Text = kmActual.ToString();
        }

        private void CargarDatosEnPantalla()
        {
            try
            {
                int kmActual = LeerOdometroActual();
                List<ComponenteEstadoItem> items = _service.ObtenerEstadoComponentes(kmActual);
                LstComponentes.ItemsSource = items;

                ComparativaCombustibleResumen resumen = _service.CalcularResumenYComparativa(kmActual, 1000);

                TxtResumenTotal.Text = $"{resumen.TotalComponentes} registrados";
                TxtResumenAlertas.Text = $"{resumen.ComponentesEnAlerta} en alerta | {resumen.ComponentesVencidos} vencidos";
                TxtResumenInversion.Text = $"$ {resumen.InversionTotalRepuestos.ToString("N0", CulturaCol)}";
                TxtResumenAhorro.Text = $"Ahorro GNV: $ {resumen.AhorroEstimadoGnv.ToString("N0", CulturaCol)} ({resumen.PorcentajeAhorroGnv:0.0}%)";
                TxtResumenCombustible.Text =
                    $"Gasolina: $ {resumen.CostoGasolinaEstimado.ToString("N0", CulturaCol)} | GNV: $ {resumen.CostoGnvEstimado.ToString("N0", CulturaCol)}";
            }
            catch (Exception ex)
            {
                MostrarNotificacion($"Error al cargar datos: {ex.Message}", TipoNotificacion.Error);
            }
        }

        private void BtnActualizarOdometro_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                int kmActual = LeerOdometroActual();
                _service.ActualizarKilometrajeActual(kmActual);
                CargarDatosEnPantalla();
                MostrarNotificacion(
                    $"Odómetro actualizado a {kmActual.ToString("N0", CulturaCol)} km. Estados de desgaste recalculados.",
                    TipoNotificacion.Exito);
            }
            catch (Exception ex)
            {
                MostrarNotificacion(ex.Message, TipoNotificacion.Error);
            }
        }

        private void BtnGuardar_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var (tipo, nombre, combustible, kmInst, vidaUtil, costo) = LeerYValidarFormulario();
                int nuevoId = _service.RegistrarComponente(tipo, nombre, kmInst, vidaUtil, combustible, costo);

                LimpiarFormulario();
                CargarDatosEnPantalla();
                MostrarNotificacion($"Componente '{nombre}' registrado con Id #{nuevoId}.", TipoNotificacion.Exito);
            }
            catch (Exception ex)
            {
                MostrarNotificacion(ex.Message, TipoNotificacion.Error);
            }
        }

        private void BtnActualizar_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (_idSeleccionado is null)
                {
                    MostrarNotificacion("Seleccione primero un componente de la lista para actualizar.", TipoNotificacion.Advertencia);
                    return;
                }

                var (tipo, nombre, combustible, kmInst, vidaUtil, costo) = LeerYValidarFormulario();
                bool actualizado = _service.ActualizarComponente(_idSeleccionado.Value, tipo, nombre, kmInst, vidaUtil, combustible, costo);

                if (actualizado)
                {
                    int idEditado = _idSeleccionado.Value;
                    LimpiarFormulario();
                    CargarDatosEnPantalla();
                    MostrarNotificacion($"Componente #{idEditado} ('{nombre}') actualizado correctamente.", TipoNotificacion.Exito);
                }
                else
                {
                    MostrarNotificacion("No se encontró el componente en la base de datos.", TipoNotificacion.Advertencia);
                }
            }
            catch (Exception ex)
            {
                MostrarNotificacion(ex.Message, TipoNotificacion.Error);
            }
        }

        private void BtnEliminar_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                if (_idSeleccionado is null)
                {
                    MostrarNotificacion("Seleccione primero un componente de la lista para eliminar.", TipoNotificacion.Advertencia);
                    return;
                }

                int idEliminar = _idSeleccionado.Value;
                bool eliminado = _service.EliminarComponente(idEliminar);

                if (eliminado)
                {
                    LimpiarFormulario();
                    CargarDatosEnPantalla();
                    MostrarNotificacion($"Componente #{idEliminar} eliminado de la bitácora.", TipoNotificacion.Exito);
                }
                else
                {
                    MostrarNotificacion("No fue posible eliminar el componente seleccionado.", TipoNotificacion.Advertencia);
                }
            }
            catch (Exception ex)
            {
                MostrarNotificacion(ex.Message, TipoNotificacion.Error);
            }
        }

        private void BtnLimpiar_Click(object? sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            MostrarNotificacion("Formulario limpio. Listo para nuevo registro.", TipoNotificacion.Info);
        }

        private void LstComponentes_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_cargandoFormulario)
                return;

            if (LstComponentes.SelectedItem is not ComponenteEstadoItem item)
                return;

            _cargandoFormulario = true;
            try
            {
                _idSeleccionado = item.Id;
                TxtIdSeleccionado.Text = $"Modo: Editando componente #{item.Id}";

                CmbTipoComponente.SelectedItem = item.TipoComponente;
                TxtNombre.Text = item.Nombre;
                CmbTipoCombustible.SelectedItem = item.TipoCombustible;
                TxtKmInstalacion.Text = item.KilometrajeInstalacion.ToString();
                TxtVidaUtilKm.Text = item.VidaUtilKm.ToString();
                TxtCostoRepuesto.Text = item.CostoRepuesto.ToString("0.##", CultureInfo.InvariantCulture);

                BtnActualizar.IsEnabled = true;
                BtnEliminar.IsEnabled = true;
                MostrarNotificacion($"Componente #{item.Id} cargado en el formulario.", TipoNotificacion.Info);
            }
            finally
            {
                _cargandoFormulario = false;
            }
        }

        private int LeerOdometroActual()
        {
            string texto = TxtOdometroActual.Text?.Trim() ?? string.Empty;
            if (!int.TryParse(texto, out int kmActual) || kmActual < 0)
            {
                throw new ArgumentException("Ingrese un odómetro actual válido (número entero mayor o igual a 0).");
            }

            return kmActual;
        }

        private (string Tipo, string Nombre, string Combustible, int KmInstalacion, int VidaUtilKm, decimal CostoRepuesto) LeerYValidarFormulario()
        {
            string tipo = CmbTipoComponente.SelectedItem as string ?? string.Empty;
            string nombre = TxtNombre.Text?.Trim() ?? string.Empty;
            string combustible = CmbTipoCombustible.SelectedItem as string ?? string.Empty;

            if (!int.TryParse(TxtKmInstalacion.Text?.Trim(), out int kmInstalacion))
                throw new ArgumentException("El campo 'Km Instalación' debe ser un número entero válido.");

            if (!int.TryParse(TxtVidaUtilKm.Text?.Trim(), out int vidaUtilKm))
                throw new ArgumentException("El campo 'Vida Útil (km)' debe ser un número entero válido.");

            string costoTexto = (TxtCostoRepuesto.Text?.Trim() ?? string.Empty).Replace(",", ".");
            if (!decimal.TryParse(costoTexto, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal costoRepuesto))
                throw new ArgumentException("El campo 'Costo del Repuesto' debe ser un valor numérico válido.");

            return (tipo, nombre, combustible, kmInstalacion, vidaUtilKm, costoRepuesto);
        }

        private void LimpiarFormulario()
        {
            _cargandoFormulario = true;
            try
            {
                _idSeleccionado = null;
                TxtIdSeleccionado.Text = "Modo: Nuevo registro";
                CmbTipoComponente.SelectedIndex = 0;
                TxtNombre.Text = string.Empty;
                CmbTipoCombustible.SelectedIndex = 0;
                TxtKmInstalacion.Text = string.Empty;
                TxtVidaUtilKm.Text = string.Empty;
                TxtCostoRepuesto.Text = string.Empty;

                LstComponentes.SelectedItem = null;
                BtnActualizar.IsEnabled = false;
                BtnEliminar.IsEnabled = false;
            }
            finally
            {
                _cargandoFormulario = false;
            }
        }

        private void MostrarNotificacion(string mensaje, TipoNotificacion tipo)
        {
            TxtNotificacion.Text = mensaje;

            switch (tipo)
            {
                case TipoNotificacion.Exito:
                    BannerNotificacion.Background = new SolidColorBrush(Color.Parse("#DCFCE7"));
                    BannerNotificacion.BorderBrush = new SolidColorBrush(Color.Parse("#16A34A"));
                    TxtNotificacion.Foreground = new SolidColorBrush(Color.Parse("#166534"));
                    break;
                case TipoNotificacion.Advertencia:
                    BannerNotificacion.Background = new SolidColorBrush(Color.Parse("#FEF3C7"));
                    BannerNotificacion.BorderBrush = new SolidColorBrush(Color.Parse("#D97706"));
                    TxtNotificacion.Foreground = new SolidColorBrush(Color.Parse("#92400E"));
                    break;
                case TipoNotificacion.Error:
                    BannerNotificacion.Background = new SolidColorBrush(Color.Parse("#FEE2E2"));
                    BannerNotificacion.BorderBrush = new SolidColorBrush(Color.Parse("#DC2626"));
                    TxtNotificacion.Foreground = new SolidColorBrush(Color.Parse("#991B1B"));
                    break;
                default:
                    BannerNotificacion.Background = new SolidColorBrush(Color.Parse("#E0F2FE"));
                    BannerNotificacion.BorderBrush = new SolidColorBrush(Color.Parse("#0284C7"));
                    TxtNotificacion.Foreground = new SolidColorBrush(Color.Parse("#0369A1"));
                    break;
            }
        }
    }
}