using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LMSProjectAPI;

public class Course
{
    public int CourseId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? TeacherId { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }

    [JsonIgnore]
    [NotMapped]
    public IFormFile? Image { get; set; }

  

    [JsonIgnore]
    public virtual ICollection<Enrollment>? Enrollments { get; set; } = new List<Enrollment>();

    [JsonIgnore]
    public virtual ICollection<Feedback>? Feedbacks { get; set; } = new List<Feedback>();
    [JsonIgnore]
    public virtual ICollection<Module>? Modules { get; set; } = new List<Module>();
    [JsonIgnore]
    public virtual User? Teacher { get; set; }
}


public static class ImageHelper
{
    public static string directory = "Images";
    public static string SaveImageToFile(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            return null;
        if (!Directory.Exists($"wwwroot/{directory}"))
        {
            Directory.CreateDirectory($"wwwroot/{directory}");
        }

        string fullPath = $"{directory}/{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";

        using (var stream = new FileStream($"wwwroot/{fullPath}", FileMode.Create))
        {
            imageFile.CopyTo(stream);
        }

        return fullPath;
    }

    public static string DeleteFile(string filePath)
    {
        var path = $"{Directory.GetCurrentDirectory()}/wwwroot/{filePath}";

        if (!System.IO.File.Exists(path)) return "File not found.";

        try
        {
            System.IO.File.Delete(path);
            return "File deleted successfully.";
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
