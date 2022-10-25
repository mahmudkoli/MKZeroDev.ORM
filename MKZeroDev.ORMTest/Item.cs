using MKZeroDev.ORM;

namespace MKZeroDev.ORMTest
{
    public abstract class Base
    {
        [PrimaryKey]
        public Guid Id { get; set; } = default!;
    }

    [TableName("Categories")]
    public class Category : Base
    {
        [ColumnDef("Category_Name")]
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int Count { get; set; } = default!;
        public List<Product> Products { get; set; } = default!;

        public override string ToString()
        {
            return $"Id: {Id}\nName: {Name}\nDescription: {Description}";
        }
    }

    [TableName("Products")]
    public class Product : Base
    {
        [ColumnDef("Product_Name")]
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int Quantity { get; set; } = default!;
        public decimal UnitPrice { get; set; } = default!;
        public decimal? TotalPrice { get; set; } = default!;
        public bool IsDefault { get; set; }
        public Guid? CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        public override string ToString()
        {
            return $"Id: {Id}\nName: {Name}\nDescription: {Description}\nQuantity: {Quantity}\nUnitPrice: {UnitPrice}\nTotalPrice: {TotalPrice}\nIsDefault: {IsDefault}\nCategoryId: {CategoryId}";
        }
    }

    [TableName("Results")]
    public class Result : Base
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public float TotalMark { get; set; } = default!;
        public float Mark { get; set; } = default!;

        public override string ToString()
        {
            return $"Id: {Id}\nName: {Name}\nDescription: {Description}";
        }
    }
}
