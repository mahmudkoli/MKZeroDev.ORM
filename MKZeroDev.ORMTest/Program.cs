// See https://aka.ms/new-console-template for more information
using MKZeroDev.ORM;
using MKZeroDev.ORMTest;

//var orm = new ORMExecutor("Server =.\\SQLEXPRESS; Database = ORMCore; Trusted_Connection = true");

//Console.WriteLine(orm.CreateTableIfNotExists<Category>());
//Console.WriteLine(orm.Insert<Category>(new Category { Id = Guid.Parse("65777ABB-F735-4DE1-B1FA-414457E86FCE"), Name = "Category Name 1", Description = "Category Description 1" }));
//Console.WriteLine(orm.Update<Category>(new Category { Id = Guid.Parse("65777ABB-F735-4DE1-B1FA-414457E86FCE"), Name = "Category Name 1", Description = "Category Description 2" }));
//Console.WriteLine(orm.Delete<Category>(new Category { Id = Guid.Parse("65777ABB-F735-4DE1-B1FA-414457E86FCE"), Name = "Category Name 1", Description = "Category Description 1" }));
//foreach (var item in orm.SelectAll<Category>())
//{
//    Console.WriteLine("---------------------------------------");
//    Console.WriteLine(item.ToString());
//    Console.WriteLine("---------------------------------------");
//}
//Console.WriteLine(orm.SelectFirstOrDefault<Category>()?.ToString());

//Console.WriteLine(orm.CreateTable<Product>());
//Console.WriteLine(orm.Insert<Product>(new Product { Id = Guid.Parse("379F66EE-1B44-4E67-81B0-E3CE52B55746"), Name = "Product Name 1", Description = "Product Description 1", Quantity = 15, UnitPrice = 10.5M, IsDefault = true }));
//Console.WriteLine(orm.Update<Product>(new Product { Id = Guid.Parse("379F66EE-1B44-4E67-81B0-E3CE52B55746"), Name = "Product Name 1", Description = "Product Description 1", Quantity = 15, UnitPrice = 10.5M, IsDefault = false }));
//Console.WriteLine(orm.Delete<Product>(new Product { Id = Guid.Parse("379F66EE-1B44-4E67-81B0-E3CE52B55746"), Name = "Product Name 1", Description = "Product Description 1", Quantity = 15, UnitPrice = 10.5M, IsDefault = false }));
//foreach (var item in orm.SelectAll<Product>())
//{
//    Console.WriteLine("---------------------------------------");
//    Console.WriteLine(item.ToString());
//    Console.WriteLine("---------------------------------------");
//}
//Console.WriteLine(orm.SelectFirstOrDefault<Product>()?.ToString());

var context = new DatabaseContext("Server =.\\SQLEXPRESS; Database = ORMCore; Trusted_Connection = true");
