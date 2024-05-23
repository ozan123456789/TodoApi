using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace TodoApi.Todos
{
    
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class TodosController : ControllerBase
        {
            private readonly TodoDbContext _dbContext;
            private readonly CurrentUser _currentUser;

            public TodosController(TodoDbContext dbContext, CurrentUser currentUser)
            {
                _dbContext = dbContext;
                _currentUser = currentUser;
            }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodos()
            {
                var todos = await _dbContext.Todos
                    .Where(todo => todo.OwnerId == _currentUser.Id)
                    .Select(t => t.AsTodoItem())
                    .AsNoTracking()
                    .ToListAsync();

                return Ok(todos);
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<TodoItem>> GetTodoById(int id)
            {
                var todo = await _dbContext.Todos.FindAsync(id);

                if (todo == null || (todo.OwnerId != _currentUser.Id && !_currentUser.IsAdmin))
                {
                    return NotFound();
                }

                return Ok(todo.AsTodoItem());
            }

            [HttpPost]
            public async Task<ActionResult<TodoItem>> CreateTodoItem(TodoItem newTodo)
            {
                var todo = new Todo
                {
                    Title = newTodo.Title,
                    OwnerId = _currentUser.Id
                };

                _dbContext.Todos.Add(todo);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTodoById), new { id = todo.Id }, todo.AsTodoItem());
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateTodoItem(int id, TodoItem updatedTodo)
            {
                if (id != updatedTodo.Id)
                {
                    return BadRequest();
                }

                var todo = await _dbContext.Todos
                    .Where(t => t.Id == id && (t.OwnerId == _currentUser.Id || _currentUser.IsAdmin))
                    .FirstOrDefaultAsync();

                if (todo == null)
                {
                    return NotFound();
                }

                todo.Title = updatedTodo.Title;
                todo.IsComplete = updatedTodo.IsComplete;

                await _dbContext.SaveChangesAsync();

                return Ok();
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteTodoItem(int id)
            {
                var todo = await _dbContext.Todos
                    .Where(t => t.Id == id && (t.OwnerId == _currentUser.Id || _currentUser.IsAdmin))
                    .FirstOrDefaultAsync();

                if (todo == null)
                {
                    return NotFound();
                }

                _dbContext.Todos.Remove(todo);
                await _dbContext.SaveChangesAsync();

                return Ok();
            }
        }
    

}
