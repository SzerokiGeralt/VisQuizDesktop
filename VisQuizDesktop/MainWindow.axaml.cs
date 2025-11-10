using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VisQuizDesktop.Models;
using VisQuizDesktop.Services;
using VisQuizDesktop.ViewModels;

namespace VisQuizDesktop
{
    public partial class MainWindow : Window
    {
        private Quiz _quiz;
        private ViewState _currentView = ViewState.CategorySelection;
        private List<Border> _answerBorders = new List<Border>();
        private List<Border> _categoryBorders = new List<Border>();
        private List<char> _inputLabel = new List<char> { 'A', 'B', 'C', 'D', 'E', 'F' };
        private bool _isProcessing = false; // Flaga blokuj¹ca wielokrotne klikniêcia

        public MainWindow()
        {
            InitializeComponent();
            InitializeQuiz();
            SetupEventHandlers();
            ShowCategorySelection();
        }

        private void InitializeQuiz()
        {
            if (!Directory.Exists(QuestionLoader.QuestionsDirectory) ||
                !Directory.GetFiles(QuestionLoader.QuestionsDirectory, "*.json").Any())
            {
                var loader = new QuestionLoader();
                loader.SeedCategories();
            }

            _quiz = new Quiz();
        }

        private void SetupEventHandlers()
        {
            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            // Zablokuj input jeœli trwa przetwarzanie
            if (_isProcessing) return;

            int keyNumber = GetKeyNumber(e.Key);
            if (keyNumber == -1) return;

            switch (_currentView)
            {
                case ViewState.CategorySelection:
                    HandleCategorySelection(keyNumber);
                    break;
                case ViewState.Question:
                    HandleAnswerSelection(keyNumber);
                    break;
                case ViewState.Results:
                    if (keyNumber == 1)
                    {
                        RestartGame();
                    }
                    break;
            }
        }

        private int GetKeyNumber(Key key)
        {
            return key switch
            {
                Key.D1 or Key.NumPad1 => 1,
                Key.D2 or Key.NumPad2 => 2,
                Key.D3 or Key.NumPad3 => 3,
                Key.D4 or Key.NumPad4 => 4,
                _ => -1
            };
        }

        private void ShowCategorySelection()
        {
            _currentView = ViewState.CategorySelection;
            _isProcessing = false; // Odblokuj input

            CategoryPanel.IsVisible = true;
            QuestionPanel.IsVisible = false;
            ResultsPanel.IsVisible = false;
            ProgressDotsPanel.IsVisible = false;

            DisplayCategories();
        }

        private void DisplayCategories()
        {
            CategoriesStack.Children.Clear();
            _categoryBorders.Clear();

            // Uk³ad horyzontalny - kategorie obok siebie
            for (int i = 0; i < _quiz.Categories.Count; i++)
            {
                var category = _quiz.Categories[i];

                // G³ówny kontener z kategori¹ i etykiet¹
                var categoryContainer = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Vertical,
                    Spacing = 15,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                // Border z tekstem kategorii (bez etykiety)
                var border = new Border
                {
                    Classes = { "category" },
                    MinWidth = 350,
                    MinHeight = 200,
                    Child = new Grid  // Zmiana z StackPanel na Grid
                    {
                        Children = 
                        {
                            new TextBlock
                            {
                                Text = category.Name,
                                FontSize = 32,
                                FontWeight = FontWeight.Bold,
                                Foreground = Brushes.White,
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                TextWrapping = TextWrapping.Wrap,
                                TextAlignment = TextAlignment.Center,
                                MaxWidth = 300,
                                Padding = new Avalonia.Thickness(10)
                            }
                        }
                    }
                };

                // Etykieta pod boxem
                var label = new Border
                {
                    Classes = { "letters" },
                    CornerRadius = new Avalonia.CornerRadius(150),
                    Width = 150,
                    Height = 150,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Child = new TextBlock
                    {
                        Text = $"{_inputLabel[i]}",
                        FontSize = 50,
                        FontWeight = FontWeight.Bold,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    },
                    Padding = new Avalonia.Thickness(0, 10, 0, 0)
                };

                // Dodaj etykietê i border do kontenera
                categoryContainer.Children.Add(border);
                categoryContainer.Children.Add(label);

                _categoryBorders.Add(border);
                CategoriesStack.Children.Add(categoryContainer);
            }
        }

