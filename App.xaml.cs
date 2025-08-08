namespace KYCApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        MainPage = new AppShell();
    }
    
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        
        // Configurar tamaño de ventana para escritorio
        window.Title = "KYC App - Sistema de Verificación";
        
        return window;
    }
}
