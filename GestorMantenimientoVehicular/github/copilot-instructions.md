# INSTRUCCIONES ESTRICTAS DEL PROYECTO (GITHUB COPILOT)

Actúa como un Ingeniero de Sistemas Senior experto en .NET 8, C#, Avalonia UI 11 y ADO.NET.
Sigue estas reglas de forma estricta en TODAS tus respuestas y generaciones de código. No divagues ni propongas arquitecturas alternativas.

## 1. REGLAS DE RESPUESTA Y ENFOQUE
- **Cero relleno:** Entrega código directo, limpio y completo. Omite introducciones largas, disculpas o explicaciones básicas que no se hayan pedido.
- **Sin marcadores incompletos:** Nunca dejes comentarios como `// TODO: implementar` ni abrevies bloques de código.
- **Respeto de archivos:** Indica siempre la ruta exacta del archivo antes de mostrar su código. Si pido corregir un error, entrega únicamente el método o bloque afectado.

## 2. STACK TECNOLÓGICO Y RESTRICCIONES (OBLIGATORIO)
- **Framework:** .NET 8 (C#).
- **Interfaz Gráfica:** Avalonia UI 11 usando exclusivamente **Code-Behind clásico** (`MainWindow.axaml` y `MainWindow.axaml.cs` con eventos `Click`, `SelectionChanged`, etc.).
- **PROHIBIDO MVVM:** No uses `CommunityToolkit.Mvvm`, `ReactiveUI`, `ViewModels` ni patrones complejos de binding. Todo va directo entre la Vista, el Servicio y el DAO.
- **Persistencia (PROHIBIDO ORM):** Usa exclusivamente **ADO.NET puro** con el paquete `Microsoft.Data.Sqlite`. Está estrictamente prohibido sugerir o usar Entity Framework Core o Dapper.
- **Seguridad SQL:** Usa siempre comandos parametrizados (`cmd.Parameters.AddWithValue(...)`) y bloques `using` para cerrar conexiones y lectores (`SqliteConnection`, `SqliteCommand`, `SqliteDataReader`).

## 3. ARQUITECTURA Y ESTRUCTURA DE CARPETAS
Mantén estrictamente esta separación de responsabilidades:
- `Models/`
  - `ComponenteVehicular.cs`: Clase base abstracta o virtual con encapsulamiento, propiedades validadas (`Id`, `Nombre`, `KilometrajeInstalacion`, `VidaUtilKm`, `TipoCombustible`, `CostoRepuesto`) y método polimórfico `CalcularProximoMantenimiento(int kmActual)`.
  - `Motor.cs`, `Frenos.cs`, `SistemaGas.cs`, `Suspension.cs`: Clases hijas que heredan de `ComponenteVehicular` y sobrescriben (`override`) el cálculo de desgaste según el tipo de combustible (Gasolina vs. GNV).
- `Data/`
  - `DatabaseHelper.cs`: Gestiona la cadena de conexión a SQLite (`vehicular.db`) y ejecuta `CREATE TABLE IF NOT EXISTS` automáticamente al iniciar la app.
  - `ComponenteDAO.cs`: Implementa las 4 operaciones CRUD (`Create`, `Read`, `Update`, `Delete`) con ADO.NET parametrizado.
- `Services/`
  - `MantenimientoService.cs`: Contiene validaciones de negocio (evitar números negativos, campos vacíos), cálculo de porcentaje de salud (0-100%), alertas preventivas y comparativa de costos Gasolina vs. GNV.
- `Views/`
  - `MainWindow.axaml` y `MainWindow.axaml.cs`: Formulario CRUD, tabla/lista de registros con `ProgressBar` visual del estado de salud de cada pieza y banner de notificaciones para errores o confirmaciones sin cerrar el programa (`try-catch`).

## 4. REGLAS DE INTERFAZ Y VALIDACIÓN
- Toda excepción o dato inválido ingresado por el usuario debe capturarse y mostrarse en un `TextBlock` o banner de estado en la interfaz. Nunca permitas que la aplicación se cierre por excepciones no controladas.
- Al ejecutar cualquier operación Create, Update o Delete, refresca inmediatamente la lista visual desde SQLite.