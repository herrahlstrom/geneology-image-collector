namespace GenPhoto
{
    public class AppState
    {
        public event EventHandler<Guid>? OpenImageRequest;

        public event EventHandler<Guid>? OpenPersonRequest;

        public void OpenImage(Guid id) => OnOpenImageRequest(id);

        public void OpenPerson(Guid id) => OnOpenPersonRequest(id);

        protected virtual void OnOpenImageRequest(Guid e)
        {
            OpenImageRequest?.Invoke(this, e);
        }

        protected virtual void OnOpenPersonRequest(Guid e)
        {
            OpenPersonRequest?.Invoke(this, e);
        }
    }
}