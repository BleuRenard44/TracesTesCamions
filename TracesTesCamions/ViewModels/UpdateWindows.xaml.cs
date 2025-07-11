using System.Windows;

namespace TracesTesCamions.Views
{
    public partial class UpdateWindow : Window
    {
        public string CurrentVersion { get; }
        public string LatestVersion { get; }

        public bool ShouldUpdate { get; private set; } = false;

        public UpdateWindow(string currentVersion, string latestVersion)
        {
            InitializeComponent();
            CurrentVersion = $"Version actuelle : {currentVersion}";
            LatestVersion = $"Nouvelle version : {latestVersion}";
            DataContext = this;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            ShouldUpdate = true;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
