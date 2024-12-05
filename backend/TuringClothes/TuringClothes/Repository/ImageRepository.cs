using TuringClothes.Database;

namespace TuringClothes.Repository
{
    public class ImageRepository : Repository<Image, int>
    {
        public ImageRepository(MyDatabase myDatabase) : base(myDatabase)
        {
        }
    }
}
