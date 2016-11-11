using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoRepo.Tests.Pocos;
using System.Collections.Generic;
using System.Threading.Tasks;
using NoRepo.Tests.Docs;

namespace NoRepo.Tests
{
    [TestClass]
    public class AllTests
    {
        private static string key = "[set your key here]";
        private static string endpoint = "[set your DocumentDb endpoint here]";
        private static string dbName = "[set your db name here]";
        private static string collection = "[set your collection name here]";

        [TestMethod]
        public async Task DocumentDbRepoTest()
        {
            var repo = new DocumentDbRepo(endpoint, key, dbName, collection);

            foreach (var contact0 in (await repo.Where<Contact>(c => c._docType== "Contact")).ToList())
                await repo.Remove(contact0.id);

            var contact = new Contact()
            {
                DateAdded = DateTime.UtcNow,
                FirstName = "John",
                LastName = "Doe",
                Addresses = new List<Address>(),
                _docType = "Contact"
            };

            contact.Addresses.Add(new Address()
            {
                Line1 = "123 Sample st.",
                Line2 = null,
                CountryCode = "US",
                StateCode = "IL",
                PostalCode = "9876543"
            });

            contact.id = await repo.Create<Contact>(contact);
            Assert.IsTrue(!String.IsNullOrWhiteSpace(contact.id));

            var contact2 = await repo.Get<Contact>(contact.id);
            DoPocoAsserts(contact, contact2);

            var contact3 = await repo.First<Contact>(c => c.id == contact.id);
            DoPocoAsserts(contact, contact3);

            var contact4 = await repo.FirstOrDefault<Contact>(c => c.FirstName == contact.FirstName && c.LastName == contact.LastName);
            DoPocoAsserts(contact, contact4);

            var contact5 = (await repo.Where<Contact>(c => c.FirstName == contact.FirstName && c.LastName == contact.LastName)).ToList();
            DoPocoAsserts(contact, contact5[0]);

            var query = String.Format("select * from c where c._docType ='Contact' and c.id = '{0}'", contact.id);
            var contact6 = (await repo.Query<Contact>(query)).ToList();
            DoPocoAsserts(contact, contact6[0]);

            contact.FirstName = "William";
            await repo.Upsert<Contact>(contact.id, contact);
            var contact7 = await repo.FirstOrDefault<Contact>(c => c.id == contact.id);
            DoPocoAsserts(contact, contact7);

            await repo.Remove(contact.id);
            var contact8 = await repo.FirstOrDefault<Contact>(c => c.id == contact.id);

            Assert.IsNull(contact8);
        }

        [TestMethod]
        public async Task DocumentBaseTest()
        {
            RepoContext.AddDocumentDbRepo(endpoint, key, dbName, collection);

            foreach (var c in await ContactDoc.GetAll())
                await ContactDoc.Remove(c.id);

            var contact = new ContactDoc()
            {
                DateAdded = DateTime.UtcNow,
                FirstName = "John",
                LastName = "Doe",
                Addresses = new List<Address>()
            };

            contact.Addresses.Add(new Address()
            {
                Line1 = "123 Sample st.",
                Line2 = null,
                CountryCode = "US",
                StateCode = "IL",
                PostalCode = "9876543"
            });

            await ContactDoc.Create(contact);

            var contact2 = await ContactDoc.Get(contact.id);
            DoDocAsserts(contact, contact2);

            var contact3 = await ContactDoc.First(c => c.FirstName == contact.FirstName && c.LastName == contact.LastName);
            DoDocAsserts(contact, contact3);

            var contact4 = await ContactDoc.FindById(contact.id);
            DoDocAsserts(contact, contact4);

            var contact5 = (await ContactDoc.Where(c => c.FirstName == contact.FirstName && c.LastName == contact.LastName)).ToList();
            DoDocAsserts(contact, contact5[0]);

            var query = String.Format("select * from c where c._docType ='ContactDoc' and c.id = '{0}'", contact.id);
            var contact6 = (await ContactDoc.Query(query)).ToList();
            DoDocAsserts(contact, contact6[0]);

            contact.FirstName = "William";
            await ContactDoc.Upsert(contact);
            var contact7 = await ContactDoc.First(c => c.FirstName == "William");
            DoDocAsserts(contact, contact7);

            await ContactDoc.Remove(contact.id);
            var contact8 = await ContactDoc.FirstOrDefault(c => c.id == contact.id);

            Assert.IsNull(contact8);
        }


        private static void DoPocoAsserts(Contact contact, Contact contact2)
        {
            Assert.AreEqual(contact.FirstName, contact2.FirstName);
            Assert.AreEqual(contact.LastName, contact2.LastName);
            Assert.AreEqual(contact.DateAdded, contact2.DateAdded);
            Assert.AreEqual(contact.id, contact2.id);
            Assert.IsNotNull(contact2.Addresses);
            Assert.AreEqual(contact.Addresses.Count, contact2.Addresses.Count);

            Assert.AreEqual(contact.Addresses[0].Line1, contact2.Addresses[0].Line1);
            Assert.AreEqual(contact.Addresses[0].Line2, contact2.Addresses[0].Line2);
            Assert.AreEqual(contact.Addresses[0].CountryCode, contact2.Addresses[0].CountryCode);
            Assert.AreEqual(contact.Addresses[0].PostalCode, contact2.Addresses[0].PostalCode);
            Assert.AreEqual(contact.Addresses[0].StateCode, contact2.Addresses[0].StateCode);
        }

        private static void DoDocAsserts(ContactDoc contact, ContactDoc contact2)
        {
            Assert.AreEqual(contact.FirstName, contact2.FirstName);
            Assert.AreEqual(contact.LastName, contact2.LastName);
            Assert.AreEqual(contact.DateAdded, contact2.DateAdded);
            Assert.AreEqual(contact.id, contact2.id);
            Assert.IsNotNull(contact2.Addresses);
            Assert.AreEqual(contact.Addresses.Count, contact2.Addresses.Count);

            Assert.AreEqual(contact.Addresses[0].Line1, contact2.Addresses[0].Line1);
            Assert.AreEqual(contact.Addresses[0].Line2, contact2.Addresses[0].Line2);
            Assert.AreEqual(contact.Addresses[0].CountryCode, contact2.Addresses[0].CountryCode);
            Assert.AreEqual(contact.Addresses[0].PostalCode, contact2.Addresses[0].PostalCode);
            Assert.AreEqual(contact.Addresses[0].StateCode, contact2.Addresses[0].StateCode);
        }

    }
}
