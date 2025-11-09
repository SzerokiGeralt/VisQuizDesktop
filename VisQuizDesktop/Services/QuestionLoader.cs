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
    public class QuestionLoader
    {
        public const string QuestionsDirectory = "Pytania";

        public void SeedCategories()
        {
            System.IO.Directory.CreateDirectory(QuestionsDirectory);

            List<QuestionCategory> exampleCategories = new List<QuestionCategory>
            {
                new QuestionCategory
                {
                    Name = "Grafika komputerowa",
                    Questions = new List<Question>
                    {
                        new Question
                        {
                            Text = "Jaki jest najpopularniejszy format plików graficznych z kompresją stratną?",
                            Answers = new List<string> { "JPEG", "PNG", "BMP", "TIFF" },
                            CorrectAnswerIndex = 0,
                            ImagePath = ""
                        },
                        new Question
                        {
                            Text = "Który model kolorów jest używany do wyświetlania obrazów na monitorze?",
                            Answers = new List<string> { "CMYK", "RGB", "HSV", "LAB" },
                            CorrectAnswerIndex = 1,
                            ImagePath = ""
                        }
                    }
                },
                new QuestionCategory
                {
                    Name = "Informatyka",
                    Questions = new List<Question>
                    {
                        new Question
                        {
                            Text = "Co oznacza skrót CPU?",
                            Answers = new List<string> { "Central Processing Unit", "Computer Personal Unit", "Central Program Utility", "Core Processing Utility" },
                            CorrectAnswerIndex = 0,
                            ImagePath = ""
                        },
                        new Question
                        {
                            Text = "Który język programowania jest używany głównie do tworzenia stron internetowych?",
                            Answers = new List<string> { "Python", "JavaScript", "C++", "Java" },
                            CorrectAnswerIndex = 1,
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
