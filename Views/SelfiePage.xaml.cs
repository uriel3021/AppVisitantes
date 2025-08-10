using Microsoft.Maui.Media;

namespace KYCApp.Views
{
    public partial class SelfiePage : ContentPage
    {
        private readonly string qrCode;
        private readonly string documentPhotoPath;
        private string selfiePhotoPath = string.Empty;

        public SelfiePage(string qrCode, string documentPhotoPath)
        {
            InitializeComponent();
            this.qrCode = qrCode;
            this.documentPhotoPath = documentPhotoPath;
        }

        private async void OnTakeSelfieClicked(object sender, EventArgs e)
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
                        SelfieImage.Source = ImageSource.FromStream(() => stream);
                        
                        // Show the image and finish button
                        SelfieePlaceholder.IsVisible = false;
                        SelfieImage.IsVisible = true;
                        ContinueButton.IsVisible = true;
                        
                        selfiePhotoPath = photo.FullPath;
                    }
                }
                else
                {
                    await DisplayAlert("Error", "La cámara no está disponible", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo tomar la selfie: {ex.Message}", "OK");
            }
        }

        private async void OnFinishClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selfiePhotoPath))
            {
                await Navigation.PushAsync(new ConfirmationPage(qrCode, documentPhotoPath));
            }
        }

        private async void OnContinueClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selfiePhotoPath))
            {
                await Navigation.PushAsync(new ConfirmationPage(qrCode, documentPhotoPath));
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
