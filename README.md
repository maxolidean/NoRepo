# NoRepo : { Not Only a Repository Pattern Implemantation }

- A very thin library to easily implement the repo pattern with minimal code for no-sql databases 
- Includes an implementation for Azure DocumentDb.
- Supports partitioned collections


## Usage
You can choose one of two modes of usage.
- Direct
- Inheritance & attributes

## Direct mode
```c#
using NoRepo;

var repo = DocumentDbRepo(endpoint, key, dbName, collection);

// Create a document
customerInstance.id = await repo.Create<Customer>(customerInstance);

// Upsert a document
customerInstance.id = await repo.Upsert<Customer>(customerInstance);

// Get an existing 
var customer = await repo.Get<Customer>(id);

// First or Default
var customer = await repo.FirstOrDefault<Customer>(c => c.FirstName == "John" && c.LastName == "Coltrane");

// Where
var customers = await repo.Where<Customer>(c => c.LastName == "Coltrane");

// Query
var customers = await repo.Query<Customer>("select * from c where c.LastName = 'Coltraner'");

// Remove
await repo.Remove(customer.id);

```

## Inheritance & Attributes
```c#
using NoRepo;

[Storable("CustomersRepository")]  // Specifies the repository in which this document will be stored. For a DocumentDbRepo this is the collection name.
public class Customer : DocumentBase<Customer>  // Provides persistence logic and "id" attribute.
{
  public string FirstName { get; set; }
  public string LastName { get; set; }
  public string CompanyName { get set;}
  //TODO: other props
}

...

// Adds a new DocumentDbRepo to the context 
RepoContext.AddDocumentDbRepo(endpoint, key, dbName, collection);

// Create
await Customer.Create(customerInstance);
Console.WriteLine("The instance was created with this id: " + customerInstance.id);

// Upsert
var id = Guid.NewId().ToString();
await Customer.Upsert(id, customerInstance);

// Get
Customer customer = await Customer.Get(id);  //throws exception if not exists

// First
Cuastomer customer = await Customer.First(c => c.LastName == "Coltrane");  //throws exception if not found

// First or default
Customer customer = await Customer.FirstOrDefault(c => c.LastName == "Coltrane");  // returns null if not found

//Where 
IEnumerable<Customer> customers = await Customer.Where(c => c.FirstName == "John"); 

//Remove
await Customer.Remove(customer.id);

```

## How to extend to other repositories
- Just create your own implementation of the IRepository interface

```c#
using NoRepo;

public class MyOwnRepo : IRepository
{
  //TODO: Add implementation methods.
}

RepoContext.AddRepository("MyOwnRepo", new MyOwnRepo);

[Storable("MyOwnRepo")] 
public class Customer : DocumentBase<Customer>  // Provides persistence logic and "id" attribute.
{
  public string FirstName { get; set; }
  public string LastName { get; set; }
  public string CompanyName { get set;}
  //TODO: other props
}



```







