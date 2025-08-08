using ZXing.Net.Maui;

namespace KYCApp.Views
{
    public partial class QRScanPage : ContentPage
    {
        private readonly List<string> validQRCodes = new List<string>
        {
            "QR001",
            "QR002",  
            "QR003",
            "QR004"
        };

        public QRScanPage()
        {
            InitializeComponent();
        }

        private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var first = e.Results?.FirstOrDefault();
            if (first is not null)
            {
                var qrCode = first.Value;
                
                // Stop detection
                CameraView.IsDetecting = false;

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    if (validQRCodes.Contains(qrCode))
                    {
                        await DisplayAlert("QR Válido", $"Código QR válido: {qrCode}", "Continuar");
                        await Navigation.PushAsync(new DocumentCapturePage(qrCode));
                    }
                    else
                    {
                        await DisplayAlert("QR Inválido", "El código QR no es válido", "OK");
                        CameraView.IsDetecting = true; // Resume scanning
                    }
                });
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            CameraView.IsDetecting = false;
        }
    }
}
