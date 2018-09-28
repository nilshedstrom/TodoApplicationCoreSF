using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using TodoActor.Interfaces;

namespace TodoActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class TodoActor : Actor, ITodoActor, IRemindable
    {
        private const string Items = "Items";

        private const string SendReminderEmail = "SendReminderEmail";

        /// <summary>
        /// Initializes a new instance of TodoActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public TodoActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            await RegisterReminderAsync(SendReminderEmail, new byte[0],
                TimeSpan.FromSeconds(10),
                TimeSpan.FromHours(24));
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            switch (reminderName)
            {
                case SendReminderEmail:
                    //TODO: Send reminder email
                    break;
            }
        }

        public async Task<List<TodoItem>> GetItems(CancellationToken cancellationToken)
        {
            ConditionalValue<List<TodoItem>> items =
                await StateManager.TryGetStateAsync<List<TodoItem>>(Items, cancellationToken);
            return items.HasValue ? items.Value : null;
        }

        public async Task<bool> AddItem(TodoItem todoItem, CancellationToken cancellationToken)
        {
            await StateManager.AddOrUpdateStateAsync(Items, new List<TodoItem> { todoItem }, (s, list) =>
            {
                list.Add(todoItem);
                return list;
            }, cancellationToken);
            await SaveStateAsync();
            return true;
        }
    }
}
