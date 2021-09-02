using Assets.Scripts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.DataObjects
{
    class Person
    {
        public string Name;
        public PersonGenderType Gender;
        public EventDate BirthEventDate;
        public EventDate DeathEventDate;
        public bool IsLiving;
        List<(PersonRelationshipType Relationship, Person RelatedPerson)> FamilyRelationships;
    }
}
