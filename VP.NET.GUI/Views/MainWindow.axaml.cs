using Avalonia.Controls;
using System.ComponentModel;

namespace VP.NET.GUI.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
        }
    }
}