namespace Catalog.API.Products.CreateProduct
{
    public class ProductDto
    {
        public string Name { get; set; } = default!;
        public List<string> Category { get; set; } = new List<string>();
        public string Description { get; set; } = default!;
        public string ImageFile { get; set; } = default!;
        public decimal Price { get; set; }
    }
}
