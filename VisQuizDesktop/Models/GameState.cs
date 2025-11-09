using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisQuizDesktop.Models
{
    public class GameState
    {
        public List<Question> CorrectlyAnsweredQuestions { get; set; }
        public List<Question> WronglyAnsweredQuestions { get; set; }
        public QuestionCategory? CurrentCategory { get; set; }
        public Question? CurrentQuestion { get; set; }

        public GameState()
        {
            CorrectlyAnsweredQuestions = new List<Question>();
            WronglyAnsweredQuestions = new List<Question>();
        }
    }
}
