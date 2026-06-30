# 🚀 GUÍA DE INSTALACIÓN Y CONFIGURACIÓN - API MERCADO LIBRE

## ✅ PASOS COMPLETADOS

Se han creado los siguientes archivos y configuraciones:

### 📁 Archivos Creados

1. **DTOs/MercadoLibreDTO.cs** - Modelos de datos
2. **Services/MercadoLibreService.cs** - Servicio principal
3. **Controllers/MercadoLibreController.cs** - Controlador
4. **Views/MercadoLibre/Index.cshtml** - Vista principal
5. **Views/MercadoLibre/VerVisitas.cshtml** - Vista de estadísticas
6. **Views/MercadoLibre/_ModalVisitas.cshtml** - Modal de visitas
7. **wwwroot/js/mercadolibre.js** - Funciones JavaScript
8. **SQL/00_MercadoLibre_Tabla_y_SPs.sql** - Scripts de base de datos

### ⚙️ Configuraciones Actualizadas

- ✅ **appsettings.json** - Configuración de API
- ✅ **Program.cs** - Registro de servicios

---

## 📋 PASOS QUE DEBES COMPLETAR

### 1️⃣ EJECUTAR EL SCRIPT SQL

**Archivo:** `SQL/00_MercadoLibre_Tabla_y_SPs.sql`

1. Abre SQL Server Management Studio (SSMS)
2. Conéctate a tu servidor: `CRASCLNBK113\SQLEXPRESS`
3. Selecciona la base de datos: `JCF_DEV`
4. Abre el archivo SQL y ejecútalo (F5)
5. Verifica que se haya creado:
   - Tabla: `MercadoLibre_Tokens`
   - 7 Stored Procedures con prefijo `PP_psnp_MercadoLibre_`

### 2️⃣ CREAR APLICACIÓN EN MERCADO LIBRE

#### Paso A: Acceder al DevCenter

Ingresa a: https://developers.mercadolibre.cl/devcenter/

#### Paso B: Crear Nueva Aplicación

1. Haz clic en **"Crear nueva aplicación"**
2. Completa los datos:

**Información Básica:**
- **Nombre:** Sistema Inmobiliaria JCF
- **Descripción:** Sistema de gestión de publicaciones inmobiliarias
- **Logo:** (Sube el logo de tu empresa)

**Configuración Importante:**
- **URIs de Redirect:** 
  ```
  https://localhost:5001/MercadoLibre/Callback
  ```
  ⚠️ **MUY IMPORTANTE:** Esta URL debe ser EXACTA y usar HTTPS

- **Use PKCE:** ✅ Activado (recomendado)
- **Scopes:** 
  - ✅ Lectura
  - ✅ Escritura

3. Guarda la aplicación

#### Paso C: Obtener Credenciales

Después de crear la app, obtendrás:
- **CLIENT_ID** (también llamado APP_ID)
- **CLIENT_SECRET** (¡Guárdalo en un lugar seguro!)

### 3️⃣ CONFIGURAR appsettings.json

Abre el archivo `appsettings.json` y reemplaza los valores:

```json
"MercadoLibre": {
  "ClientId": "AQUÍ_TU_CLIENT_ID",
  "ClientSecret": "AQUÍ_TU_CLIENT_SECRET",
  "RedirectUri": "https://localhost:5001/MercadoLibre/Callback",
  "SiteId": "MLC"
}
```

**Valores de SiteId por país:**
- Chile: `MLC`
- Argentina: `MLA`
- Brasil: `MLB`
- México: `MLM`
- Colombia: `MCO`

### 4️⃣ CONFIGURAR HTTPS EN DESARROLLO

Para que funcione el callback en localhost con HTTPS:

#### Opción A: Certificado de Desarrollo (Recomendado)

Ejecuta en PowerShell (como administrador):

