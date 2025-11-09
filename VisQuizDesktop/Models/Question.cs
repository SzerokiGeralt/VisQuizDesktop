using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisQuizDesktop.Models
{
    internal class Question
    {
        public required string Text { get; set; }
        public required List<string> Answers { get; set; }
        public required int CorrectAnswerIndex { get; set; }
        public string ImagePath { get; set; }
    }
}
