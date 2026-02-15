using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TodoDS.Models;

namespace TodoDS.Services;

public sealed class TodoStore
{
    private readonly string _path;
    private readonly List<TodoItem> _items = new();
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public event EventHandler? Changed;

    public TodoStore(string path)
    {
        _path = path;
    }

    public IReadOnlyList<TodoItem> Items => _items;

    public void Load()
    {
        _items.Clear();
        if (!File.Exists(_path))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_path);
            var data = JsonSerializer.Deserialize<TodoStoreData>(json, _options);
            if (data?.Items is { Count: > 0 })
            {
                _items.AddRange(data.Items);
            }
        }
        catch (Exception)
        {
            // Ignore malformed data and keep app running.
        }

        NormalizeItems(_items);
        SortAll();
    }

    public TodoItem Add(string title, DateTime? dueTime, TodoItem? parent, bool isList = false)
    {
        var item = new TodoItem
        {
            Id = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}",
            Title = title.Trim(),
            DueTime = dueTime,
            Completed = false,
            CreatedAt = DateTime.Now,
            IsList = isList,
        };

        GetBucket(parent).Add(item);
        SortAll();
        Save();
        return item;
    }

    public void Update(TodoItem updated)
    {
        var existing = FindById(updated.Id);
        if (existing == null)
        {
            return;
        }

        existing.Title = updated.Title.Trim();
        existing.DueTime = updated.DueTime;
        if (!existing.IsList)
        {
            existing.Completed = updated.Completed;
        }

        SortAll();
        Save();
    }

    public void Delete(string id)
    {
        if (RemoveById(_items, id))
        {
            SortAll();
            Save();
        }
    }

    public IReadOnlyList<TodoItem> GetSorted(TodoItem? parent, bool includeCompleted = true)
    {
        return SortItems(GetBucket(parent), includeCompleted);
    }

    public IReadOnlyList<TodoItem> GetFlattenedTodos(bool includeCompleted = true)
    {
        var items = new List<TodoItem>();
        CollectTodos(_items, items, includeCompleted);
        return SortItems(items, includeCompleted);
    }

    public TodoItem? FindById(string id)
    {
        return FindById(_items, id);
    }

    public TodoItem? FindParent(string id)
    {
        return FindParent(_items, id, null);
    }

    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? AppPaths.DataDirectory);
        var payload = new TodoStoreData { Items = _items };
        var json = JsonSerializer.Serialize(payload, _options);
        File.WriteAllText(_path, json);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private List<TodoItem> GetBucket(TodoItem? parent)
    {
        return parent?.Children ?? _items;
    }

    private static void NormalizeItems(List<TodoItem> items)
    {
        foreach (var item in items)
        {
            item.Children ??= new List<TodoItem>();
            if (item.Children.Count > 0)
            {
                item.IsList = true;
            }

            if (item.IsList)
            {
                NormalizeItems(item.Children);
            }
        }
    }

    private static bool RemoveById(List<TodoItem> items, string id)
    {
        var index = items.FindIndex(item => item.Id == id);
        if (index >= 0)
        {
            items.RemoveAt(index);
            return true;
        }

        foreach (var item in items)
        {
            if (item.IsList && RemoveById(item.Children, id))
            {
                return true;
            }
        }

        return false;
    }

    private static TodoItem? FindById(IEnumerable<TodoItem> items, string id)
    {
        foreach (var item in items)
        {
            if (item.Id == id)
            {
                return item;
            }

            if (item.IsList)
            {
                var found = FindById(item.Children, id);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private static TodoItem? FindParent(IEnumerable<TodoItem> items, string id, TodoItem? parent)
    {
        foreach (var item in items)
        {
            if (item.Id == id)
            {
                return parent;
            }

            if (item.IsList)
            {
                var found = FindParent(item.Children, id, item);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private static void CollectTodos(IEnumerable<TodoItem> items, List<TodoItem> output, bool includeCompleted)
    {
        foreach (var item in items)
        {
            if (item.IsList)
            {
                CollectTodos(item.Children, output, includeCompleted);
                continue;
            }

            if (includeCompleted || !item.Completed)
            {
                output.Add(item);
            }
        }
    }

    private static List<TodoItem> SortItems(IEnumerable<TodoItem> items, bool includeCompleted)
    {
        var query = items;
        if (!includeCompleted)
        {
            query = query.Where(item => item.IsList || !item.Completed);
        }

        return query
            .OrderBy(item => item.IsList ? 0 : 1)
            .ThenBy(item => item.Completed ? 1 : 0)
            .ThenBy(item => item.DueTime.HasValue ? 0 : 1)
            .ThenBy(item => item.DueTime ?? DateTime.MaxValue)
            .ThenBy(item => item.CreatedAt)
            .ToList();
    }

    private void SortAll()
    {
        SortList(_items);
    }

    private void SortList(List<TodoItem> items)
    {
        var sorted = SortItems(items, includeCompleted: true);
        items.Clear();
        items.AddRange(sorted);
        foreach (var item in items)
        {
            if (item.IsList && item.Children.Count > 0)
            {
                SortList(item.Children);
            }
        }
    }

    private sealed class TodoStoreData
    {
        public List<TodoItem> Items { get; set; } = new();
    }
}
