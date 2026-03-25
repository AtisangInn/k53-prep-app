using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using K53PrepApp.Data;
using K53PrepApp.Models;

namespace K53PrepApp.Controllers;

// ============================================================
//  STUDENTS CONTROLLER
// ============================================================
[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    // Read admin code from environment variable, fall back to appsettings.json for local dev
    private string AdminCode => Environment.GetEnvironmentVariable("ADMIN_CODE") 
                                ?? _config["AdminCode"] 
                                ?? "admin1234";

    public StudentsController(AppDbContext db, IConfiguration config)
    {
        _db = db; _config = config;
    }

    // POST /api/students/identify - create or retrieve student by name+phone
    [HttpPost("identify")]
    public async Task<IActionResult> Identify([FromBody] IdentifyDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Phone))
            return BadRequest("Name and phone are required.");

        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.Name == dto.Name.Trim() && s.Phone == dto.Phone.Trim());

        if (student == null)
        {
            student = new Student { Name = dto.Name.Trim(), Phone = dto.Phone.Trim() };
            _db.Students.Add(student);
        }
        else
        {
            student.LastSeen = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { student.Id, student.Name, student.Phone });
    }

    // GET /api/students - admin: list all students with stats
    [HttpGet]
    public async Task<IActionResult> GetAll([FromHeader(Name = "X-Admin-Code")] string? adminCode)
    {
        if (adminCode != AdminCode) return Unauthorized();

        var students = await _db.Students
            .Include(s => s.TestResults)
            .OrderByDescending(s => s.LastSeen)
            .Select(s => new {
                s.Id, s.Name, s.Phone,
                s.FirstSeen, s.LastSeen,
                s.FlippedCardsCount, s.TotalStudySeconds,
                TestCount = s.TestResults.Count,
                LastScore = s.TestResults
                    .OrderByDescending(t => t.TakenAt)
                    .Select(t => (int?)( t.RulesScore + t.SignsScore + t.ControlsScore ))
                    .FirstOrDefault(),
                LastPass = s.TestResults
                    .OrderByDescending(t => t.TakenAt)
                    .Select(t => (bool?)t.OverallPass)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(students);
    }

    // GET /api/students/{id}/results - get test history for a student
    [HttpGet("{id}/results")]
    public async Task<IActionResult> GetResults(int id, [FromHeader(Name = "X-Admin-Code")] string? adminCode)
    {
        if (adminCode != AdminCode) return Unauthorized();

        var results = await _db.TestResults
            .Where(t => t.StudentId == id)
            .OrderByDescending(t => t.TakenAt)
            .Select(t => new {
                t.Id, t.TakenAt, t.DurationSeconds,
                t.RulesScore, t.RulesTotal,
                t.SignsScore, t.SignsTotal,
                t.ControlsScore, t.ControlsTotal,
                t.OverallPass
            })
            .ToListAsync();

        return Ok(results);
    }

    // POST /api/students/{id}/activity - increment engagement stats
    [HttpPost("{id}/activity")]
    public async Task<IActionResult> UpdateActivity(int id, [FromBody] ActivityUpdateDto dto)
    {
        var student = await _db.Students.FindAsync(id);
        if (student == null) return NotFound();

        if (dto.FlippedIncrement > 0)
            student.FlippedCardsCount += dto.FlippedIncrement;
        
        if (dto.StudySecondsIncrement > 0)
            student.TotalStudySeconds += dto.StudySecondsIncrement;

        student.LastSeen = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { student.FlippedCardsCount, student.TotalStudySeconds });
    }

    // DELETE /api/students/{id} - admin: delete student
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromHeader(Name = "X-Admin-Code")] string? adminCode)
    {
        if (adminCode != AdminCode) return Unauthorized();

        var student = await _db.Students.FindAsync(id);
        if (student == null) return NotFound();

        _db.Students.Remove(student);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// ============================================================
//  TESTS CONTROLLER
// ============================================================
[ApiController]
[Route("api/[controller]")]
public class TestsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    // Read admin code from environment variable, fall back to appsettings.json for local dev
    private string AdminCode => Environment.GetEnvironmentVariable("ADMIN_CODE") 
                                ?? _config["AdminCode"] 
                                ?? "admin1234";

    public TestsController(AppDbContext db, IConfiguration config) 
    { 
        _db = db; 
        _config = config;
    }

    // POST /api/tests/submit - student submits a completed test
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] TestSubmissionDto dto)
    {
        var student = await _db.Students.FindAsync(dto.StudentId);
        if (student == null) return BadRequest("Student not found.");

        // Load the correct answers for all submitted question IDs
        var questionIds = dto.Answers.Select(a => a.QuestionId).ToList();
        var questions = await _db.Questions
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id);

        // Build result
        var result = new TestResult
        {
            StudentId = dto.StudentId,
            DurationSeconds = dto.DurationSeconds,
            TakenAt = DateTime.UtcNow
        };

        foreach (var answer in dto.Answers)
        {
            if (!questions.TryGetValue(answer.QuestionId, out var q)) continue;

            var isCorrect = answer.ChosenOption != null &&
                            answer.ChosenOption.Equals(q.CorrectOption, StringComparison.OrdinalIgnoreCase);

            result.Answers.Add(new TestAnswer {
                QuestionId = answer.QuestionId,
                ChosenOption = answer.ChosenOption,
                IsCorrect = isCorrect
            });

            // Tally by category
            switch (q.Category)
            {
                case "Rules":
                    result.RulesTotal++;
                    if (isCorrect) result.RulesScore++;
                    break;
                case "Signs":
                    result.SignsTotal++;
                    if (isCorrect) result.SignsScore++;
                    break;
                case "Controls":
                    result.ControlsTotal++;
                    if (isCorrect) result.ControlsScore++;
                    break;
            }
        }

        student.LastSeen = DateTime.UtcNow;
        _db.TestResults.Add(result);
        await _db.SaveChangesAsync();

        // Return the full result with correct answers for the results page
        var answersWithAnswers = result.Answers.Select(a => new {
            a.QuestionId,
            a.ChosenOption,
            a.IsCorrect,
            CorrectOption = questions[a.QuestionId].CorrectOption,
            QuestionText = questions[a.QuestionId].Text,
            Category = questions[a.QuestionId].Category,
            ImageUrl = questions[a.QuestionId].ImageUrl,
            Explanation = questions[a.QuestionId].Explanation
        });

        return Ok(new {
            result.Id,
            result.TakenAt,
            result.DurationSeconds,
            result.RulesScore, result.RulesTotal,
            result.SignsScore, result.SignsTotal,
            result.ControlsScore, result.ControlsTotal,
            result.OverallPass,
            Answers = answersWithAnswers
        });
    }

    // GET /api/tests/{id} - retrieve a saved result
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _db.TestResults
            .Include(t => t.Answers)
            .ThenInclude(a => a.Question)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (result == null) return NotFound();

        return Ok(new {
            result.Id,
            result.TakenAt,
            result.DurationSeconds,
            result.RulesScore, result.RulesTotal,
            result.SignsScore, result.SignsTotal,
            result.ControlsScore, result.ControlsTotal,
            result.OverallPass,
            Answers = result.Answers.Select(a => new {
                a.QuestionId,
                a.ChosenOption,
                a.IsCorrect,
                CorrectOption = a.Question.CorrectOption,
                QuestionText = a.Question.Text,
                Category = a.Question.Category,
                ImageUrl = a.Question.ImageUrl,
                Explanation = a.Question.Explanation
            })
        });
    }

    // GET /api/tests/admin/all - admin: all test results with stats
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllAdmin([FromHeader(Name = "X-Admin-Code")] string? adminCode)
    {
        if (adminCode != AdminCode) return Unauthorized();

        var results = await _db.TestResults
            .Include(t => t.Student)
            .OrderByDescending(t => t.TakenAt)
            .Take(200)
            .Select(t => new {
                t.Id, t.TakenAt, t.DurationSeconds,
                StudentName = t.Student.Name,
                StudentPhone = t.Student.Phone,
                t.RulesScore, t.RulesTotal,
                t.SignsScore, t.SignsTotal,
                t.ControlsScore, t.ControlsTotal,
                t.OverallPass
            })
            .ToListAsync();

        return Ok(results);
    }
}

// DTOs
public record IdentifyDto(string Name, string Phone);
public record ActivityUpdateDto(int FlippedIncrement, int StudySecondsIncrement);
public record TestSubmissionDto(int StudentId, int DurationSeconds, List<AnswerDto> Answers);
public record AnswerDto(int QuestionId, string? ChosenOption);
