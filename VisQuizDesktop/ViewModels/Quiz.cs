using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisQuizDesktop.Models;

namespace VisQuizDesktop.ViewModels
{
    public class Quiz
    {
        public GameState? CurrentState { get; set; }
        public List<QuestionCategory> Categories { get; set; }
        public Stopwatch Timer { get; set; }
        public int MaxQuestions { get; set; } = 5;

        public Quiz()
        {
            var questionLoader = new Services.QuestionLoader();
            Categories = questionLoader.LoadCategories();
            System.Diagnostics.Debug.WriteLine($"Załadowano: {Categories.Sum(n => n.Questions.Count)} pytań");
            Timer = new Stopwatch();
        }

        public Question? GetCurrentQuestion()
        {
            return CurrentState?.CurrentQuestion;
        }

        public Question? NextQuestion()
        {
            if (CurrentState?.CurrentCategory == null) return null;
            
            if (CurrentState.CorrectlyAnsweredQuestions.Count + CurrentState.WronglyAnsweredQuestions.Count >= MaxQuestions)
            {
                return null;
            }

            var notAnsweredQuestions = CurrentState.CurrentCategory.Questions.Except(
                CurrentState.CorrectlyAnsweredQuestions.Union(CurrentState.WronglyAnsweredQuestions)
            ).ToList();
            
            if (notAnsweredQuestions.Count == 0) return null;
            
            var nextQuestion = notAnsweredQuestions[new Random().Next(notAnsweredQuestions.Count)];
            CurrentState.CurrentQuestion = nextQuestion;
            return nextQuestion;
        }

        public void StartQuiz(QuestionCategory category)
        {
            CurrentState = new GameState();
            Timer.Restart();
            CurrentState.CurrentCategory = category;
        }

        public bool AnswerQuestion(int answerIndex)
        {
            var currentQuestion = GetCurrentQuestion();
            if (currentQuestion != null && currentQuestion.CorrectAnswerIndex == answerIndex)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void FinishQuiz()
        { 
            Timer.Stop();
        }
    }
}
