using Microsoft.Maui.Controls;

namespace KYCApp.Views
{
    public partial class SplashPage : ContentPage
    {
        public SplashPage()
        {
            InitializeComponent();
            
            // Iniciar la animación y navegación
            StartSplashSequence();
        }

        private async void StartSplashSequence()
        {
            try
            {
                // Animación de aparición del logo
                LogoImage.Opacity = 0;
                await LogoImage.FadeTo(1, 1000, Easing.CubicOut);
                
                // Esperar un momento para mostrar el logo
                await Task.Delay(2000);
                
                // Animación de salida suave
                await LogoImage.FadeTo(0.7, 500, Easing.CubicIn);
                
                // Navegar a MainPage
                if (Application.Current != null)
                    Application.Current.MainPage = new AppShell();
            }
            catch (Exception ex)
            {
                // En caso de error, navegar directamente
                System.Diagnostics.Debug.WriteLine($"[SPLASH] Error: {ex.Message}");
                if (Application.Current != null)
                    Application.Current.MainPage = new AppShell();
            }
        }
    }
}
