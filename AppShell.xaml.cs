using KYCApp.Views;

namespace KYCApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Registrar rutas para navegación
        Routing.RegisterRoute("MainPage", typeof(MainPage));
        Routing.RegisterRoute("qrscan", typeof(QRScanPage));
        Routing.RegisterRoute("documentcapture", typeof(DocumentCapturePage));
        Routing.RegisterRoute("placascapture", typeof(PlacasCapturePage));
        Routing.RegisterRoute("resumen", typeof(ResumenPage));
        Routing.RegisterRoute("confirmation", typeof(ConfirmationPage));
    }
}
