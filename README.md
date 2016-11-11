# NoRepo : { Not Only a Repository Pattern Implemantation }

- A very thin library to easily implement the repo pattern with minimal code. 
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

```
