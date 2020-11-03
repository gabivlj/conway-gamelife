using System;
using System.Collections.Generic;
using System.Threading;

namespace testconway
{
    public delegate void Task();

    class TaskReader
    {
        public TaskReader(int maxConcurrentTasks)
        {
            queue = new Queue<Task>();
            resource = new Semaphore(1, 1);
            queueResources = new Semaphore(0, maxConcurrentTasks);
        }

        private Queue<Task> queue;
        private Semaphore resource;
        private Semaphore queueResources;

        public void Send(Task task)
        {
            // Wait that the queue is ready to get enqueued
            resource.WaitOne();

            queue.Enqueue(task);

            resource.Release();

            // Tell threads that there is a new task
            queueResources.Release();
        }

        public void Finish()
        {
            resource.WaitOne();            
            queue.Clear();
            resource.Release();
            queueResources.Release();
        }

        public Task GetOne()
        {
            // Wait for the queue to not be empty so we are not "hanging" for the queue Mutex when it's empty
            queueResources.WaitOne();

            // Dequeue it from the queue
            resource.WaitOne();

            // This means that the TaskReader has been finished!
            if (queue.Count == 0)
            {
                resource.Release();
                queueResources.Release();
                return null;
            }

            // Get it and release queue semaphore 
            Task t = queue.Dequeue();
            resource.Release();

            return t;
        }

    }

    class ThreadDoer
    {
        enum State
        {
            Initialized,            
            Ending,
            Finished,
        }

        private Thread thread;
        private State state;

        public ThreadDoer(TaskReader reader)
        {            
            thread = new Thread(() =>
            {
                state = State.Initialized;
                while (state != State.Ending)
                {
                    Task task = reader.GetOne();
                    if (task == null)
                    {
                        continue;
                    }
                    task();
                }
                state = State.Finished;
            });

            // thread.IsBackground = true;
            thread.Start();
        }

        public void End()
        {
            state = State.Ending;
        }
    }

    public class TaskManager
    {
        #region Private
        private ThreadDoer[] threads;
        private TaskReader reader;

        #endregion

        #region Public

        #endregion

        public TaskManager(int size, int maxAwaitingTasks)
        {
            reader = new TaskReader(maxAwaitingTasks);
            threads = new ThreadDoer[size];
            for (int i = 0; i < size; ++i)
            {
                threads[i] = new ThreadDoer(reader);
            }
        }

        #region Public Methods

        public void Do(Task task)
        {
            reader.Send(task);
        }

        public void Finish()
        {
            reader.Finish();
            for (int i = 0; i < threads.Length; ++i)
            {
                threads[i].End();
            }
        }

        #endregion

        #region Private Methods


        #endregion
    }
}
