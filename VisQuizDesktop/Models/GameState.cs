using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisQuizDesktop.Models
{
    internal class GameState
    {
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public long ElapsedMiliseconds { get; set; }
        public string CategoryName { get; set; }

    }
}
