namespace ApplicationWeb.HelperAndConstant
{
    public interface IImageProcessing
    {
        public string GetImagePath(IFormFile photo, string parentFolderName);
        public string GetImagePath(string file, string parentFolderName);
        public string GetImagePathForDb(string imagePath);
    }
}
