using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoRepo.Tests.Pocos
{
    public class Contact
    {
        public string id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateAdded { get; set; }
        public List<Address> Addresses {get; set;}
        public string _docType { get; set; }

    }
}
