using Avalonia.Controls;
using VisQuizDesktop.Models;

namespace VisQuizDesktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var questionLoader = new Services.QuestionLoader();
        }
    }
}