```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

#### Opción B: Usar tu Servidor de Producción

Si tienes un servidor en producción con HTTPS, cambia el `RedirectUri` a:
```
https://tudominio.com/MercadoLibre/Callback
```

### 5️⃣ COMPILAR Y PROBAR

```bash
dotnet build
dotnet run
```

---

## 🔧 CÓMO USAR LA INTEGRACIÓN

### 1. Conectar con Mercado Libre

1. Inicia sesión en tu sistema
2. Ve a: **Configuración > Mercado Libre** (o `/MercadoLibre/Index`)
3. Haz clic en **"Conectar con Mercado Libre"**
4. Inicia sesión en Mercado Libre (usa la cuenta **propietaria**, NO un colaborador)
5. Autoriza la aplicación
6. Serás redirigido de vuelta a tu sistema

✅ **¡Listo!** Ya estás conectado.

### 2. Ver Visitas de una Publicación

#### Opción A: Desde la Vista de Propiedades

Agrega este botón en tu vista de propiedades (ejemplo para `Propiedades/Index.cshtml`):

```html
<!-- En la tabla de propiedades, agrega esta columna -->
<td>
    @if (!string.IsNullOrEmpty(propiedad.ItemIdMercadoLibre))
    {
        <button type="button" 
                class="btn btn-sm btn-outline-primary" 
                onclick="MercadoLibre.verVisitasModal('@propiedad.ItemIdMercadoLibre', '@propiedad.Titulo')">
            <i class="bi bi-eye"></i>
            Ver Visitas ML
        </button>
    }
</td>
```

#### Opción B: Vista Completa de Estadísticas

```html
<a href="@Url.Action("VerVisitas", "MercadoLibre", new { itemId = "MLC123456789", titulo = "Mi Propiedad" })" 
   class="btn btn-primary">
    <i class="bi bi-bar-chart-line"></i>
    Ver Estadísticas Completas
</a>
```

#### Opción C: Mostrar Visitas en un Elemento

```html
<div id="visitasML"></div>

<script>
    // Mostrar visitas en el elemento
    MercadoLibre.mostrarVisitasEnElemento('MLC123456789', 'visitasML');
</script>
```

### 3. API Endpoints Disponibles

#### Obtener Visitas (JSON)
```javascript
fetch('/MercadoLibre/ObtenerVisitasJson?itemId=MLC123456789')
    .then(response => response.json())
    .then(data => {
        console.log('Visitas totales:', data.visitasTotales);
        console.log('Últimos 30 días:', data.visitasUltimos30Dias);
        console.log('Últimos 7 días:', data.visitasUltimos7Dias);
    });
```

#### Obtener Visitas por Día
```javascript
fetch('/MercadoLibre/ObtenerVisitasPorDia?itemId=MLC123456789&dias=30')
    .then(response => response.json())
    .then(data => {
        console.log(data);
    });
```

---

## 🎨 AGREGAR BOTÓN EN TU VISTA DE PROPIEDADES

### Actualizar `Views/Propiedades/Index.cshtml`

1. En el `<head>` o al final del archivo, agrega el script:

```html
@section Scripts {
    <script src="~/js/mercadolibre.js"></script>
}
```

2. En la tabla de propiedades, agrega una columna para Mercado Libre:

```html
<thead>
    <tr>
        <!-- Tus columnas existentes -->
        <th>Código</th>
        <th>Dirección</th>
        <th>Tipo</th>
        <th>Precio</th>
        <!-- Nueva columna -->
        <th>Visitas ML</th>
        <th>Acciones</th>
    </tr>
</thead>
<tbody>
    @foreach (var propiedad in Model)
    {
        <tr>
            <!-- Tus celdas existentes -->
            <td>@propiedad.Codigo</td>
            <td>@propiedad.Direccion</td>
            <td>@propiedad.TipoPropiedad</td>
            <td>@propiedad.Precio?.ToString("C0")</td>
            
            <!-- Nueva celda con botón -->
            <td class="text-center">
                @if (!string.IsNullOrEmpty(propiedad.ItemIdMercadoLibre))
                {
                    <button type="button" 
                            class="btn btn-sm btn-outline-warning" 
                            onclick="MercadoLibre.verVisitasModal('@propiedad.ItemIdMercadoLibre', '@propiedad.Direccion')"
                            title="Ver visitas en Mercado Libre">
                        <i class="bi bi-eye-fill"></i>
                        <small>ML</small>
                    </button>
                }
                else
                {
                    <span class="text-muted">-</span>
                }
            </td>
            
            <td>
                <!-- Tus botones de acciones existentes -->
            </td>
        </tr>
    }
