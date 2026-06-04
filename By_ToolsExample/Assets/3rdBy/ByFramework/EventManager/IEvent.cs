namespace _3rdBy.MetaFramework.EventNotice
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public interface IEvent
    {
        Type GetEventType();
    }
    
    public interface IEventClass: IEvent
    {
        void Handle(object a);
    }
    
    [Event]
    public abstract class AEventClass<A>: IEventClass where A: class
    {
        public Type GetEventType()
        {
            return typeof (A);
        }
    
        protected abstract void Run(object a);
    
        public void Handle(object a)
        {
            try
            {
                Run(a);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
    	
    [Event]
    public abstract class AEvent<A>: IEvent where A: struct
    {
        public Type GetEventType()
        {
            return typeof (A);
        }
    
        protected abstract void Run(A a);
    
        public void Handle(A a)
        {
            try
            {
                Run(a);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
    	
    [Event]
    public abstract class AEventAsync<A>: IEvent where A: struct
    {
        public Type GetEventType()
        {
            return typeof (A);
        }
    
        protected abstract UniTask Run(A a);
    
        public async UniTask Handle(A a)
        {
            try
            {
                await Run(a);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}