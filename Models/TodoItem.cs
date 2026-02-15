using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TodoDS.Models;

public sealed class TodoItem
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime? DueTime { get; set; }
    public bool Completed { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsList { get; set; }
    public List<TodoItem> Children { get; set; } = new();

    [JsonIgnore]
    public bool HasTime => DueTime.HasValue;

    [JsonIgnore]
    public string DueDisplay => IsList
        ? "列表"
        : DueTime.HasValue
            ? DueTime.Value.ToString("MM-dd HH:mm")
            : "常驻";

    [JsonIgnore]
    public int ChildCount => Children?.Count ?? 0;

    [JsonIgnore]
    public string Subtitle => IsList ? $"列表 · {ChildCount}项" : DueDisplay;
}
