namespace ChineseSaleApi.Services
{
    public class CacheSettings
    {
        public CategoriesSettings Categories { get; set; } = new();
        public LotteriesSettings Lotteries { get; set; } = new();
        public PackagesSettings Packages { get; set; } = new();
        public GiftsSettings Gifts { get; set; } = new();
    }

    public class CategoriesSettings
    {
        public int GetAllTtlHours { get; set; } = 24;
        public int GetByIdTtlHours { get; set; } = 24;
    }

    public class LotteriesSettings
    {
        public int GetAllTtlHours { get; set; } = 12;
        public int GetByIdTtlHours { get; set; } = 12;
    }

    public class PackagesSettings
    {
        public int GetAllByLotteryTtlHours { get; set; } = 6;
        public int GetByIdTtlHours { get; set; } = 6;
    }

    public class GiftsSettings
    {
        public int GetAllByLotteryTtlHours { get; set; } = 5;
        public int GetByIdTtlHours { get; set; } = 5;
        public int GetCountByLotteryTtlHours { get; set; } = 5;
    }
}