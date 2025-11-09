using Avalonia.Controls;
using System;
using System.Linq;
using VisQuizDesktop.Models;

namespace VisQuizDesktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var questionLoader = new Services.QuestionLoader();
            var categories = questionLoader.LoadCategories();
            System.Diagnostics.Debug.WriteLine($"Za³adowano: {categories.Sum(n => n.Questions.Count)} pytañ");
        }
    }
}