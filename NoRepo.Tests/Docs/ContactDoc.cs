using NoRepo.Tests.Pocos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoRepo.Tests.Docs
{
    [Storable("Accounts")]
    public class ContactDoc : DocumentBase<ContactDoc>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateAdded { get; set; }
        public List<Address> Addresses { get; set; }

    }
}
