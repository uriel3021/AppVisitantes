namespace KYCApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnScanQRClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new QRScanPage());
        }
    }
}
