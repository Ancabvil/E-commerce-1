using TuringClothes.Database;
using TuringClothes.Dtos;
using TuringClothes.Helpers;
using TuringClothes.Model;
using TuringClothes.Repository;

namespace TuringClothes.Services
{
    public class ImageService
    {
        private const string IMAGES_FOLDER = "images";
        private readonly UnitOfWork _unitOfWork;

        public ImageService(UnitOfWork unitOfWork, ImageRepository imageRepository)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Image> InsertAsync(CreateUpdateImageRequest image)
        {
            string relativePath = $"{IMAGES_FOLDER}/{Guid.NewGuid()}_{image.File.FileName}";

            Image newImage = new Image
            {
                Name = image.Name,
                Path = relativePath
            };

            await _unitOfWork._imageRepository.InsertAsync(newImage);

            if (await _unitOfWork.SaveChangesAsync())
            {
                await StoreImageAsync(relativePath, image.File);
            }

            return newImage;
        }

        public async Task<string> InsertImage(IFormFile file)
        {
            string path = $"{IMAGES_FOLDER}{Guid.NewGuid()}_{file.FileName}";
            await StoreImageAsync(path, file);
            return path;
        }

        private async Task StoreImageAsync(string path, IFormFile file)
        {
            using Stream stream = file.OpenReadStream();
            await FileHelper.SaveAsync(stream, path);
        }
    }
}
