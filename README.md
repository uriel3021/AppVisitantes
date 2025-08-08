# KYC App - Sistema de Verificación de Visitantes

![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-8.0-blue)
![Android](https://img.shields.io/badge/Android-API%2021%2B-green)
![iOS](https://img.shields.io/badge/iOS-11.0%2B-blue)
![Azure SQL](https://img.shields.io/badge/Database-Azure%20SQL-orange)

Una aplicación móvil desarrollada en .NET MAUI para la verificación y gestión de visitantes mediante códigos QR.

## 🚀 Características

- **Escaneo de códigos QR** para identificación de visitantes
- **Captura de documentos** con validación automática
- **Selfie de verificación** para autenticación biométrica
- **Conexión a base de datos Azure SQL** para almacenamiento seguro
- **Interfaz intuitiva** optimizada para dispositivos móviles
- **Arquitectura robusta** con Entity Framework Core

## 📱 Plataformas Soportadas

- **Android** (API 21+)
- **iOS** (11.0+)

## 🛠️ Tecnologías Utilizadas

- **.NET MAUI 8.0** - Framework multiplataforma
- **Entity Framework Core 8.0.8** - ORM para acceso a datos
- **Azure SQL Database** - Base de datos en la nube
- **ZXing.Net.Maui** - Escaneo de códigos QR
- **CommunityToolkit.Maui** - Controles adicionales

## 📋 Prerequisitos

- Visual Studio 2022 17.8+ con cargas de trabajo de .NET MAUI
- .NET 8.0 SDK
- Android SDK (para desarrollo Android)
- Xcode (para desarrollo iOS en macOS)

## ⚙️ Configuración

### Base de Datos

1. Configura tu cadena de conexión en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tu-servidor;Database=tu-base;User ID=tu-usuario;Password=tu-password;..."
  }
}
```

### Estructura de Base de Datos

La aplicación utiliza las siguientes tablas:

- **Visitantes**: Información de visitantes registrados
- **CodigoQR**: Códigos QR asociados a visitantes

## 🔧 Instalación y Ejecución

1. **Clonar el repositorio:**
```bash
git clone https://github.com/tu-usuario/kyc-app.git
cd kyc-app
```

2. **Restaurar paquetes NuGet:**
```bash
dotnet restore
```

3. **Configurar la base de datos:**
   - Actualiza la cadena de conexión en `appsettings.json`
   - Ejecuta las migraciones si es necesario

4. **Ejecutar la aplicación:**
```bash
# Para Android
dotnet build -f net8.0-android

# Para iOS
dotnet build -f net8.0-ios
```

## 📦 Generar APK

Para generar el APK de producción:

```bash
dotnet publish -f net8.0-android -c Release
```

El APK se generará en: `bin/Release/net8.0-android/KYCApp.KYCApp-Signed.apk`

## 🏗️ Arquitectura

```
├── Data/
│   └── KYCDbContext.cs          # Contexto de Entity Framework
├── Models/
│   ├── Visitante.cs             # Modelo de visitante
│   └── CodigoQR.cs              # Modelo de código QR
├── Services/
│   ├── VisitanteService.cs      # Lógica de negocio
│   ├── CameraService.cs         # Servicios de cámara
│   └── QRScannerService.cs      # Servicios de QR
├── ViewModels/
│   └── [ViewModels MVVM]        # ViewModels para binding
├── Views/
│   ├── MainPage.xaml            # Página principal
│   ├── QRScanPage.xaml          # Escaneo de QR
│   ├── DocumentCapturePage.xaml # Captura de documentos
│   └── SelfiePage.xaml          # Captura de selfie
└── appsettings.json             # Configuración de aplicación
```

## 🔐 Seguridad

- Conexión segura a base de datos con SSL/TLS
- Validación de datos en cliente y servidor
- Manejo seguro de credenciales mediante configuración externa

## 🤝 Contribución

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/nueva-funcionalidad`)
3. Commit tus cambios (`git commit -am 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para más detalles.

## 📞 Soporte

Para soporte técnico o consultas, por favor abre un issue en este repositorio.

---

Desarrollado con ❤️ usando .NET MAUI
