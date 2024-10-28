using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class TasksController : ControllerBase
{
    private readonly TaskContext _context;
    public TasksController(TaskContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all tasks with optional filtering and pagination.
    /// </summary>
    /// <param name="isCompleted">Filter by completion status.</param>
    /// <param name="pageNumber">Page number for pagination.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>A list of tasks.</returns>
    [ProducesResponseType(typeof(IEnumerable<Task>), 200)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAllTasks(
        [FromQuery] bool? isCompleted = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        IQueryable<TaskItem> query = _context.Tasks.AsQueryable();

        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsDone == isCompleted.Value);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var tasks = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(
            new
            {
                totalCount,
                pageSize,
                currentPage = pageNumber,
                totalPages
            }
        ));

        return Ok(tasks);

    }

    /// <summary>
    /// Retrieves a task find by id.
    /// </summary>
    /// <param name="id">Id.</param>
    /// <returns>The required task, or a 404 Not Found if it doesn't exist.</returns>
    [ProducesResponseType(typeof(TaskItem), 201)]
    [ProducesResponseType(404)]
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        return Ok(task);
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <param name="task">A task object to be created</param>
    /// <returns>The created task</returns>
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
    {
        if (task == null)
        {
            return BadRequest();
        }
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return CreatedAtAction("GetTask", new { id = task.Id }, task);
    }

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    /// <param name="id">Id of the task to update</param>
    /// <param name="task">The updated task object</param>
    /// <returns>No content if sucessful, or a 404 Not Found if the task doesn't exist.</returns>
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskItem task)
    {
        if (task.Id != id)
        {
            return BadRequest();
        }
        _context.Entry(task).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();

        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Tasks.Any(t => t.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing task.
    /// </summary>
    /// <param name="id">Id of the task to delete</param>
    /// <returns>No content if sucessful, or 404 Not Found if the task doesn't exist.</returns>
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
