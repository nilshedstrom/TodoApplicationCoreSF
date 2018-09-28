using System;


namespace TodoActor.Interfaces
{
    /// <summary>
    /// Item in a todo-list
    /// </summary>
    public class TodoItem
    {
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateFinished { get; set; }
        public bool Finished { get; set; }
    }
}