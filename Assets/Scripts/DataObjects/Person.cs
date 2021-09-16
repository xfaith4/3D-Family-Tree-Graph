using Assets.Scripts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
		private int originalBirthEventDate;
        public int deathEventDate;   //TODO upgrade to EventDate in the future
		private int originalDeathEventDate;
        public bool isLiving;
		private bool originalIsLiving;
        public GameObject personNodeGameObject;
        List<(PersonRelationshipType Relationship, Person RelatedPerson)> familyRelationships;

        public Person(int arrayIndex, int ownerId, PersonGenderType gender, string given, string surname, int birthYear, int deathYear, bool isLiving)
        {
            this.tribeArrayIndex = arrayIndex;
            this.dataBaseOwnerId = ownerId;
            this.gender = gender;
            givenName = given;
            surName = surname;
            originalBirthEventDate = birthEventDate = birthYear;
            originalDeathEventDate = deathEventDate = deathYear;
            originalIsLiving = this.isLiving = isLiving;
        }

		public int FixUpAndReturnMarriageDate(int marriageEventDate)
        {
			// zero or Bogus MarriageEventDate
			return (marriageEventDate == 0 || marriageEventDate < birthEventDate ) ?
				birthEventDate + 20 : marriageEventDate;
		}

		public void FixUpDatesForViewingWithMarriageDate(int marriageEventDate)
        {
			//  OringinalBirthDate   OriginalDeathDate  IsLiving     Cleanup Action
			//  0	birth += delta	 0  death += delta   1 or 0        delta = marriageEventDate - (birth + 20)
			//  0   birth += delta   good
			// good                  0                            if death < marraige, then death = marriage + 5

			int fixedUpMarriageDate = FixUpAndReturnMarriageDate(marriageEventDate);

			var deltaFromMarriageAtTwenty = fixedUpMarriageDate - birthEventDate + 20;
			if (originalBirthEventDate == 0)
				birthEventDate += deltaFromMarriageAtTwenty;
			if (originalDeathEventDate == 0)
				deathEventDate += deltaFromMarriageAtTwenty;
			if (originalBirthEventDate != 0 && originalDeathEventDate == 0 && deathEventDate < fixedUpMarriageDate)
				deathEventDate = fixedUpMarriageDate + 5;
		}

		public void FixUpDatesForViewing()
        {
			var currentYear = DateTime.Now.Year;
			// DATA Clean up for a Visual Clean Display
			//  BirthDate   DeathDate  IsLiving calculatedAge     Cleanup Action
			//  0 (calc)	0			1        100             - Make them born 100 years ago, from today
			//  0 (calc)    0 (calc)    0		  90			 - Make them born 100 years ago, and died 10 years ago
			//  0 (calc)    given       0         90             - make them born 90 years before thier death
			//  0           given       1 (calc)                 - death date indicates not living, so flip Isliving
			if (birthEventDate == 0 || birthEventDate > deathEventDate)
			{
				if (deathEventDate == 0)
				{
					if (isLiving)
						birthEventDate = currentYear - 100;
					else
					{
						birthEventDate = currentYear - 100;
						deathEventDate = currentYear - 10;
					}
				}
				else
				{
					birthEventDate = deathEventDate - 90;
					if (isLiving)
						isLiving = false;
				}
			}
			// DATA Clean up for a Visual Clean Display
			//  BirthDate   DeathDate  IsLiving calculatedAge     Cleanup Action
			//  given       0           1 (calc)  min(100,today-given) - Set IsLiving to false is over 100
			//  given       0 (calc)    0         calc           - Calc death date to be (if older then 10): 10 years ago
			//                                                      if 1 to 10 years old: 1 year after birth
			//                                                      if 0 years old then deathDate = birthDate
			//														if more then 100 years old then cap deathdate at birthDate + 90
			else if (deathEventDate == 0)
			{
				if (isLiving && (currentYear - birthEventDate > 100))
					isLiving = false;
				if (!isLiving)
				{
					if ((currentYear - birthEventDate) > 100)
						deathEventDate = birthEventDate + 90;
					else if ((currentYear - birthEventDate) > 10)
						deathEventDate = currentYear - 10;
					else if ((currentYear - birthEventDate) > 0)
						deathEventDate = birthEventDate + 1;
					else deathEventDate = birthEventDate;
				}
			}
		}
    }
}
