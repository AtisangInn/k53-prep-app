import os

questions_path = r'c:\tmp\formatted_questions.txt'
seed_data_path = r'c:\Users\kings\AppData\Roaming\Microsoft\Windows\Libraries\Downloads\K53PrepApp\K53PrepApp\backend\Data\SeedData.cs'

with open(questions_path, 'r', encoding='utf-8') as f:
    questions_content = f.read()

template = f"""using K53PrepApp.Models;
using K53PrepApp.Data;

namespace K53PrepApp.Data;

public static class SeedData
{{
    public static void Seed(AppDbContext db)
    {{
        if (db.Questions.Any()) return; // Already seeded

        var questions = new List<Question>
        {{
{questions_content}
        }};

        db.Questions.AddRange(questions);
        db.SaveChanges();
    }}
}}
"""

with open(seed_data_path, 'w', encoding='utf-8') as f:
    f.write(template)

print(f"Successfully updated {{seed_data_path}}")
