using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace LMSProjectAPI;

public class TeacherDetail
{
    public int UserId { get; set; }
    public string Qualification { get; set; } = null!;
    public decimal ExperienceYears { get; set; }

    [JsonIgnore] 
    public virtual User? User { get; set; }
}
