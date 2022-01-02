// See https://aka.ms/new-console-template for more information
using LightADO.Test;

Console.WriteLine("Hello, World!");

new Category().GetListOfCategory().ForEach(x => Console.WriteLine(x.Name));