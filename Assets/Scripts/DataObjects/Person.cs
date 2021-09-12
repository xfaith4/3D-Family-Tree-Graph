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
        public int dataBaseOwnerId;
        public int tribeArrayIndex;
        public string surName;
        public string givenName;
        public PersonGenderType gender;
        public int birthEventDate;   //TODO upgrade to EventDate in the future
        public int deathEventDate;   //TODO upgrade to EventDate in the future
        public bool isLiving;
        List<(PersonRelationshipType Relationship, Person RelatedPerson)> familyRelationships;

        public Person(int arrayIndex, int ownerId, PersonGenderType gender, string given, string surname, int birthYear, int deathYear)
        {
            this.tribeArrayIndex = arrayIndex;
            this.dataBaseOwnerId = ownerId;
            this.gender = gender;
            givenName = given;
            surName = surname;
            birthEventDate = birthYear;
            deathEventDate = deathYear;
            isLiving = birthYear != 0 && deathYear == 0;
        }
    }

}
