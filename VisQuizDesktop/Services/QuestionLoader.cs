using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisQuizDesktop.Models;
using System.Text.Json;

namespace VisQuizDesktop.Services
{
    internal class QuestionLoader
    {
        public const string QuestionsDirectory = "Pytania";

        public void SeedCategories()
        {
            System.IO.Directory.CreateDirectory(QuestionsDirectory);

            List<QuestionCategory> exampleCategories = new List<QuestionCategory>
            {
                new QuestionCategory
                {
                    Name = "Science",
                    Questions = new List<Question>
                    {
                        new Question
                        {
                            Text = "What is the chemical symbol for water?",
                            Answers = new List<string> { "H2O", "O2", "CO2", "NaCl" },
                            CorrectAnswerIndex = 0,
                            ImagePath = ""
                        },
                        new Question
                        {
                            Text = "What planet is known as the Red Planet?",
                            Answers = new List<string> { "Earth", "Mars", "Jupiter", "Venus" },
                            CorrectAnswerIndex = 1,
                            ImagePath = ""
                        }
                    }
                },
                new QuestionCategory
                {
                    Name = "History",
                    Questions = new List<Question>
                    {
                        new Question
                        {
                            Text = "Who was the first President of the United States?",
                            Answers = new List<string> { "George Washington", "Thomas Jefferson", "Abraham Lincoln", "John Adams" },
                            CorrectAnswerIndex = 0,
                            ImagePath = ""
                        },
                        new Question
                        {
                            Text = "In which year did World War II end?",
                            Answers = new List<string> { "1945", "1939", "1918", "1963" },
                            CorrectAnswerIndex = 0,
                            ImagePath = ""
                        }
                    }
                }
            };

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
            };

            foreach (var category in exampleCategories)
            {
                string filePath = System.IO.Path.Combine(QuestionsDirectory, $"{category.Name}.json");
                string json = JsonSerializer.Serialize(category, options);
                System.IO.File.WriteAllText(filePath, json);
            }
        }

        public List<QuestionCategory> LoadCategories() { 
            var dirInfo = new System.IO.DirectoryInfo(QuestionsDirectory);
            var categories = new List<QuestionCategory>();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };


            var files = dirInfo.GetFiles("*.json");
            foreach (var file in files)
            {
                string json = System.IO.File.ReadAllText(file.FullName);
                var category = JsonSerializer.Deserialize<QuestionCategory>(json, options);
                if (category != null)
                {
                    categories.Add(category);
                }
            }

            return categories;
        }
    }
}