        private async void HandleCategorySelection(int keyNumber)
        {
            if (keyNumber >= 1 && keyNumber <= _quiz.Categories.Count)
            {
                _isProcessing = true; // Zablokuj input

                // Podœwietl wybran¹ kategoriê
                HighlightCategory(keyNumber - 1);

                var selectedCategory = _quiz.Categories[keyNumber - 1];
                _quiz.StartQuiz(selectedCategory);

                // OpóŸnienie dla efektu wizualnego
                await Task.Delay(300);
                
                StartQuestion();
            }
        }

        private void HighlightCategory(int index)
        {
            if (index >= 0 && index < _categoryBorders.Count)
            {
                _categoryBorders[index].Background = new SolidColorBrush(Color.Parse("#8cc747"));
            }
        }

        private void StartQuestion()
        {
            var question = _quiz.NextQuestion();
            if (question == null)
            {
                ShowResults();
                return;
            }

            _currentView = ViewState.Question;
            CategoryPanel.IsVisible = false;
            QuestionPanel.IsVisible = true;
            ProgressDotsPanel.IsVisible = false;

            DisplayQuestion(question);
            
            _isProcessing = false; // Odblokuj input po wyœwietleniu pytania
        }

        private void DisplayQuestion(Question question)
        {
            if (_quiz.CurrentState == null) return;

            int currentQuestionNumber = _quiz.CurrentState.CorrectlyAnsweredQuestions.Count +
                                       _quiz.CurrentState.WronglyAnsweredQuestions.Count + 1;

            QuestionNumberText.Text = $"Pytanie {currentQuestionNumber} z {_quiz.MaxQuestions}";
            QuestionText.Text = question.Text;

            // Aktualizuj kropki postêpu
            UpdateProgressDots(currentQuestionNumber);

            // Obs³uga obrazka
            if (!string.IsNullOrEmpty(question.ImagePath) && File.Exists(question.ImagePath))
            {
                QuestionImage.Source = new Bitmap(question.ImagePath);
                QuestionImage.IsVisible = true;
            }
            else
            {
                QuestionImage.IsVisible = false;
            }

            // Wyœwietl odpowiedzi w uk³adzie horyzontalnym
            AnswersStack.Children.Clear();
            _answerBorders.Clear();

            for (int i = 0; i < question.Answers.Count; i++)
            {
                // G³ówny kontener z odpowiedzi¹ i etykiet¹ (jak w kategoriach)
                var answerContainer = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Vertical,
                    Spacing = 15,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                // Border z tekstem odpowiedzi (bez etykiety)
                var border = new Border
                {
                    Classes = { "answer" },
                    MinWidth = 350,
                    MinHeight = 200,
                    Child = new Grid  // Zmiana z StackPanel na Grid (jak w kategoriach)
                    {
                        Children =
                        {
                            new TextBlock
                            {
                                Text = question.Answers[i],
                                FontSize = 28,
                                FontWeight = FontWeight.Bold,
                                Foreground = Brushes.White,
                                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                TextWrapping = TextWrapping.Wrap,
                                TextAlignment = TextAlignment.Center,
                                MaxWidth = 300,
                                Padding = new Avalonia.Thickness(10)
                            }
                        }
                    }
                };

                // Etykieta pod boxem (dok³adnie taka sama jak w kategoriach)
                var label = new Border
                {
                    Classes = { "letters" },
                    CornerRadius = new Avalonia.CornerRadius(150),
                    Width = 150,
                    Height = 150,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Child = new TextBlock
                    {
                        Text = $"{_inputLabel[i]}",
                        FontSize = 50,
                        FontWeight = FontWeight.Bold,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    },
                    Padding = new Avalonia.Thickness(0, 10, 0, 0)
                };

                // Dodaj border i etykietê do kontenera
                answerContainer.Children.Add(border);
                answerContainer.Children.Add(label);

                _answerBorders.Add(border);
                AnswersStack.Children.Add(answerContainer);
            }
        }

