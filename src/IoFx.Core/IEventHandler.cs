namespace IoFx
{    
    interface IHandler<in TValue>
    {
        void OnEvent(TValue entry);
    }
}
