namespace K53PrepApp.Models;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    // Engagement Stats
    public int FlippedCardsCount { get; set; }
    public int TotalStudySeconds { get; set; }

    // Freemium / Premium
    public int FreeTestsUsed { get; set; } = 0;
    public int FreeFlipsToday { get; set; } = 0;
    public int FreeNextsToday { get; set; } = 0;
    public DateTime? LastFreeFlipDate { get; set; }
    
    // Anti-Abuse
    public string DeviceId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    
    public bool IsPremium { get; set; } = false;
    public DateTime? PremiumUntil { get; set; }

    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    public ICollection<StudentPayment> Payments { get; set; } = new List<StudentPayment>();
}

public class StudentPayment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public string MPaymentId { get; set; } = string.Empty;   // our unique reference
    public string? PfPaymentId { get; set; }                  // PayFast's payment ID
    public decimal Amount { get; set; }
    public string Status { get; set; } = "pending";           // pending | complete | cancelled
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}

public class TestResult
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public DateTime TakenAt { get; set; } = DateTime.UtcNow;
    public int DurationSeconds { get; set; }

    // Category scores
    public int RulesScore { get; set; }
    public int RulesTotal { get; set; }
    public int SignsScore { get; set; }
    public int SignsTotal { get; set; }
    public int ControlsScore { get; set; }
    public int ControlsTotal { get; set; }

    public bool OverallPass =>
        RulesScore >= 22 &&
        SignsScore >= 22 &&
        ControlsScore >= 6;

    public ICollection<TestAnswer> Answers { get; set; } = new List<TestAnswer>();
}

public class TestAnswer
{
    public int Id { get; set; }
    public int TestResultId { get; set; }
    public TestResult TestResult { get; set; } = null!;
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public string? ChosenOption { get; set; } // null = skipped
    public bool IsCorrect { get; set; }
}
