using System;

namespace Code.QM.Util
{
    public delegate void Listener<in TD>(TD data);

    public delegate void Listener();
    
    public class Observable <TData>
    {

        private event Listener<TData> MyEvent;
        
        public void AddListener(Listener<TData> listener)
        {
            MyEvent -= listener;
            MyEvent += listener;
        }
        
        public void RemoveListener(Listener<TData> listener)
        {
            MyEvent -= listener;
        }

        public void Invoke(TData data)
        {
            MyEvent?.Invoke(data);
        }
    }
    
    public class Observable
    {

        private event Listener MyEvent;
        
        public void AddListener(Listener listener)
        {
            MyEvent -= listener;
            MyEvent += listener;
        }
        
        public void RemoveListener(Listener listener)
        {
            MyEvent -= listener;
        }

        public void Invoke()
        {
            MyEvent?.Invoke();
        }
    }

}