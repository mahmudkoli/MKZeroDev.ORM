// See https://aka.ms/new-console-template for more information
using MKZeroDev.ORMTest;

var context = new DatabaseContext("Server =.\\SQLEXPRESS; Database = ORMCore; Trusted_Connection = true");
context.DatabaseUpdate();

//context.Categories.Insert(new Category { Id = Guid.NewGuid(), Name = "TV", Description = "Television", Count = 15 });
//context.Categories.Insert(new Category { Id = Guid.NewGuid(), Name = "PC", Description = "Personal Computer", Count = 10 });

//var catId = Guid.Parse("11BAD26E-54D1-4CF1-8FE0-397625EAC111");
//context.Categories.Insert(new Category { Id = catId, Name = "Category Name 1", Description = "Category Description 1" });
//context.Categories.Update(new Category { Id = catId, Name = "Category Name 2", Description = "Category Description 2" });
//context.Categories.Delete(new Category { Id = catId, Name = "Category Name 1", Description = "Category Description 1" });
foreach (var item in context.Categories.SelectAll())
{
    Console.WriteLine("---------------------------------------");
    Console.WriteLine(item.ToString());
    Console.WriteLine("---------------------------------------");
}
//Console.WriteLine(context.Categories.SelectFirstOrDefault()?.ToString());


//var prodId = Guid.Parse("11BAD26E-54D1-4CF1-8FE0-397625EAC222");
//context.Products.Insert(new Product { Id = prodId, Name = "Product Name 1", Description = "Product Description 1", Quantity = 15, UnitPrice = 10.5M, IsDefault = true });
//context.Products.Update(new Product { Id = prodId, Name = "Product Name 1", Description = "Product Description 1", Quantity = 15, UnitPrice = 10.5M, IsDefault = false });
//context.Products.Delete(new Product { Id = prodId, Name = "Product Name 1", Description = "Product Description 1", Quantity = 15, UnitPrice = 10.5M, IsDefault = false });
//foreach (var item in context.Products.SelectAll())
//{
//    Console.WriteLine("---------------------------------------");
//    Console.WriteLine(item.ToString());
//    Console.WriteLine("---------------------------------------");
//}
//Console.WriteLine(context.Products.SelectFirstOrDefault()?.ToString());




