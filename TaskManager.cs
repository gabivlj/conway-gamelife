using System;
using System.Collections.Generic;
using System.Threading;

namespace testconway
{
    public delegate void Task(object parameter);

    class TaskHolder
    {
        public object parameter;
        public Task task;

        public TaskHolder(Task task, object parameter)
        {
            this.task = task;
            this.parameter = parameter;
        }
    }

    class TaskReader
    {
        public TaskReader()
        {
            queue = new Queue<TaskHolder>();
            resource = new Semaphore(1, 1);
            localRes = new Semaphore(1, 1);
            handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            batch = new Queue<TaskHolder>();
        }

        #region Private
        private Queue<TaskHolder> queue;
        private Queue<TaskHolder> batch;
        private Semaphore resource;        
        private Semaphore localRes;
        private EventWaitHandle handle;

        #endregion

        #region Public
        public void Send(Task task, object parameter)
        {            
            // Wait that the queue is ready to get enqueued
            resource.WaitOne();

            queue.Enqueue(new TaskHolder(task, parameter));

            
            resource.Release();

            // Tell threads that there is a new task
            handle.Set();

        }

        /// <summary>
        /// TODO Test this
        /// </summary>
        public void Finish()
        {
            
        }

        /// <summary>
        /// Gets one task from the queue, if there is none it will return null.
        /// </summary>
        /// <returns>Returns null if there is not a task, else a TaskHolder</returns>
        public TaskHolder GetOne()
        {
            // Only one thread can access the queue semaphore
            localRes.WaitOne();
            // We use a localBatcher that doesn't block the queue
            if (batch.Count != 0)
            {
                TaskHolder ts = batch.Dequeue();
                localRes.Release();
                return ts;
            }
            // Make sure no one is accessing the queue
            resource.WaitOne();

            // If it is empty, wait for signal
            if (queue.Count == 0)
            {
                // Release queue semaphore so a new task can be added
                resource.Release();
                // Wait until there is a new task in the queue.
                handle.WaitOne();
                // Release Semaphore for the resources
                localRes.Release();
                return null;
            }
            
            // Get it and release queue semaphore 
            TaskHolder t = queue.Dequeue();
            // swap current elements of the queue to the batcher so we don't block the queue for next element
            batch = queue;
            // reinitialize queue
            queue = new Queue<TaskHolder>();
            // Release everything, so a new thread can access the queue mutex, and also there can be new elements
            // added into the queue
            resource.Release();
            localRes.Release();
            return t;
        }

        #endregion



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
                while (true)
                {
                    TaskHolder taskHolder = reader.GetOne();
                    if (taskHolder == null)
                    {
                        if (state == State.Ending)
                            break;
                        continue;
                    }
                    taskHolder.task(taskHolder.parameter);
                }
                state = State.Finished;                
            });
            thread.IsBackground = true;
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

        public TaskManager(int size)
        {
            reader = new TaskReader();
            threads = new ThreadDoer[size];
            for (int i = 0; i < size; ++i)
            {
                threads[i] = new ThreadDoer(reader);
            }
        }

        #region Public Methods

        public void Do(Task task, object parameter = null)
        {
            reader.Send(task, parameter);
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
