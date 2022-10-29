using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StasCourseProject.Database;
using StasCourseProject.Dto;
using Task = StasCourseProject.Database.Task;

namespace StasCourseProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TaskController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    [ProducesResponseType(typeof(List<Task>), 200)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _context.Tasks.ToListAsync());
    }
    
    [HttpGet("{taskId:int}")]
    [ProducesResponseType(typeof(Task), 200)]
    public async Task<IActionResult> GetByTaskId(int taskId)
    {
        return Ok(await _context.Tasks.FirstOrDefaultAsync(x => x.Id == taskId));
    }
    
    [HttpGet("complete/{taskId:int}")]
    [ProducesResponseType(typeof(Task), 200)]
    public async Task<IActionResult> CompleteTaskById(int taskId)
    {
        var result = _context.Tasks.FirstOrDefault(x => x.Id == taskId);
        if (result == null)
        {
            return NotFound();
        }

        if (result.FinishDateTime == null)
        {
            result.FinishDateTime = DateTime.Now;
        }
        else
        {
            result.FinishDateTime = null;
        }

        _context.Tasks.Update(result);
        
        await _context.SaveChangesAsync();
        
        return Ok(result);
    }
    
    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(List<Task>), 200)]
    public async Task<IActionResult> GetByUserId(string userId)
    {
        List<Task> result = await _context.Tasks.AsNoTracking().Where(x => x.UserId == userId).ToListAsync();
        return Ok(result);
    }
    
    [HttpGet("manager/{userId}")]
    [ProducesResponseType(typeof(List<Task>), 200)]
    public async Task<IActionResult> GetByManagerId(string userId)
    {
        List<Task> result = await _context.Tasks.AsNoTracking().Where(x => x.ManagerId == userId).ToListAsync();
        return Ok(result);
    }

    [HttpPost("")]
    [ProducesResponseType(typeof(Task), 200)]
    public async Task<IActionResult> CreateTask([FromBody] CreatingTask task)
    {
        var newTask = new Task()
        {
            AssignDateTime = DateTime.Now,
            FinishDateTime = null,
            Title = task.Title,
            Description = task.Description,
            ManagerId = task.ManagerId,
            UserId = task.UserId
        };
        await _context.Tasks.AddAsync(newTask);
        await _context.SaveChangesAsync();

        return Ok(newTask);
    }
    
    [HttpDelete("{taskId:int}")]
    [ProducesResponseType(typeof(Task), 200)]
    public async Task<IActionResult> DeleteByTaskId(int taskId)
    {
        var result = _context.Tasks.AsNoTracking().FirstOrDefault(x => x.Id == taskId);
        if (result == null) return Ok();
        _context.Tasks.Remove(result);
        await _context.SaveChangesAsync();
        return Ok();
    }
}