using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CalculatorMAP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Subscribe to the PropertyChanged event of the view model
            ((AppViewModel)DataContext).PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "IsProgrammerMode")
                {
                    bool isProgrammerMode = ((AppViewModel)DataContext).IsProgrammerMode;
                    this.Width = isProgrammerMode ? 475 : 325;
                }
            };
        }
    }
}