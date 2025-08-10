namespace KYCApp.Views
{
    public partial class PlacasCapturePage : ContentPage
    {
        private string qrCode;
        private string documentPhotoPath;
        private string placasPhotoPath = string.Empty;

        public PlacasCapturePage(string qrCode, string documentPath)
        {
            InitializeComponent();
            this.qrCode = qrCode;
            this.documentPhotoPath = documentPath;
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
                        // Save photo to local storage
                        var localFilePath = Path.Combine(FileSystem.CacheDirectory, $"placas_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");

                        using var sourceStream = await photo.OpenReadAsync();
                        using var localFileStream = File.OpenWrite(localFilePath);
                        await sourceStream.CopyToAsync(localFileStream);

                        placasPhotoPath = localFilePath;

                        // Show preview
                        PlacaImage.Source = ImageSource.FromFile(localFilePath);
                        PlacaImage.IsVisible = true;
                        PlacaPlaceholder.IsVisible = false;

                        // Show continue button
                        ContinueButton.IsVisible = true;
                    }
                }
                else
                {
                    await DisplayAlert("Error", "La captura de fotos no está soportada en este dispositivo", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al capturar foto: {ex.Message}", "OK");
            }
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(placasPhotoPath))
            {
                await DisplayAlert("Error", "Debe tomar una foto de las placas antes de continuar", "OK");
                return;
            }

            // Navigate to summary page with all data
            await Navigation.PushAsync(new ResumenPage(qrCode, documentPhotoPath, placasPhotoPath));
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            // Clean up photo file if exists
            if (!string.IsNullOrEmpty(placasPhotoPath) && File.Exists(placasPhotoPath))
            {
                try
                {
                    File.Delete(placasPhotoPath);
                }
                catch { /* Ignore errors */ }
            }

            await Navigation.PopAsync();
        }

        private async void OnNewCaptureClicked(object sender, EventArgs e)
        {
            // Reset the photo state
            if (!string.IsNullOrEmpty(placasPhotoPath) && File.Exists(placasPhotoPath))
            {
                try
                {
                    File.Delete(placasPhotoPath);
                }
                catch { /* Ignore errors */ }
            }

            placasPhotoPath = string.Empty;
            PlacaPlaceholder.IsVisible = true;
            PlacaImage.IsVisible = false;
            ContinueButton.IsVisible = false;
        }
    }
}
