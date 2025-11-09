using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
        private List<char> _inputLabel = new List<char> {'A','B','C','D','E','F'};

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

            CategoryPanel.IsVisible = true;
            QuestionPanel.IsVisible = false;
            ResultsPanel.IsVisible = false;
            ProgressBar.IsVisible = false;

            DisplayCategories();
        }

        private void DisplayCategories()
        {
            CategoriesGrid.Children.Clear();
            _categoryBorders.Clear();

            // Uk³ad w stylu Milionerów - 2 kolumny, wiele rzêdów
            for (int i = 0; i < _quiz.Categories.Count; i++)
            {
                var category = _quiz.Categories[i];
                int row = i / 2;
                int col = i % 2;

                var border = new Border
                {
                    Classes = { "category" },
                    MinWidth = 450,
                    Child = new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        Spacing = 20,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = $"{_inputLabel[i]}",
                                FontSize = 48,
                                FontWeight = FontWeight.Bold,
                                Foreground = new SolidColorBrush(Color.Parse("#1b75bc")),
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                Width = 60
                            },
                            new TextBlock
                            {
                                Text = category.Name,
                                FontSize = 36,
                                FontWeight = FontWeight.Bold,
                                Foreground = Brushes.White,
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                            }
                        }
                    }
                };

                Grid.SetRow(border, row);
                Grid.SetColumn(border, col);

                _categoryBorders.Add(border);
                CategoriesGrid.Children.Add(border);
            }
        }

        private void HandleCategorySelection(int keyNumber)
        {
            if (keyNumber >= 1 && keyNumber <= _quiz.Categories.Count)
            {
                // Podœwietl wybran¹ kategoriê
                HighlightCategory(keyNumber - 1);

                var selectedCategory = _quiz.Categories[keyNumber - 1];
                _quiz.StartQuiz(selectedCategory);

                // OpóŸnienie dla efektu wizualnego
                Task.Delay(300).ContinueWith(_ =>
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(() => StartQuestion());
                });
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
            ProgressBar.IsVisible = true;

            DisplayQuestion(question);
        }

        private void DisplayQuestion(Question question)
        {
            if (_quiz.CurrentState == null) return;

            int currentQuestionNumber = _quiz.CurrentState.CorrectlyAnsweredQuestions.Count +
                                       _quiz.CurrentState.WronglyAnsweredQuestions.Count + 1;

            QuestionNumberText.Text = $"Pytanie {currentQuestionNumber} z {_quiz.MaxQuestions}";
            QuestionText.Text = question.Text;

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

            // Wyœwietl odpowiedzi w uk³adzie 2 kolumny
            AnswersGrid.Children.Clear();
            _answerBorders.Clear();

            // Dynamiczne dostosowanie liczby rzêdów
            AnswersGrid.RowDefinitions.Clear();
            int rowCount = (int)Math.Ceiling(question.Answers.Count / 2.0);
            for (int r = 0; r < rowCount; r++)
            {
                AnswersGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int i = 0; i < question.Answers.Count; i++)
            {
                int row = i / 2;
                int col = i % 2;

                var border = new Border
                {
                    Classes = { "answer" },
                    MinWidth = 700,
                    Child = new StackPanel
                    {
                        Orientation = Avalonia.Layout.Orientation.Horizontal,
                        Spacing = 20,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = $"{_inputLabel[i]}",
                                FontSize = 42,
                                FontWeight = FontWeight.Bold,
                                Foreground = new SolidColorBrush(Color.Parse("#1b75bc")),
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                Width = 50
                            },
                            new TextBlock
                            {
                                Text = question.Answers[i],
                                FontSize = 32,
                                Foreground = Brushes.White,
                                TextWrapping = TextWrapping.Wrap,
                                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                                MaxWidth = 600
                            }
                        }
                    }
                };

                Grid.SetRow(border, row);
                Grid.SetColumn(border, col);

                _answerBorders.Add(border);
                AnswersGrid.Children.Add(border);
            }

            // Aktualizuj progress bar
            ProgressBar.Value = currentQuestionNumber - 1;
            ProgressBar.Maximum = _quiz.MaxQuestions;
        }

        private async void HandleAnswerSelection(int keyNumber)
        {
            var question = _quiz.GetCurrentQuestion();
            if (question == null || keyNumber < 1 || keyNumber > question.Answers.Count)
                return;

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

            QuestionPanel.IsVisible = false;
            ProgressBar.IsVisible = false;
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
            PercentageText.Text = $"Wynik: {percentage}%";
            TimeText.Text = $"Czas: {timeFormatted}";
        }

        private void RestartGame()
        {
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
