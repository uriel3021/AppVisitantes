using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace KYCApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        try
        {
            base.OnCreate(savedInstanceState);
            
            // Configurar manejo de excepciones no manejadas
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Excepción no manejada: {args.Exception}");
                args.Handled = true;
            };
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] Error en MainActivity: {ex}");
        }
    }
}
