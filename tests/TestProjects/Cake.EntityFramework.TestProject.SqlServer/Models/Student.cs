using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cake.EntityFramework.TestProject.SqlServer.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string LastSchool { get; set; }

        public string EyeColor { get; set; }

        public int Age { get; set; }

        public int FatherAge { get; set; }

        public int MotherAge { get; set; }

        public string OtherThing { get; set; }

        public ICollection<Class> Class { get; set; }
    }
}
