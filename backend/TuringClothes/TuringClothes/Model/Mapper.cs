
using TuringClothes.Database;
using TuringClothes.Dtos;
using TuringClothes.Extensions;

namespace TuringClothes.Model
{
    public class Mapper
    {
        public ImageDto ToDto(Image image, HttpRequest httpRequest = null)
        {
            return new ImageDto()
            {
                Id = image.Id,
                Name = image.Name,
                Url = httpRequest is null ? image.Path : httpRequest.GetAbsoluteUrl(image.Path),
            };
        }

        public IEnumerable<ImageDto> ToDto(IEnumerable<Image> images, HttpRequest httpRequest = null)
        {
            return images.Select(image => ToDto(image, httpRequest));
        }
    }
}
