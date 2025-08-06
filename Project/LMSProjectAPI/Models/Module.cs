using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LMSProjectAPI;

public partial class Module
{
    public int ModuleId { get; set; }

    public int? CourseId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? VideoUrl { get; set; }

    public int? OrderIndex { get; set; }
    [JsonIgnore]
    public virtual Course? Course { get; set; }
}
