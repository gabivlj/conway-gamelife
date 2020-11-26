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

    /// <summary>
    /// Writes and reads tasks in a safe multithreaded way.
    /// </summary>
    class TaskReaderWriter
    {
        public TaskReaderWriter()
        {
            queue = new Queue<TaskHolder>(1000);
            resource = new Semaphore(1, 1);
            localRes = new Semaphore(1, 1);
            handle = new EventWaitHandle(false, EventResetMode.ManualReset);
            batch = new Queue<TaskHolder>(1000);
        }

        #region Private
        private Queue<TaskHolder> queue;
        private Queue<TaskHolder> batch;
        private Semaphore resource;        
        private Semaphore localRes;
        private EventWaitHandle handle;

        #endregion

        #region Public

        /// <summary>
        /// Sends a task and a parameter to the Queue
        ///
        /// </summary>
        /// <safety>
        /// It is safe single and multithreaded.
        /// </safety>
        /// <param name="task"></param>
        /// <param name="parameter"></param>
        public void Send(Task task, object parameter)
        {            
            // Wait that the queue is ready to get enqueued
            resource.WaitOne();

            queue.Enqueue(new TaskHolder(task, parameter));

            // Tell threads that there is a new task
            if (queue.Count == 1)
            {
                handle.Set();
            }

            resource.Release();                    

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
                handle.Reset();

                // Release Semaphore for the resources
                localRes.Release();

                return null;
            }
            
            // Get it and release queue semaphore 
            TaskHolder t = queue.Dequeue();

            Queue<TaskHolder> emptyQueue = batch;

            // swap current elements of the queue to the batcher so we don't block the queue for next element
            batch = queue;

            // swap queue
            queue = emptyQueue;
            
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

        public ThreadDoer(TaskReaderWriter reader)
        {            
            thread = new Thread(() =>
            {
                state = State.Initialized;
                while (true)
                {
                    TaskHolder taskHolder = reader.GetOne();
                    if (taskHolder == null)
                    {
                        // no more tasks and the user wants to finish
                        if (state == State.Ending)
                        {
                            break;
                        }
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
        private TaskReaderWriter reader;
        #endregion

        #region Public

        #endregion

        public TaskManager(int size)
        {
            reader = new TaskReaderWriter();
            threads = new ThreadDoer[size];
            for (int i = 0; i < size; ++i)
            {
                threads[i] = new ThreadDoer(reader);
            }
        }

        #region Public Methods

        /// <summary>
        /// Sends a task to the TaskManager
        ///
        /// It's safe to do anywhere, even sharing across threads.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="parameter"></param>
        public void Do(Task task, object parameter = null)
        {
            reader.Send(task, parameter);
        }

        public void Finish()
        {
            for (int i = 0; i < threads.Length; ++i)
            {
                // end all of the leftover tasks and finish
                threads[i].End();
            }
        }

        #endregion

        #region Private Methods


        #endregion
    }
}