        private void UpdateProgressDots(int currentQuestion)
        {
            // ZnajdŸ wszystkie kropki
            var dots = new[] { Dot1, Dot2, Dot3, Dot4, Dot5 };

            for (int i = 0; i < dots.Length; i++)
            {
                if (i < currentQuestion)
                {
                    dots[i].Fill = new SolidColorBrush(Color.Parse("#8cc747"));
                }
                else
                {
                    dots[i].Fill = new SolidColorBrush(Color.Parse("#3A3F5F"));
                }
            }
        }

        private async void HandleAnswerSelection(int keyNumber)
        {
            var question = _quiz.GetCurrentQuestion();
            if (question == null || keyNumber < 1 || keyNumber > question.Answers.Count)
                return;

            _isProcessing = true; // Zablokuj input

            int answerIndex = keyNumber - 1;
            bool isCorrect = _quiz.AnswerQuestion(answerIndex);

            // Poka¿ feedback wizualny
            await ShowAnswerFeedback(answerIndex, isCorrect);

            // Zapisz odpowiedŸ
            if (_quiz.CurrentState != null && question != null)
            {
                if (isCorrect)
                {
                    _quiz.CurrentState.CorrectlyAnsweredQuestions.Add(question);
                }
                else
                {
                    _quiz.CurrentState.WronglyAnsweredQuestions.Add(question);
                }
            }

            await Task.Delay(1500);

            // Nastêpne pytanie lub wyniki
            StartQuestion();
        }

        private async Task ShowAnswerFeedback(int selectedIndex, bool isCorrect)
        {
            if (selectedIndex >= 0 && selectedIndex < _answerBorders.Count)
            {
                _answerBorders[selectedIndex].Background = new SolidColorBrush(
                    isCorrect ? Color.Parse("#8cc747") : Color.Parse("#F44336")
                );
                if (!isCorrect)
                {
                    // Podœwietl poprawn¹ odpowiedŸ
                    var question = _quiz.GetCurrentQuestion();
                    if (question != null)
                    {
                        int correctIndex = question.CorrectAnswerIndex;
                        if (correctIndex >= 0 && correctIndex < _answerBorders.Count)
                        {
                            _answerBorders[correctIndex].Background = new SolidColorBrush(Color.Parse("#8cc747"));
                        }
                    }
                }
            }

            await Task.Delay(500);
        }

        private void ShowResults()
        {
            _quiz.FinishQuiz();
            _currentView = ViewState.Results;
            _isProcessing = false; // Odblokuj input na ekranie wyników

            QuestionPanel.IsVisible = false;
            ProgressDotsPanel.IsVisible = false;
            ResultsPanel.IsVisible = true;

            DisplayResults();
        }

        private void DisplayResults()
        {
            if (_quiz.CurrentState == null) return;

            int correctAnswers = _quiz.CurrentState.CorrectlyAnsweredQuestions.Count;
            int totalQuestions = correctAnswers + _quiz.CurrentState.WronglyAnsweredQuestions.Count;
            int percentage = totalQuestions > 0 ? (correctAnswers * 100 / totalQuestions) : 0;

            var elapsed = _quiz.Timer.Elapsed;
            string timeFormatted = $"{elapsed.Minutes:00}:{elapsed.Seconds:00}";

            CategoryNameText.Text = $"Kategoria: {_quiz.CurrentState.CurrentCategory?.Name ?? "Nieznana"}";
            ScoreText.Text = $"Poprawne odpowiedzi: {correctAnswers} z {totalQuestions}";
            //PercentageText.Text = $"Wynik: {percentage}%";
            TimeText.Text = $"Czas: {timeFormatted}";
        }

        private void RestartGame()
        {
            _isProcessing = true; // Zablokuj input podczas restartu
            _quiz = new Quiz();
            ShowCategorySelection();
        }

        private enum ViewState
        {
            CategorySelection,
            Question,
            Results
        }
    }
}