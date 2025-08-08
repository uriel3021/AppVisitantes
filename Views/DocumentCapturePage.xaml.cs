using Microsoft.Maui.Media;

namespace KYCApp.Views
{
    public partial class DocumentCapturePage : ContentPage
    {
        private readonly string qrCode;
        private string documentPhotoPath = string.Empty;

        public DocumentCapturePage(string qrCode)
        {
            InitializeComponent();
            this.qrCode = qrCode;
        }

        private async void OnTakePhotoClicked(object sender, EventArgs e)
        {
            try
            {
                if (MediaPicker.Default.IsCaptureSupported)
                {
                    var photo = await MediaPicker.Default.CapturePhotoAsync();
                    if (photo != null)
                    {
                        // Load the photo into the UI
                        var stream = await photo.OpenReadAsync();
                        DocumentImage.Source = ImageSource.FromStream(() => stream);
                        
                        // Show the image and continue button
                        PlaceholderLabel.IsVisible = false;
                        DocumentImage.IsVisible = true;
                        ContinueButton.IsVisible = true;
                        
                        documentPhotoPath = photo.FullPath;
                    }
                }
                else
                {
                    await DisplayAlert("Error", "La cámara no está disponible", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo tomar la foto: {ex.Message}", "OK");
            }
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(documentPhotoPath))
            {
                await Navigation.PushAsync(new PlacasCapturePage(qrCode, documentPhotoPath));
            }
        }
    }
}
