namespace KYCApp.Views
{
    public partial class ConfirmationPage : ContentPage
    {
        private readonly string qrCode;
        private readonly string documentPhotoPath;

        public ConfirmationPage(string qrCode, string documentPhotoPath)
        {
            InitializeComponent();
            this.qrCode = qrCode;
            this.documentPhotoPath = documentPhotoPath;
            
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
        }

        private async void OnNewProcessClicked(object sender, EventArgs e)
        {
            // Go back to the main page
            await Navigation.PopToRootAsync();
        }
    }
}
