using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Swashbuckle.AspNetCore.Annotations;
using TodoActor.Interfaces;

namespace Web.Controllers
{
    /// <summary>
    /// Handles todo-lists
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Produces("application/json")]
    [Route("api/Todo")]
    public class TodoController : Controller
    {
        /// <summary>
        /// Gets the todo items for the specified email
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>A list of todo items</returns>
        [HttpGet]
        [SwaggerResponse(200, "The list of todo items was returned", typeof(IEnumerable<TodoItem>))]
        [SwaggerResponse(404, "Could not find items for specified email")]
        public async Task<IActionResult> GetList([FromQuery]string email,
            CancellationToken cancellationToken)
        {
            ITodoActor todoActor = GetTodoActorProxy(email);
            List<TodoItem> list = await todoActor.GetItems(cancellationToken);
            if (list != null)
                return Ok(list.Select(item => new TodoItem
                {
                    Description = item.Description,
                    DateAdded = item.DateAdded,
                    DateFinished = item.DateFinished,
                    Finished = item.Finished
                }));
            return NotFound();
        }

        /// <summary>
        /// Adds a new item to the list
        /// </summary>
        /// <param name="email">The email</param>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Ok if the operation was successfull</returns>
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [SwaggerResponse(200, "The item was added")]
        public async Task<IActionResult> AddItem([FromQuery]string email,
            AddTodoItemRequest request, CancellationToken cancellationToken)
        {
            ITodoActor todoActor = GetTodoActorProxy(email);
            await todoActor.AddItem(new TodoItem
            {
                Description = request.Description,
                DateAdded = DateTime.Now,
                DateFinished = DateTime.MinValue,
                Finished = false
            }, cancellationToken);
            return Ok();
        }

        private static ITodoActor GetTodoActorProxy(string email)
        {
            return ActorProxy.Create<ITodoActor>(new ActorId(email));
        }
    }

    public class AddTodoItemRequest
    {
        /// <summary>
        /// The description of the item
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The Id of the item
        /// </summary>
        public Guid Id { get; set; }
    }

}