</tbody>
```

---

## 🔍 CAMPOS NECESARIOS EN LA BASE DE DATOS

Asegúrate de tener estos campos en tu tabla de Propiedades:

```sql
-- Si no existe, agregar columna para guardar el ItemId de Mercado Libre
ALTER TABLE Propiedades ADD ItemIdMercadoLibre VARCHAR(50) NULL;

-- Índice para búsquedas más rápidas
CREATE NONCLUSTERED INDEX IX_Propiedades_ItemIdML 
ON Propiedades(ItemIdMercadoLibre) 
WHERE ItemIdMercadoLibre IS NOT NULL;
```

---

## ⚠️ SOLUCIÓN DE PROBLEMAS

### Error: "No se recibió el código de autorización"

**Causa:** El redirect_uri no coincide exactamente con el configurado.

**Solución:**
1. Verifica que el redirect_uri en appsettings.json sea EXACTO al configurado en Mercado Libre
2. Debe incluir HTTPS
3. No debe tener parámetros adicionales

### Error: "invalid_client"

**Causa:** CLIENT_ID o CLIENT_SECRET incorrectos.

**Solución:**
1. Verifica las credenciales en DevCenter
2. Asegúrate de no tener espacios extras al copiarlas
3. Reinicia la aplicación después de cambiar appsettings.json

### Error: "Lo sentimos, la aplicación no puede conectarse"

**Causa:** Usuario no es propietario o tiene datos pendientes.

**Solución:**
1. Usa la cuenta PROPIETARIA de Mercado Libre (no colaborador)
2. Completa cualquier validación de datos pendiente en Mercado Libre
3. Verifica que la cuenta esté activa

### El token expira constantemente

**Causa:** El refresh token no se está renovando correctamente.

**Solución:**
1. Verifica que los Stored Procedures se ejecutaron correctamente
2. Revisa los logs de la aplicación para ver errores
3. Prueba desconectar y volver a conectar

---

## 📊 DATOS QUE PUEDES OBTENER

### Visitas Totales
- Históricas (últimos 2 años)
- Por rango de fechas (máximo 150 días)

### Visitas Detalladas
- Por día
- Por semana
- Por mes

### Estadísticas
- Promedio de visitas por día
- Tendencias
- Comparativas

---

## 🔐 SEGURIDAD

✅ **Implementado:**
- Tokens almacenados en base de datos encriptada
- Renovación automática de access tokens
- Manejo seguro de refresh tokens
- Validación de estados de conexión

⚠️ **Recomendaciones:**
- NO compartas tu CLIENT_SECRET
- NO subas appsettings.json a repositorios públicos
- Usa variables de entorno en producción
- Implementa logs de auditoría para accesos

---

## 📞 SOPORTE

Si tienes problemas:

1. **Revisa los logs** en `/logs` o en la consola de la aplicación
2. **Verifica la conexión** en `/MercadoLibre/Index`
3. **Consulta la documentación oficial**: https://developers.mercadolibre.cl/es_ar/api-docs-es

---

## ✨ PRÓXIMOS PASOS SUGERIDOS

1. Agregar campo `ItemIdMercadoLibre` en tu modelo de Propiedades
2. Modificar el formulario de alta/edición para guardar el ItemId
3. Agregar botones de visitas en las vistas de propiedades
4. Crear un dashboard con estadísticas consolidadas
5. Implementar notificaciones de visitas bajas
6. Agregar exportación de reportes de visitas

---

## 📝 NOTAS IMPORTANTES

- Los tokens de refresh expiran después de **6 meses de inactividad**
- Los access tokens duran **6 horas**
- El sistema renueva automáticamente los tokens cuando es necesario
- Solo se puede tener **una conexión activa** a la vez
- El usuario que autoriza debe ser el **propietario** de la cuenta de Mercado Libre

---

¡Configuración completa! 🎉

Si tienes dudas, revisa la documentación oficial de Mercado Libre o consulta este archivo nuevamente.

