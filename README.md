# ds-2026-grupo-JST

Repositorio del Trabajo Práctico Integrador de Desarrollo de Software 2026.

## Integrantes

- Silvana Mendieta: Silvana Mendieta Logvinenko
- Juan Cruz Lissa: Juan Cruz Lissa
- Tadeo Caminos: tadecaminos

## Tecnologías utilizadas

- ABP Framework - Application (Layered)
- .NET 10
- Entity Framework Core
- MySQL
- Angular 22
- Node.js 24
- Yarn 1.22.x
- Git y GitHub
- Visual Studio Code / Visual Studio

La solución implementa una arquitectura monolítica en capas basada en ABP Application (Layered), de acuerdo con la decisión arquitectónica definida en el TP03.

## Requisitos

Para ejecutar SmartPantry localmente se requiere:

- .NET SDK 10
- Node.js 24.15.0 o superior
- Yarn 1.22.x
- MySQL Server
- ABP CLI / ABP Studio
- Git
- Visual Studio Code o Visual Studio

Se puede verificar la instalación de las herramientas principales con:

```bash
dotnet --version
node --version
yarn --version
abp --version
git --version
```

## Configuración local

SmartPantry utiliza una base de datos MySQL local.

Los proyectos que requieren acceso a la base de datos son:

- `src/SmartPantry.DbMigrator`
- `src/SmartPantry.HttpApi.Host`

Los archivos `appsettings.json` contienen una cadena de conexión de ejemplo y no deben contener contraseñas reales.

Cada integrante debe configurar localmente su propia conexión mediante la variable de entorno `ConnectionStrings__Default`.

### macOS / Linux

```bash
export ConnectionStrings__Default="Server=localhost;Port=3306;Database=SmartPantry;Uid=root;Pwd=<contraseña-local>;"
```

### Windows PowerShell

```powershell
$env:ConnectionStrings__Default="Server=localhost;Port=3306;Database=SmartPantry;Uid=root;Pwd=<contraseña-local>;"
```

La contraseña real de cada desarrollador no debe almacenarse ni versionarse en el repositorio.

La variable de entorno debe configurarse en la misma terminal desde la cual se ejecute `DbMigrator` o `HttpApi.Host`.

Si se cierra esa terminal, será necesario volver a configurar la variable antes de ejecutar nuevamente dichos proyectos.

## Restaurar dependencias

Desde la raíz del repositorio ejecutar:

```bash
abp install-libs
dotnet restore ./SmartPantry.slnx
```

Para restaurar las dependencias del frontend:

```bash
cd angular
yarn install --frozen-lockfile
cd ..
```

## Crear y actualizar la base de datos

Primero verificar que MySQL Server esté iniciado.

Con `ConnectionStrings__Default` configurada, desde la raíz del repositorio ejecutar:

```bash
dotnet run --project ./src/SmartPantry.DbMigrator
```

`DbMigrator` aplica las migraciones de Entity Framework Core y crea las tablas de infraestructura requeridas por ABP, como usuarios, roles, permisos, configuración y auditoría.

Cada integrante puede crear su propia base de datos local ejecutando este comando.

## Ejecutar el backend

Desde la raíz del repositorio y con `ConnectionStrings__Default` configurada ejecutar:

```bash
dotnet run --project ./src/SmartPantry.HttpApi.Host
```

URL local utilizada por el backend:

```text
https://localhost:44337
```

Swagger se encuentra disponible en:

```text
https://localhost:44337/swagger
```

Si la máquina todavía no confía en el certificado HTTPS de desarrollo de .NET, se puede ejecutar:

```bash
dotnet dev-certs https --trust
```

Para detener el backend utilizar:

```text
Ctrl + C
```

## Ejecutar el frontend

El backend debe permanecer ejecutándose en una terminal.

En una segunda terminal ejecutar:

```bash
cd angular
yarn start
```

URL local del frontend:

```text
http://localhost:4200
```

La aplicación Angular consume los servicios expuestos por `HttpApi.Host` mediante HTTP y no accede directamente a la base de datos MySQL.

Para detener Angular utilizar:

```text
Ctrl + C
```

## Verificación del backend

Desde la raíz del repositorio ejecutar:

```bash
dotnet restore ./SmartPantry.slnx
dotnet build ./SmartPantry.slnx --configuration Release --no-restore
dotnet test ./SmartPantry.slnx --configuration Release --no-build
```

La compilación debe finalizar sin errores.

Los tests del backend utilizan SQLite como base de datos de pruebas, por lo que no dependen de la base MySQL local utilizada por la aplicación.

## Verificación del frontend

Desde la carpeta `angular` ejecutar:

```bash
yarn install --frozen-lockfile
yarn build
yarn test --watch=false
```

El proyecto incluye un test mínimo de verificación para comprobar que el entorno de pruebas de Angular funciona correctamente.

## Integración continua

El repositorio utiliza GitHub Actions mediante el archivo:

```text
.github/workflows/ci.yml
```

El workflow contiene dos jobs independientes.

### Backend

El job `backend`:

1. Restaura las dependencias de .NET.
2. Compila la solución.
3. Ejecuta los tests del backend.

### Frontend

El job `frontend`:

1. Instala las dependencias de Angular.
2. Compila el frontend.
3. Ejecuta los tests del frontend.

El objetivo de la integración continua es detectar errores de compilación o tests antes de integrar cambios a las ramas compartidas del proyecto.

Los jobs `backend` y `frontend` deben finalizar correctamente antes de integrar los cambios mediante Pull Request.

## Flujo básico para ejecutar SmartPantry

En resumen, luego de clonar el repositorio:

```bash
abp install-libs
dotnet restore ./SmartPantry.slnx
```

Configurar `ConnectionStrings__Default` según el sistema operativo.

Luego ejecutar:

```bash
dotnet run --project ./src/SmartPantry.DbMigrator
```

Después mantener el backend ejecutándose:

```bash
dotnet run --project ./src/SmartPantry.HttpApi.Host
```

Y en otra terminal iniciar Angular:

```bash
cd angular
yarn start
```

Finalmente abrir en el navegador:

```text
http://localhost:4200
```