using System;

namespace TripleMatch.Command
{
    /// <summary>
    /// Represents a command dispatcher.
    /// But reactive animation handles concurrency on items, so the queue does not need an async loop.
    /// </summary>
    public class CommandQueue : ICommandQueue
    {
        public void Enqueue(ICommand command)
        {
            command?.Execute();
        }
    }
}
