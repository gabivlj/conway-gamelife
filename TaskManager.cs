﻿using System;
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
        public TaskReader(int maxConcurrentTasks)
        {
            queue = new Queue<TaskHolder>();
            resource = new Semaphore(1, 1);
            queueResources = new Semaphore(0, maxConcurrentTasks);
        }

        private Queue<TaskHolder> queue;
        private Semaphore resource;
        private Semaphore queueResources;

        public void Send(Task task, object parameter)
        {
            // Wait that the queue is ready to get enqueued
            resource.WaitOne();

            queue.Enqueue(new TaskHolder(task, parameter));

            resource.Release();

            // Tell threads that there is a new task
            queueResources.Release();
        }

        public void Finish()
        {
            // Release all semaphores once and empty the queue, this special case will be handled below.
            resource.WaitOne();            
            queue.Clear();
            resource.Release();
            queueResources.Release();
        }

        public TaskHolder GetOne()
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
            TaskHolder t = queue.Dequeue();
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
                    TaskHolder taskHolder = reader.GetOne();
                    if (taskHolder == null)
                    {
                        continue;
                    }
                    taskHolder.task(taskHolder.parameter);
                }
                state = State.Finished;
            });
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

        public TaskManager(int size, int maxConcurrentTasks)
        {
            reader = new TaskReader(maxConcurrentTasks);
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
