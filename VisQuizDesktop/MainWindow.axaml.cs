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
        private bool _isProcessing = false;

        public MainWindow()
        {
            InitializeComponent();
            InitializeQuiz();
            SetupEventHandlers();
            ShowCategorySelection();
        }

        private BoxShadows CreateGlowShadow(string color)
        {
            return new BoxShadows(
                new BoxShadow
                {
                    Color = Color.Parse(color),
                    OffsetX = 0,
                    OffsetY = 0,
                    Blur = 30,
                    Spread = 5
                }
            );
        }

        private void SetBorderGlow(Border border, bool isCorrect)
        {
            var color = isCorrect ? "#8cc747" : "#F44336";

            border.Background = new SolidColorBrush(Color.Parse("#333739"));
            border.BorderBrush = new SolidColorBrush(Color.Parse(color));
            border.BorderThickness = new Avalonia.Thickness(7);
            border.BoxShadow = CreateGlowShadow(color);
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
            _isProcessing = false;

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

            for (int i = 0; i < _quiz.Categories.Count; i++)
            {
                var category = _quiz.Categories[i];

                var categoryContainer = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Vertical,
                    Spacing = 15,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                var border = new Border
                {
                    Classes = { "category" },
                    MinWidth = 350,
                    MinHeight = 200,
                    Child = new Grid
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
                _isProcessing = true;

                HighlightCategory(keyNumber - 1);

                var selectedCategory = _quiz.Categories[keyNumber - 1];
                _quiz.StartQuiz(selectedCategory);

                await Task.Delay(300);

                StartQuestion();
            }
        }

        private void HighlightCategory(int index)
        {
            if (index >= 0 && index < _categoryBorders.Count)
            {
                SetBorderGlow(_categoryBorders[index], true);
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

            _isProcessing = false;
        }

        private void DisplayQuestion(Question question)
        {
            if (_quiz.CurrentState == null) return;

            int currentQuestionNumber = _quiz.CurrentState.CorrectlyAnsweredQuestions.Count +
                                       _quiz.CurrentState.WronglyAnsweredQuestions.Count + 1;

            QuestionNumberText.Text = $"Pytanie {currentQuestionNumber} z {_quiz.MaxQuestions}";
            QuestionText.Text = question.Text;

            UpdateProgressDots(currentQuestionNumber);

            if (!string.IsNullOrEmpty(question.ImagePath) && File.Exists(question.ImagePath))
            {
                QuestionImage.Source = new Bitmap(question.ImagePath);
                QuestionImage.IsVisible = true;
            }
            else
            {
                QuestionImage.IsVisible = false;
            }

            AnswersStack.Children.Clear();
            _answerBorders.Clear();

            for (int i = 0; i < question.Answers.Count; i++)
            {
                var answerContainer = new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Vertical,
                    Spacing = 15,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                var border = new Border
                {
                    Classes = { "answer" },
                    MinWidth = 350,
                    MinHeight = 200,
                    Child = new Grid
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

                answerContainer.Children.Add(border);
                answerContainer.Children.Add(label);

                _answerBorders.Add(border);
                AnswersStack.Children.Add(answerContainer);
            }
        }

        private void UpdateProgressDots(int currentQuestion)
        {
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

            _isProcessing = true;

            int answerIndex = keyNumber - 1;
            bool isCorrect = _quiz.AnswerQuestion(answerIndex);

            await ShowAnswerFeedback(answerIndex, isCorrect);

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

            StartQuestion();
        }

        private async Task ShowAnswerFeedback(int selectedIndex, bool isCorrect)
        {
            if (selectedIndex >= 0 && selectedIndex < _answerBorders.Count)
            {
                SetBorderGlow(_answerBorders[selectedIndex], isCorrect);

                if (!isCorrect)
                {
                    var question = _quiz.GetCurrentQuestion();
                    if (question != null)
                    {
                        int correctIndex = question.CorrectAnswerIndex;
                        if (correctIndex >= 0 && correctIndex < _answerBorders.Count)
                        {
                            SetBorderGlow(_answerBorders[correctIndex], true);
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
            _isProcessing = false;

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
            TimeText.Text = $"Czas: {timeFormatted}";
        }

        private void RestartGame()
        {
            _isProcessing = true;
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