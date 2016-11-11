# NoRepo : { Not Only a Repository Pattern Implemantation }

- A very thin library to easily implement the repo pattern with minimal code for no-sql databases 
- Includes an implementation for Azure DocumentDb.
- Supports partitioned collections


## Usage
You can choose one choose one of two modes of usage.
- Direct
- Inheritance & attributes

## Direct mode
```c#
using NoRepo;

var repo = DocumentDbRepo(endpoint, key, dbName, collection);

// Create a document
customerInstance.id = repo.Create<Customer>(customerInstance);

// Upsert a document
customerInstance.id = repo.Upsert<Customer>(customerInstance);

// Get an existing 
var customer = repo.Get<Customer>(id);

// First or Default
var customer = repo.FirstOrDefault<Customer>(c => c.FirstName == "John" && c.LastName == "Coltrane");

// Where
var customers = repo.Where<Customer>(c => c.LastName == "Coltrane");

// Query
var customers = repo.Query<Customer>("select * from c where c.LastName = 'Coltraner'");

// Remove
repo.Remove(customer.id);

```
