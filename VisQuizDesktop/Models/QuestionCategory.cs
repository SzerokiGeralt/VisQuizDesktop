using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisQuizDesktop.Models
{
    internal class QuestionCategory
    {
        public required string Name { get; set; }
        public required List<Question> Questions { get; set; }
    }
}
