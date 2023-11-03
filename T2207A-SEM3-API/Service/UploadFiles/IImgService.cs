namespace T2207A_SEM3_API.Service.UploadFiles
{
    public interface IImgService
    {
        Task<string> UploadImageAsync(IFormFile img);
    }
}
