using System;
using System.ComponentModel.DataAnnotations;

public class SimulationConfiguration
{
    [Key]
    public int ConfigId { get; set; }

    [Required]
    public int FrequencyInSeconds { get; set; }

    [Required]
    public float BaselinePM2_5 { get; set; }

    [Required]
    public float BaselinePM10 { get; set; }

    [Required]
    public float BaselineO3 { get; set; }

    [Required]
    public float BaselineNO2 { get; set; }

    [Required]
    public float BaselineSO2 { get; set; }

    [Required]
    public float BaselineCO { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.Now;
}
