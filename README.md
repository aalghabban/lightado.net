
![enter image description here](https://api.nuget.org/v3-flatcontainer/lightado.net/6.0.2/icon)

# Welcome to LightAdo!

Let's face it, writing SQL Server validation, casting and business logic boilerplate is a drag. That's why we wrote LighAdo 

# Getting Started. 

To Strat with lightado open your terminal and dir to you project and install lightado package as following: 

    dotnet add package LightAdo.net --version 6.0.2

## # Setting up your connection

Open up the app settings.json and add new connection string with name  DefaultConnection as following: 

    "ConnectionStrings":  {
        "DefaultConnection": "Data Source=.\\SQLEXPRESS;Initial Catalog=Northwind;Integrated Security=True"    
     }

### Write your class

let's Suppose you are reading from north winds database table named categories, first let's build the class: 

    namespace LightADO.Test;
    public  class  Category
    {
	    public int CategoryId { get; set; }
	    public string CategoryName { get; set; }
	    public List<Category> GetListOfCategory()
	    {
		    return new Query().ExecuteToListOfObject<Category>("select * from categories",System.Data.CommandType.Text);
	    }
    }

notes that all take to get started is to call Execute To List Of Object methods and passing the SQL query to it, to make things more interested how about we use C# naming conventions unstained of SQL server buy decoring the CategoryId and CategoyName with attributes column name as following: 

    namespace LightADO.Test;
    public  class  Category
    {
	    [ColumnName("CategoryId")]
	    public int Id { get; set; }
	    
	    [ColumnName("CategoryName")]
	    public string Name { get; set; }
	    
	    public List<Category> GetListOfCategory()
	    {
		    return new Query().ExecuteToListOfObject<Category>("select * from categories", System.Data.CommandType.Text);
	    }
    }

# Futuers List

## Auto Mapping. 

Map object from and to database using custom names, types including foreign keys which get populated in the way! 

## Auto Validation

Validate class properties as you like just mark the property with attribute AutoValidation, which allow you to throw your own Exception. 

## CRUD

Create, Read, Update and delete out of the box just inherits the class CrudBase and make sure you setup the SP for it. 

## Data Event

Fire even before, after open and closing connection, before , after execute the commend, catch what you like and change or drop the execution. 

## Async API

All of the method you can call it using Async in case you code required this kind of development. 

## Transactions

Push and execute list of transaction to database using one line of code. 

# Versions

## 6.0.2

Supporting dotnet core 6.0 
