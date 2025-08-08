namespace KYCApp.Views
{
    public partial class ConfirmationPage : ContentPage
    {
        private readonly string qrCode;
        private readonly string documentPhotoPath;
        private readonly string selfiePhotoPath;

        public ConfirmationPage(string qrCode, string documentPhotoPath, string selfiePhotoPath)
        {
            InitializeComponent();
            this.qrCode = qrCode;
            this.documentPhotoPath = documentPhotoPath;
            this.selfiePhotoPath = selfiePhotoPath;
            
            LoadData();
        }

        private async void LoadData()
        {
            QRCodeLabel.Text = qrCode;

            // Load document image
            if (!string.IsNullOrEmpty(documentPhotoPath) && File.Exists(documentPhotoPath))
            {
                DocumentImage.Source = ImageSource.FromFile(documentPhotoPath);
            }

            // Load selfie image
            if (!string.IsNullOrEmpty(selfiePhotoPath) && File.Exists(selfiePhotoPath))
            {
                SelfieImage.Source = ImageSource.FromFile(selfiePhotoPath);
            }
        }

        private async void OnNewProcessClicked(object sender, EventArgs e)
        {
            // Go back to the main page
            await Navigation.PopToRootAsync();
        }
    }
}
