using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoListApi.DTOs;
using TodoListApi.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TodoListApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly TodoListContext _context;
           
        public TaskController(TodoListContext context)
        {
            _context = context;
        }

        // GET: api/<TaskController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Task>>> Get()
        {
            return await _context.Tasks.ToListAsync();
        }

        // GET api/<TaskController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Task>> Get(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return task;            
        }

        // POST api/<TaskController>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Models.Task>> Post([FromBody] TaskDto task)
        {  
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }
            var mapperConfig = new MapperConfiguration(cfg => cfg.CreateMap<TaskDto, Models.Task>());
            var mapper = new Mapper(mapperConfig);
            var taskEntity = mapper.Map<Models.Task>(task);
            taskEntity.UserId = (int) user.Id;
            _context.Tasks.Add(taskEntity);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = taskEntity.Id }, taskEntity);
        }

        // PUT api/<TaskController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Models.Task task)
        {
            if (id != task.Id)
            {
                return BadRequest();
            }
            _context.Entry(task).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }   
        }
        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
        // DELETE api/<TaskController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
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

        [HttpGet("my-tasks")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Models.Task>>> GetMyTasks()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return NotFound();
            }
            var tasks = await _context.Tasks.Where(t => t.UserId == user.Id).ToListAsync();
            // exclude user info
            tasks.ForEach(t => t.User = null);
            return tasks;
        }
    }
}
