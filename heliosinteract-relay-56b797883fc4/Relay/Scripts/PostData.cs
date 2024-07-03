namespace Helios.Relay
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    public class PostData : IPostData
    {
        [ModelBinder(BinderType = typeof(FormDataJsonBinder))]
        public RExperience Experience { get; set; }

        [ModelBinder(BinderType = typeof(FormDataJsonBinder))]
        public RGuest Guest { get; set; }

        [ModelBinder(BinderType = typeof(FormDataJsonBinder))]
        public RFile FileInfo { get; set; }

        public string Key { get; set; }
        
        public RelayService RelayService { get; set; }

        public IFormFile File { get; set; }
    }

    public interface IPostData
    {
        RelayService RelayService { get; set; }
        string Key { get; set; }
        RExperience Experience { get; set; }
        RGuest Guest { get; set; }
        RFile FileInfo { get; set; }
        IFormFile File { get; set; }
    }
}